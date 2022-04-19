using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Assets.Core.Game;
using Assets.Utils;
using Grpc.Core;
using UnityEngine;

using PlayerConnectionListener = Assets.Utils.AsyncQueue<(PlayerConnection Info, System.Threading.Tasks.TaskCompletionSource<bool> SentCallback)>;

namespace Assets.Networking.Services
{
    class GameServiceImpl : GameService.GameServiceBase
    {
        public event Action<string, int> MessageRecived;
        

        private readonly Game mGame;
        private readonly Player mHostPlayer;
        private readonly IGameObjectFactory mEnemyFactory;
        private readonly IGameObjectFactory mAllyFactory;
        private readonly UnitySyncContext mSyncContext;
        private readonly ConcurrentDictionary<Guid, AsyncQueue<ChatMessage>> mChatListeners = new ConcurrentDictionary<Guid, AsyncQueue<ChatMessage>>();
        private readonly ConcurrentDictionary<Guid, PlayerConnectionListener> mPlayerConnectionListeners = new ConcurrentDictionary<Guid, PlayerConnectionListener>();
        private readonly ConcurrentDictionary<string, IPlayerState> mConnectedPlayers = new ConcurrentDictionary<string, IPlayerState>();

        public GameServiceImpl(Game game, Player hostPlayer, IGameObjectFactory enemyFactory, IGameObjectFactory allyFactory, UnitySyncContext syncContext)
        {
            mGame = game;
            mHostPlayer = hostPlayer;
            mEnemyFactory = enemyFactory;
            mAllyFactory = allyFactory;
            mSyncContext = syncContext;
        }

        private PlayerState CollectPlayerState(IPlayerState player)
        {
            return new PlayerState()
            {
                ID = new ID() {Value = player.ID.ToString()},
                Money = player.Money,
                Limit = player.Limit,
                
                TurretBuildingAvaliable = player.TurretBuildingAvaliable,
                WarriorsLabBuildingAvaliable = player.WarriorsLabBuildingAvaliable,
                
                BuildingDefenceUpgradeAvaliable = player.BuildingDefenceUpgradeAvaliable,
                TurretAttackUpgradeAvaliable = player.TurretAttackUpgradeAvaliable,
                BuildingArmourUpgradeAvaliable = player.BuildingArmourUpgradeAvaliable,
                UnitArmourUpgradeAvaliable = player.UnitArmourUpgradeAvaliable,
                UnitDamageUpgradeAvaliable = player.UnitDamageUpgradeAvaliable,
                UnitAttackRangeUpgradeAvaliable = player.UnitAttackRangeUpgradeAvaliable,
                
                WorkerCost = player.WorkerCost,
                MeleeWarriorCost = player.MeleeWarriorCost,
                RangedWarriorCost = player.RangedWarriorCost,
                
                BarrakCost = player.BarrakCost,
                TurretCost = player.TurretCost,
                BuildersLabCost = player.BuildersLabCost,
                CentralBuildingCost = player.CentralBuildingCost,
                MiningCampCost = player.MiningCampCost,
                
                BuildingDefenceUpgradeCost = player.BuildingDefenceUpgradeCost,
                TurretAttackUpgradeCost = player.TurretAttackUpgradeCost,
                BuildingArmourUpgradeCost = player.BuildingArmourUpgradeCost,
                UnitArmourUpgradeCost = player.UnitArmourUpgradeCost,
                UnitDamageUpgradeCost = player.UnitDamageUpgradeCost,
                UnitAttackRangeUpgradeCost = player.UnitAttackRangeUpgradeCost,
                Team = player.Team,
            };
        }

        private GameState CollectGameState((IPlayerState player, Vector2 basePose) info, bool collectMap)
        {
            var result = new GameState();
            result.BasePos = info.basePose.ToGrpc();

            result.Player = CollectPlayerState(info.player);
            if (collectMap)
            {
                result.Map = new MapState()
                {
                    Width = mGame.Map.Width,
                    Lenght = mGame.Map.Length
                };

                for (int y = 0; y < mGame.Map.Length; y++)
                {
                    for (int x = 0; x < mGame.Map.Width; x++)
                    {
                        result.Map.Heights.Add(mGame.Map.Data.GetHeightAt(x, y));
                        result.Map.Objects.Add((int) mGame.Map.Data.GetMapObjectAt(x, y));
                    }
                }
            }

            return result;
        }

        public override async Task ListenChat(Empty request, IServerStreamWriter<ChatMessage> responseStream, ServerCallContext context)
        {
            var key = Guid.NewGuid();
            try
            {
                var queue = new AsyncQueue<ChatMessage>();
                if (!mChatListeners.TryAdd(key, queue))
                    throw new Exception("Cannot register user state listener!");

                while (true)
                {
                    var message = await queue.DequeueAsync(context.CancellationToken);
                    context.CancellationToken.ThrowIfCancellationRequested();
                    await responseStream.WriteAsync(message);
                }

            }
            catch (Exception e)
            {
                Debug.LogError(e);
                mChatListeners.TryRemove(key, out var q);
                throw;
            }
        }

        public void SendChatMessage(ChatMessage request)
        {
            MessageRecived?.Invoke(request.Nickname, request.StickerID);

            foreach (var queue in mChatListeners.Values)
                queue.Enqueue(request);
        }

        public override async Task<Empty> SendChatMessage(ChatMessage request, ServerCallContext context)
        {
            SendChatMessage(request);
            return new Empty();
        }

        public override async Task ListenPlayerConnections(ConnectRequest request, IServerStreamWriter<PlayerConnection> responseStream, ServerCallContext context)
        {
            var key = Guid.NewGuid();
            try
            {
                var queue = new PlayerConnectionListener();
                if (!mPlayerConnectionListeners.TryAdd(key, queue))
                    throw new Exception("Cannot register user state listener!");

                await responseStream.WriteAsync(new PlayerConnection()
                {
                    Nickname = GameUtils.Nickname,
                    Player = CollectPlayerState(mHostPlayer),
                    State = true,
                });
                
                foreach (var connectedPlayer in mConnectedPlayers)
                {
                    await responseStream.WriteAsync(new PlayerConnection()
                    {
                        Nickname = connectedPlayer.Key,
                        Player = CollectPlayerState(connectedPlayer.Value),
                        State = true,
                    });
                }
                
                while (true)
                {
                    var message = await queue.DequeueAsync(context.CancellationToken);
                    context.CancellationToken.ThrowIfCancellationRequested();
                    await responseStream.WriteAsync(message.Info);
                    message.SentCallback.TrySetResult(true);
                }

            }
            catch (Exception e)
            {
                Debug.LogError(e);
                mPlayerConnectionListeners.TryRemove(key, out var q);
                throw;
            }
        }

        private async Task ReportPlayerState(string nickname, IPlayerState player, bool state, CancellationToken token)
        {
            var connectionInfo = new PlayerConnection
            {
                Nickname = nickname,
                Player = CollectPlayerState(player),
                State = state,
            };

            var connectionCallbacks = mPlayerConnectionListeners.Values.Select(async l =>
            {
                var tcs = new TaskCompletionSource<bool>();
                using (token.Register(() => tcs.TrySetCanceled(token)))
                {
                    l.Enqueue((connectionInfo, tcs));
                    await tcs.Task;
                }
            });

            await Task.WhenAll(connectionCallbacks);
        }

        public override async Task ConnectAndListenState(ConnectRequest request, IServerStreamWriter<GameState> responseStream, ServerCallContext context)
        {
            try
            {
                int team;
                if (!GameUtils.RegistredPlayers.TryGetValue(request.Nickname, out team))
                    throw new Exception("Player is not registered.");

                var player = new Player(team == GameUtils.Team ? mAllyFactory : mEnemyFactory, team);

                mConnectedPlayers.AddOrUpdate(request.Nickname, player, (s, p) => player);
                await ReportPlayerState(request.Nickname, player, true, context.CancellationToken);
                
                var baseCreation = await mSyncContext.Execute(() =>
                {
                    var success = GameUtils.TryCreateBase(mGame, player, out var basePos);
                    return (basePos, success);
                });

                if (!baseCreation.success)
                    throw new Exception("Cannot create base.");
                
                context.CancellationToken.ThrowIfCancellationRequested();
                await responseStream.WriteAsync(CollectGameState((player, baseCreation.basePos), true));

                while (true)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();
                    await responseStream.WriteAsync(CollectGameState((player, baseCreation.basePos), false));
                    await Task.Delay(30, context.CancellationToken);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
            finally
            {
                if (mConnectedPlayers.TryRemove(request.Nickname, out var player))
                    await ReportPlayerState(request.Nickname, player, false, context.CancellationToken);
            }
        }
    }
}
