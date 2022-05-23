using System.Linq;
using Assets.Views;
using UnityEngine;

namespace Assets.Interaction
{
    class SelectedMiningCampActionsInterface : SelectedBuildingActionsInterface
    {
        public void FreeWorker()
        {
            var views = Interface.Selected.OfType<MiningCampView>();
            foreach (var campView in views)
            {
                campView.Orders.FreeWorker();
            }
        }

        public void CollectWorkers()
        {
            var views = Interface.Selected.OfType<MiningCampView>();
            foreach (var campView in views)
            {
                campView.Orders.CollectWorkers();
            }
        }
    }
}