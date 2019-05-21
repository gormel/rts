using System.Linq;
using Assets.Views;
using UnityEngine;

namespace Assets.Interaction
{
    class SelectedCentralBuildingsActionsInterface : MonoBehaviour
    {
        public UserInterface Interface;

        public void BuildWorker()
        {
            var views = Interface.Selected.OfType<CentralBuildingView>();
            foreach (var buildingView in views)
            {
                buildingView.QueueWorker();
            }
        }
    }
}