using System.Linq;
using Assets.Core.GameObjects.Final;
using UnityEngine;

namespace Assets.Interaction
{
    sealed class SelectedTurretActionsInterface : SelectedBuildingActionsInterface
    {
        public void BeginAttack()
        {
            Interface.BeginAttack(Interface.FetchSelectedOrders<ITurretOrders>());
        }

        public void Stop()
        {
            foreach (var order in Interface.FetchSelectedOrders<ITurretOrders>()) 
                order.Stop();
        }
    }
}