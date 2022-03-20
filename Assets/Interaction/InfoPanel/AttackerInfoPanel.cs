using System.Linq;
using Assets.Core.GameObjects.Base;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Interaction.InfoPanel
{
    class AttackerInfoPanel : MonoBehaviour
    {
        public UserInterface Interface;
        
        public Text AttackText;
        public Text AttackSpeedText;
        public Text AttackRangeText;

        public virtual void Update()
        {
            var selected = Interface.FetchSelectedInfo<IAttackerInfo>().FirstOrDefault();
            if (selected != null)
            {
                AttackText.text = selected.Damage.ToString();
                AttackSpeedText.text = selected.AttackSpeed.ToString();
                AttackRangeText.text = selected.AttackRange.ToString();
            }
        }
    }
}