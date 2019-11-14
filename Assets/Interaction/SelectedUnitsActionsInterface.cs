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

        protected IEnumerable<T> FetchSelectedOrders<T>() where T : class, IUnitOrders
        {
            return Interface.Selected.Select(v => v.OrdersBase as T).Where(o => o != null);
        }

        public void BeginGoTo()
        {
            Interface.BeginGoTo(FetchSelectedOrders<IUnitOrders>());
        }
    }
}