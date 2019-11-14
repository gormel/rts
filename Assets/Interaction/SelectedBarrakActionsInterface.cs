using System.Linq;
using Assets.Views;
using UnityEngine;

namespace Assets.Interaction {
    class SelectedBarrakActionsInterface : MonoBehaviour
    {
        public UserInterface Interface;

        public void BuildRanged()
        {
            var views = Interface.Selected.OfType<BarrakView>();
            foreach (var buildingView in views)
            {
                buildingView.Orders.QueueRanged();
            }
        }

        public void BuildMeelee()
        {
            var views = Interface.Selected.OfType<BarrakView>();
            foreach (var buildingView in views)
            {
                buildingView.Orders.QueueMeelee();
            }
        }
    }
}