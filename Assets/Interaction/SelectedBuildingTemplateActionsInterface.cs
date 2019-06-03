using System.Linq;
using Assets.Views;
using UnityEngine;

namespace Assets.Interaction
{
    class SelectedBuildingTemplateActionsInterface : MonoBehaviour
    {
        public UserInterface Interface;

        public void Cancel()
        {
            var views = Interface.Selected.OfType<BuildingTemplateView>();
            foreach (var buildingView in views)
            {
                buildingView.Cancel();
            }
        }
    }
}