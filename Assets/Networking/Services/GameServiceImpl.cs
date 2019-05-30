using System;
using System.Threading.Tasks;
using Assets.Core.Game;
using Grpc.Core;
using UnityEngine;
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

        private Task<GameState> CollectGameState(IPlayerState player)
        {
            return mSyncContext.Execute(() =>
            {
                var playerState = new PlayerState()
                {
                    ID = new ID() {Value = player.ID.ToString()},
                    Money = player.Money
                };

                var mapState = new MapState()
                {
                    Width = mGame.Map.Width,
                    Lenght = mGame.Map.Length
                };

                for (int y = 0; y < mGame.Map.Length; y++)
                for (int x = 0; x < mGame.Map.Width; x++)
                    mapState.Heights.Add(mGame.Map.Data.GetHeightAt(x, y));

                return new GameState()
                {
                    Player = playerState,
                    Map = mapState
                };
            });
        }

        public override async Task ConnectAndListenState(Empty request, IServerStreamWriter<GameState> responseStream, ServerCallContext context)
        {
            try
            {
                var player = await mSyncContext.Execute(() =>
                {
                    var pl = new Player(mServerFactory);
                    mGame.AddPlayer(pl);
                    pl.CreateWorker(new Vector2(10, 10));
                    return pl;
                });

                while (true)
                {
                    await responseStream.WriteAsync(await CollectGameState(player));
                    await Task.Delay(16);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }
    }
}
