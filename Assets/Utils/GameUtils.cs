using System.Net;
using Assets.Core.Game;
using Assets.Core.GameObjects.Final;
using Assets.Core.Map;
using UnityEngine;

namespace Assets.Utils
{
    enum GameMode
    {
        Server,
        Client
    }
    static class GameUtils
    {
        public static GameMode CurrentMode { get; set; } = GameMode.Server;
        public static IPAddress IP { get; } = IPAddress.Parse("127.0.0.1");
        public static int Port { get; } = 15656;

        static GameUtils()
        {
            //read cmd or editor config & set CurrentMode
        }

        public static Vector3 GetPosition(Vector2 flatPosition, IMapData mapData)
        {
            return new Vector3(flatPosition.x, mapData.GetHeightAt(flatPosition), flatPosition.y);
        }

        public static Vector2 GetFlatPosition(Vector3 position)
        {
            return new Vector2(position.x, position.z);
        }

        public static Vector2 CreateBase(Game game, Player player)
        {
            var size = CentralBuilding.BuildingSize + Vector2.one * 2;
            var pos = new Vector2(Random.Range(0, game.Map.Width), Random.Range(0, game.Map.Length));
            while (!game.GetIsAreaFree(pos, size))
                pos = new Vector2(Random.Range(0, game.Map.Width), Random.Range(0, game.Map.Length));

            player.CreateCentralBuilding(pos + Vector2.one).ContinueWith(t => game.PlaceObject(t.Result));
            player.CreateWorker(pos).ContinueWith(t => game.PlaceObject(t.Result));
            return pos;
        }
    }
}
