using System.Collections.Generic;
using System.Linq;
using Assets.Core.GameObjects;
using Assets.Core.GameObjects.Base;
using Assets.Views;
using Assets.Views.Base;
using UnityEngine;

namespace Assets.Interaction
{
    abstract class SelectedUnitsActionsInterface<TOrders, TInfo, TView> : MonoBehaviour 
        where TOrders : IUnitOrders
        where TInfo : IUnitInfo
        where TView : UnitView<TOrders, TInfo>
    {
        public UserInterface Interface;

        protected IEnumerable<TView> SelectedViews => Interface.Selected.OfType<TView>();

        public void BeginGoTo()
        {
            Interface.BeginGoTo(SelectedViews);
        }
    }
}