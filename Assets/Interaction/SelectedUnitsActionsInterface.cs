using System.Collections.Generic;
using System.Linq;
using Assets.Core.GameObjects;
using Assets.Core.GameObjects.Base;
using Assets.Views;
using Assets.Views.Base;
using UnityEngine;

namespace Assets.Interaction
{
    abstract class SelectedUnitsActionsInterface<TModel, TView> : MonoBehaviour where TModel : Unit where TView : UnitView<TModel>
    {
        public UserInterface Interface;

        protected IEnumerable<TView> SelectedViews => Interface.Selected.OfType<TView>();

        public void BeginGoTo()
        {
            Interface.BeginGoTo(SelectedViews);
        }
    }
}