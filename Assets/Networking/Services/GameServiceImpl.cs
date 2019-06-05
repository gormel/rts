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

        private GameState CollectGameState((IPlayerState player, Vector2 basePose) info)
        {
            var playerState = new PlayerState()
            {
                ID = new ID() {Value = info.player.ID.ToString()},
                Money = info.player.Money
            };

            var mapState = new MapState()
            {
                Width = mGame.Map.Width,
                Lenght = mGame.Map.Length
            };

            for (int y = 0; y < mGame.Map.Length; y++)
            {
                for (int x = 0; x < mGame.Map.Width; x++)
                {
                    mapState.Heights.Add(mGame.Map.Data.GetHeightAt(x, y));
                    mapState.Objects.Add((int)mGame.Map.Data.GetMapObjectAt(x, y));
                }
            }

            return new GameState()
            {
                Player = playerState,
                Map = mapState,
                BasePos = info.basePose.ToGrpc()
            };
        }

        public override async Task ConnectAndListenState(Empty request, IServerStreamWriter<GameState> responseStream, ServerCallContext context)
        {
            try
            {
                var player = await mSyncContext.Execute(() =>
                {
                    var pl = new Player(mServerFactory);
                    pl.Money.Store(10000);
                    mGame.AddPlayer(pl);
                    var basePos = GameUtils.CreateBase(mGame, pl);
                    return (pl, basePos);
                });

                while (true)
                {
                    await responseStream.WriteAsync(CollectGameState(player));
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
