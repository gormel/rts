using System.Linq;
using Assets.Views;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Interaction
{
    class SelectedCentralBuildingsActionsInterface : MonoBehaviour
    {
        public UserInterface Interface;

        public Text WorkerCostText;

        void Update()
        {
            WorkerCostText.text = Interface.Root.Player.WorkerCost.ToString();
        }
        
        public void BuildWorker()
        {
            var views = Interface.Selected.OfType<CentralBuildingView>();
            foreach (var buildingView in views)
            {
                buildingView.Orders.QueueWorker();
            }
        }
    }
}