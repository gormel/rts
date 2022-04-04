using System.Collections.Generic;
using System.Linq;
using Assets.Core.GameObjects;
using Assets.Core.GameObjects.Base;
using Assets.Views;
using Assets.Views.Base;
using UnityEngine;

namespace Assets.Interaction
{
    abstract class SelectedUnitsActionsInterface: MonoBehaviour
    {
        public UserInterface Interface;

        public void BeginGoTo()
        {
            Interface.BeginGoTo(Interface.FetchSelectedOrders<IUnitOrders>());
        }

        public void Stop()
        {
            foreach (var orders in Interface.FetchSelectedOrders<IUnitOrders>()) 
                orders.Stop();
        }
    }
}