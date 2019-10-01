using System.Linq;
using Assets.Core.GameObjects.Base;
using Assets.Views.Base;

namespace Assets.Interaction
{
    class SelectedWarriorActionsInterface<TOrders, TInfo, TView> : SelectedUnitsActionsInterface<TOrders, TInfo, TView>
        where TOrders : IWarriorOrders
        where TInfo : IWarriorInfo
        where TView : UnitView<TOrders, TInfo>
    {
        public void BeginAttack()
        {
            Interface.BeginAttack(SelectedViews);
        }
    }
}