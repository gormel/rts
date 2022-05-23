using Assets.Utils.StaticSaveLoad;
using UnityEngine;

namespace Settings
{
    static class Settings
    {
        [SaveProperty]
        public static float CameraSpeed { get; set; } = 1f;
        [SaveProperty]
        public static float GameSpeed { get; set; } = 1f;

        [SaveProperty]
        public static int SelfHealthState { get; set; } = 0;
        [SaveProperty]
        public static int AllyHealthState { get; set; } = 0;
        [SaveProperty]
        public static int EnemyHealthState { get; set; } = 0;

        static Settings()
        {
            SaveStatics.Load(typeof(Settings));
        }

        public static void Save()
        {
            SaveStatics.Save(typeof(Settings));
        }
    }
}