using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Assets.Core.Game;
using Assets.Utils;
using Core.BotIntelligence;
using Grpc.Core;
using UnityEngine;

using PlayerConnectionListener = Assets.Utils.AsyncQueue<(PlayerConnection Info, System.Threading.Tasks.TaskCompletionSource<bool> SentCallback)>;

namespace Assets.Networking.Services
{
    class GameServiceImpl : GameService.GameServiceBase
    {
        public event Action<string, int> MessageRecived;
        public event Action GameStarted;
        

        private readonly Game mGame;
        private readonly Player mHostPlayer;
        private readonly IGameObjectFactory mEnemyFactory;
        private readonly IGameObjectFactory mAllyFactory;
        private readonly UnitySyncContext mSyncContext;
        private readonly IDictionary<string, UserState> mRegistredPlayers;
        private readonly ConcurrentDictionary<Guid, AsyncQueue<ChatMessage>> mChatListeners = new();
        private readonly ConcurrentDictionary<Guid, PlayerConnectionListener> mPlayerConnectionListeners = new();
        private readonly ConcurrentDictionary<string, Player> mConnectedPlayers = new();
        private readonly ConcurrentDictionary<string, BotPlayer> mBotPlayers = new();

        private readonly ConcurrentDictionary<string, TaskCompletionSource<Player>> mRegistredPlayerConnections = new();

        public GameServiceImpl(
            Game game, 
            Player hostPlayer, 
            IGameObjectFactory enemyFactory, 
            IGameObjectFactory allyFactory, 
            UnitySyncContext syncContext, 
            IDictionary<string, UserState> registredPlayers
            )
        {
            mGame = game;
            mHostPlayer = hostPlayer;
            mEnemyFactory = enemyFactory;
            mAllyFactory = allyFactory;
            mSyncContext = syncContext;
            mRegistredPlayers = registredPlayers;

            foreach (var registredPlayer in registredPlayers.Where(p => p.Value.ID != GameUtils.Nickname))
            {
                var tcs = new TaskCompletionSource<Player>();
                mRegistredPlayerConnections.AddOrUpdate(registredPlayer.Key, tcs, (n, t) => tcs);
            }
        }

        private async Task InitBotPlayers(IDictionary<string, UserState> botPlayers, CancellationToken token = default)
        {
            foreach (var botPlayer in botPlayers)
            {
                var botFactory = botPlayer.Value.Team == mHostPlayer.Team ? mAllyFactory : mEnemyFactory;
                var bot = new BotPlayer(mGame, botPlayer.Key, botFactory, botPlayer.Value.Team);
                mBotPlayers.AddOrUpdate(botPlayer.Key, n => bot, (n, b) => bot);
                await ReportPlayerState(botPlayer.Key, bot, true, token);
                var success = await mSyncContext.Execute(() => GameUtils.TryCreateBase(mGame, bot, out _), token);

                if (!success)
                {
                    if (mBotPlayers.TryRemove(botPlayer.Key, out _))
                        await ReportPlayerState(botPlayer.Key, bot, false, token);
                    
                    continue;
                }
                
                mGame.AddBotPlayer(bot);
            }
        }

        private async Task WaitAndStartGame(CancellationToken token = default)
        {
            await using (token.Register(() =>
            {
                foreach (var tcs in mRegistredPlayerConnections.Values)
                    tcs.SetCanceled();
            }))
            {
                var players = await Task.WhenAll(mRegistredPlayerConnections.Values.Select(s => s.Task));

                foreach (var player in players)
                    player.GameplayState = PlayerGameplateState.Playing;
            }

            mHostPlayer.GameplayState = PlayerGameplateState.Playing;
            
            mGame.Start();
            GameStarted?.Invoke();
        }

        public async Task InitBotPlayersAndStartGame(IDictionary<string, UserState> botPlayers, CancellationToken token = default)
        {
            await InitBotPlayers(botPlayers, token);
            await WaitAndStartGame(token);
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
                
                Team = player.Team,
                GameplayState = player.GameplayState,
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

        public override async Task ListenChat(ConnectRequest request, IServerStreamWriter<ChatMessage> responseStream, ServerCallContext context)
        {
            var key = Guid.NewGuid();
            try
            {
                var queue = new AsyncQueue<ChatMessage>();
                if (!mChatListeners.TryAdd(key, queue))
                    throw new Exception("Cannot register user state listener!");

                while (!context.CancellationToken.IsCancellationRequested)
                {
                    var message = await queue.DequeueAsync(context.CancellationToken);
                    context.CancellationToken.ThrowIfCancellationRequested();
                    await responseStream.WriteAsync(message);
                }

            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
            finally
            {
                mChatListeners.TryRemove(key, out _);
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

                foreach (var botPlayer in mBotPlayers)
                {
                    await responseStream.WriteAsync(new PlayerConnection()
                    {
                        Nickname = botPlayer.Key,
                        Player = CollectPlayerState(botPlayer.Value),
                        State = true,
                    });
                }

                foreach (var connectedPlayer in mConnectedPlayers)
                {
                    await responseStream.WriteAsync(new PlayerConnection()
                    {
                        Nickname = connectedPlayer.Key,
                        Player = CollectPlayerState(connectedPlayer.Value),
                        State = true,
                    });
                }

                while (!context.CancellationToken.IsCancellationRequested)
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
                throw;
            }
            finally
            {
                mPlayerConnectionListeners.TryRemove(key, out _);
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
                UserState registredPlayer;
                if (!mRegistredPlayers.TryGetValue(request.Nickname, out registredPlayer))
                    throw new Exception("Player is not registered.");

                var player = new Player(request.Nickname, registredPlayer.Team == mHostPlayer.Team ? mAllyFactory : mEnemyFactory, registredPlayer.Team);
                mGame.AddPlayer(player);
                
                if (mRegistredPlayerConnections.TryGetValue(request.Nickname, out var tcs))
                    tcs.SetResult(player);

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

                while (!context.CancellationToken.IsCancellationRequested)
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
