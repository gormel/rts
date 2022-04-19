using System;
using System.Linq;
using Assets.Views;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Interaction {
    class SelectedBarrakActionsInterface : MonoBehaviour
    {
        public UserInterface Interface;

        public Text RangedCostText;
        public Text MeeleeCostText;

        private void Update()
        {
            var player = Interface.Root.Player;
            RangedCostText.text = player.RangedWarriorCost.ToString();
            MeeleeCostText.text = player.MeleeWarriorCost.ToString();
        }

        public void BuildRanged()
        {
            var views = Interface.Selected.OfType<BarrakView>().OrderBy(v => v.Info.Queued);
            foreach (var buildingView in views)
            {
                buildingView.Orders.QueueRanged();
                break;
            }
        }

        public void BuildMeelee()
        {
            var views = Interface.Selected.OfType<BarrakView>().OrderBy(v => v.Info.Queued);
            foreach (var buildingView in views)
            {
                buildingView.Orders.QueueMeelee();
                break;
            }
        }
    }
}