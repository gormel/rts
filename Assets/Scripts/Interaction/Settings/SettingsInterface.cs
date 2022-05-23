using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Interaction.Settings
{
    public class SettingsInterface : MonoBehaviour
    {
        public TextMeshProUGUI CameraSpeedValue;
        public TextMeshProUGUI GameSpeedValue;
        public TextMeshProUGUI SelfHealthViewValue;
        public TextMeshProUGUI AllyHealthViewValue;
        public TextMeshProUGUI EnemyHealthViewValue;

        public Scrollbar CameraSpeedScrollbar;
        public Scrollbar GameSpeedScrollbar;

        private void OnEnable()
        {
            CameraSpeedScrollbar.value = GetPercent(global::Settings.Settings.CameraSpeed, global::Settings.Settings.CameraSpeedBase);
            GameSpeedScrollbar.value = GetPercent(global::Settings.Settings.GameSpeed, global::Settings.Settings.GameSpeedBase);
        }

        private void Update()
        {
            CameraSpeedValue.text = global::Settings.Settings.CameraSpeed.ToString("0.#");
            GameSpeedValue.text = global::Settings.Settings.GameSpeed.ToString("0.#");
        }

        public void SaveSettings()
        {
            global::Settings.Settings.Save();
        }

        public void SetCameraSpeed(float value)
        {
            if (!gameObject.activeInHierarchy)
                return;
            
            var basic = global::Settings.Settings.CameraSpeedBase;
            global::Settings.Settings.CameraSpeed = GetBaseBasedValue(value, basic);
        }

        public void SetGameSpeed(float value)
        {
            if (!gameObject.activeInHierarchy)
                return;

            var basic = global::Settings.Settings.GameSpeedBase;
            global::Settings.Settings.GameSpeed = GetBaseBasedValue(value, basic);
        }
        
        private float GetBaseBasedValue(float percent, float baseValue)
            => baseValue / 2 * (1 - percent) + baseValue * 2 * percent;

        private float GetPercent(float value, float baseValue)
            => (value - baseValue / 2) / (3 * baseValue / 2);
    }
}