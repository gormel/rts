using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using Assets.Core.Game;
using Assets.Core.GameObjects.Final;
using Assets.Core.Map;
using Assets.Utils.StaticSaveLoad;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Utils
{
    enum GameMode
    {
        Server,
        Client
    }

    

    static class GameUtils
    {
        [SaveProperty]
        public static string Nickname { get; set; } = "Player";
        public static GameMode CurrentMode { get; set; } = GameMode.Server;
        
        [SaveProperty]
        private static string IPString { get; set; } = "127.0.0.1";
        
        public static IPAddress IP { get; set; }
        public static int GamePort { get; set; } = 15656;
        public static int LobbyPort { get; set; } = 15657;
        public static int Team { get; set; }
        public static ConcurrentDictionary<string, UserState> BotPlayers { get; } = new();
        public static ConcurrentDictionary<string, UserState> RegistredPlayers { get; } = new();


        public const int MaxPlayers = 6;

        static GameUtils()
        {
            Nickname += Random.Range(0, 10);
            SaveStatics.Load(typeof(GameUtils));
            IP = IPAddress.Parse(IPString);
        }

        public static void SaveSettings()
        {
            IPString = IP.ToString();
            SaveStatics.Save(typeof(GameUtils));
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

        public static bool TryCreateBase(Game game, Player player, out Vector2 basePos)
        {
            player.Money.Store(170);
            if (!game.Map.TryAllocateBase(out basePos))
                return false;

            var relativeCentralBuildingPosition = Vector2.one;
            player.CreateCentralBuilding(basePos + relativeCentralBuildingPosition).ContinueWith(t => game.PlaceObject(t.Result));
            player.CreateWorker(basePos).ContinueWith(t => game.PlaceObject(t.Result));
            basePos += relativeCentralBuildingPosition + CentralBuilding.BuildingSize / 2;
            return true;
        }
    }
}
