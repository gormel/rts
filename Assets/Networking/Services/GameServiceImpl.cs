using System.Threading.Tasks;
using Assets.Core.Game;
using Grpc.Core;
using UnityEngine;

namespace Assets.Networking.Services
{
    class GameServiceImpl : GameService.GameServiceBase
    {
        private readonly Game mGame;
        private readonly IGameObjectFactory mServerFactory;

        public GameServiceImpl(Game game, IGameObjectFactory serverFactory)
        {
            mGame = game;
            mServerFactory = serverFactory;
        }

        private GameState CollectGameState(IPlayerState player)
        {
            lock(mGame)
            {
                var playerState = new PlayerState()
                {
                    ID = new ID() { Value = player.ID.ToString() },
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
            }
        }

        public override async Task ConnectAndListenState(Empty request, IServerStreamWriter<GameState> responseStream, ServerCallContext context)
        {
            var player = new Player(mServerFactory);
            mGame.AddPlayer(player);
            player.CreateWorker(new Vector2(Random.Range(0, 20), Random.Range(0, 20)));

            try
            {
                while (true)
                {
                    await responseStream.WriteAsync(CollectGameState(player));
                    await Task.Delay(16);
                }
            }
            catch (RpcException ex)
            {
                //disconnect player
            }
        }
    }
}
