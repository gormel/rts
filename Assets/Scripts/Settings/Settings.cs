using Assets.Utils.StaticSaveLoad;
using UnityEngine;

namespace Settings
{
    enum ShowHealthState
    {
        SelectedOnly,
        DamagedOnly,
        Always,
    }
    
    static class Settings
    {
        public const float CameraSpeedBase = 5;
        public const float GameSpeedBase = 1;
        [SaveProperty]
        public static float CameraSpeed { get; set; } = CameraSpeedBase;
        [SaveProperty]
        public static float GameSpeed { get; set; } = GameSpeedBase;

        [SaveProperty]
        public static ShowHealthState SelfHealthState { get; set; } = ShowHealthState.DamagedOnly;
        [SaveProperty]
        public static ShowHealthState AllyHealthState { get; set; } = ShowHealthState.DamagedOnly;
        [SaveProperty]
        public static ShowHealthState EnemyHealthState { get; set; } = ShowHealthState.DamagedOnly;

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