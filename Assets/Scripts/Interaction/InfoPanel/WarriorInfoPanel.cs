using System.Linq;
using Assets.Core.GameObjects.Base;
using TMPro;
using UnityEngine.UI;

namespace Assets.Interaction.InfoPanel
{
    class WarriorInfoPanel : AttackerInfoPanel
    {
        public TextMeshProUGUI MoveSpeedText;

        public override void Update()
        {
            base.Update();
            
            var selected = Interface.FetchSelectedInfo<IWarriorInfo>().FirstOrDefault();
            if (selected != null)
            {
                MoveSpeedText.text = selected.Speed.ToString("0.00");
            }
        }
    }
}