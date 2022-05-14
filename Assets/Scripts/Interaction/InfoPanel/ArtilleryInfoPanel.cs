using System;
using System.Linq;
using Core.GameObjects.Final;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Interaction.InfoPanel
{
    class ArtilleryInfoPanel : MonoBehaviour
    {
        public UserInterface Interface;

        public TextMeshProUGUI MoveSpeedText;
        public TextMeshProUGUI ArmourText;
        public TextMeshProUGUI MissileRangeText;
        public TextMeshProUGUI MissileDamageText;

        private void Update()
        {
            var artillery = Interface.FetchSelectedInfo<IArtilleryInfo>().SingleOrDefault();
            if (artillery != null)
            {
                MoveSpeedText.text = artillery.Speed.ToString("0.##");
                ArmourText.text = artillery.Armour.ToString();
                MissileRangeText.text = artillery.LaunchRange.ToString("0.##");
                MissileDamageText.text = artillery.MissileDamage.ToString("0.##");
            }
        }
    }
}