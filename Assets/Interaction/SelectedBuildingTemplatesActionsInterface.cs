using System.Linq;
using Assets.Views;
using UnityEngine;

namespace Assets.Interaction
{
    class SelectedBuildingTemplatesActionsInterface : MonoBehaviour
    {
        public UserInterface Interface;

        public void Cancel()
        {
            var views = Interface.Selected.OfType<BuildingTemplateView>();
            foreach (var templateView in views)
                templateView.Cancel();
        }
    }
}