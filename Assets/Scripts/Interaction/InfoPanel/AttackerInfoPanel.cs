using System.Linq;
using Assets.Core.GameObjects.Base;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Interaction.InfoPanel
{
    class AttackerInfoPanel : MonoBehaviour
    {
        public UserInterface Interface;
        
        public TextMeshProUGUI AttackText;
        public TextMeshProUGUI AttackSpeedText;
        public TextMeshProUGUI AttackRangeText;
        public TextMeshProUGUI ArmourText;

        public virtual void Update()
        {
            var selected = Interface.FetchSelectedInfo<IAttackerInfo>().FirstOrDefault();
            if (selected != null)
            {
                AttackText.text = selected.Damage.ToString();
                AttackSpeedText.text = selected.AttackSpeed.ToString();
                AttackRangeText.text = selected.AttackRange.ToString();
                ArmourText.text = selected.Armour.ToString();
            }
        }
    }
}