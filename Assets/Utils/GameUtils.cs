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
        public static string Nickname { get; set; } = "Player";
        public static GameMode CurrentMode { get; set; } = GameMode.Server;
        public static IPAddress IP { get; set; } = IPAddress.Parse("127.0.0.1");
        public static int GamePort { get; set; } = 15656;
        public static int LobbyPort { get; set; } = 15657;

        static GameUtils()
        {
            Nickname += Random.Range(0, 10);
            if (PlayerPrefs.HasKey("Nickname"))
                Nickname = PlayerPrefs.GetString("Nickname");
            if (PlayerPrefs.HasKey("IP"))
                IP = IPAddress.Parse(PlayerPrefs.GetString("IP"));
            //save & restore settings;
        }

        public static void SaveSettings()
        {
            PlayerPrefs.SetString("Nickname", Nickname);
            PlayerPrefs.GetString("IP", IP.ToString());
            PlayerPrefs.Save();
        }

        public static Vector3 GetPosition(Vector2 flatPosition, IMapData mapData)
        {
            return new Vector3(flatPosition.x, mapData.GetHeightAt(flatPosition), flatPosition.y);
        }

        public static Vector2 GetFlatPosition(Vector3 position)
        {
            return new Vector2(position.x, position.z);
        }

        private static bool HasCrystal(Vector2 pos, int innerRadius, int outerRadius, IMapData mapData)
        {
            for (int dx = -outerRadius; dx < outerRadius; dx++)
            {
                for (int dy = -outerRadius; dy < outerRadius; dy++)
                {
                    var x = (int)pos.x + dx;
                    var y = (int)pos.y + dy;

                    if (x < 0 || x >= mapData.Width)
                        continue;

                    if (y < 0 || y >= mapData.Length)
                        continue;

                    if (Mathf.Abs(dx) < innerRadius || Mathf.Abs(dy) < innerRadius)
                        continue;

                    if (mapData.GetMapObjectAt(x, y) == MapObject.Crystal)
                        return true;
                }
            }
            return false;
        }

        public static Vector2 CreateBase(Game game, Player player)
        {
            player.Money.Store(17000);
            var size = CentralBuilding.BuildingSize + Vector2.one * 2;
            var pos = new Vector2(Random.Range(0, game.Map.Width), Random.Range(0, game.Map.Length));
            while (!game.GetIsAreaFree(pos, size) || !HasCrystal(pos + size / 2, (int)(size.x / 2), (int)size.x, game.Map.Data))
                pos = new Vector2(Random.Range(0, game.Map.Width), Random.Range(0, game.Map.Length));

            player.CreateCentralBuilding(pos + Vector2.one).ContinueWith(t => game.PlaceObject(t.Result));
            //player.CreateBarrak(pos + Vector2.one).ContinueWith(t => game.PlaceObject(t.Result));
            player.CreateWorker(pos).ContinueWith(t => game.PlaceObject(t.Result));
            //player.CreateWorker(pos + Vector2.right).ContinueWith(t => game.PlaceObject(t.Result));
            //player.CreateMeeleeWarrior(pos + Vector2.right).ContinueWith(t => game.PlaceObject(t.Result));
            return pos;
        }
    }
}
