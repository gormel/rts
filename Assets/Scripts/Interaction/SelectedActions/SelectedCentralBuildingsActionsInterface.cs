using System.Linq;
using Assets.Core.GameObjects.Final;
using Assets.Views;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Interaction
{
    class SelectedCentralBuildingsActionsInterface : MonoBehaviour
    {
        public UserInterface Interface;

        public TextMeshProUGUI WorkerCostText;

        void Update()
        {
            WorkerCostText.text = CentralBuilding.WorkerCost.ToString();
        }
        
        public void BuildWorker()
        {
            var views = Interface.Selected.OfType<CentralBuildingView>().OrderBy(v => v.Info.Queued);
            foreach (var buildingView in views)
            {
                buildingView.Orders.QueueWorker();
                break;
            }
        }
    }
}