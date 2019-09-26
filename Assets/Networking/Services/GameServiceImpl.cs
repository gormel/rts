using System;
using System.Threading.Tasks;
using Assets.Core.Game;
using Assets.Utils;
using Grpc.Core;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;
using Random = UnityEngine.Random;

namespace Assets.Networking.Services
{
    class GameServiceImpl : GameService.GameServiceBase
    {
        private readonly Game mGame;
        private readonly IGameObjectFactory mServerFactory;
        private readonly UnitySyncContext mSyncContext;

        public GameServiceImpl(Game game, IGameObjectFactory serverFactory, UnitySyncContext syncContext)
        {
            mGame = game;
            mServerFactory = serverFactory;
            mSyncContext = syncContext;
        }

        private GameState CollectGameState((IPlayerState player, Vector2 basePose) info, bool collectMap)
        {
            var result = new GameState();
            result.BasePos = info.basePose.ToGrpc();

            result.Player = new PlayerState()
            {
                ID = new ID() {Value = info.player.ID.ToString()},
                Money = info.player.Money
            };

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

        public override async Task ConnectAndListenState(Empty request, IServerStreamWriter<GameState> responseStream, ServerCallContext context)
        {
            try
            {
                var player = await mSyncContext.Execute(() =>
                {
                    var pl = new Player(mServerFactory);
                    pl.Money.Store(100);
                    mGame.AddPlayer(pl);
                    var basePos = GameUtils.CreateBase(mGame, pl);
                    return (pl, basePos);
                });

                context.CancellationToken.ThrowIfCancellationRequested();
                await responseStream.WriteAsync(CollectGameState(player, true));

                while (true)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();
                    await responseStream.WriteAsync(CollectGameState(player, false));
                    await Task.Delay(30, context.CancellationToken);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }
    }
}
