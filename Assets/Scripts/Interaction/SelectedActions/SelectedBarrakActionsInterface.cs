using System;
using System.Linq;
using Assets.Core.GameObjects.Final;
using Assets.Views;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Interaction {
    class SelectedBarrakActionsInterface : MonoBehaviour
    {
        public UserInterface Interface;

        public Text RangedCostText;
        public Text MeeleeCostText;
        public Text ArtilleryCostText;

        public Button QueueArtilleryButton;

        private void Update()
        {
            RangedCostText.text = Barrak.RangedWarriorCost.ToString();
            MeeleeCostText.text = Barrak.MeleeWarriorCost.ToString();
            ArtilleryCostText.text = Barrak.ArtilleryCost.ToString();
            QueueArtilleryButton.interactable = Interface.Root.Player.ArilleryOrderAvaliable;
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

        public void BuildArtillery()
        {
            var views = Interface.Selected.OfType<BarrakView>().OrderBy(v => v.Info.Queued);
            foreach (var buildingView in views)
            {
                buildingView.Orders.QueueArtillery();
                break;
            }
        }
    }
}