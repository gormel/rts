using System.Linq;
using Assets.Core.GameObjects.Final;
using UnityEngine;

namespace Assets.Interaction
{
    sealed class SelectedTurretActionsInterface : MonoBehaviour
    {
        public UserInterface Interface;

        public void BeginAttack()
        {
            Interface.BeginTurretAttack(Interface.FetchSelectedOrders<ITurretOrders>());
        }

        public void Stop()
        {
            foreach (var order in Interface.FetchSelectedOrders<ITurretOrders>()) 
                order.Stop();
        }
    }
}