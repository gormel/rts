using Assets.Core.GameObjects.Base;
using Assets.Views;
using Assets.Views.Base;

namespace Assets.Interaction
{
    sealed class SelectedWarriorActionsInterface : SelectedUnitsActionsInterface<IWarriorOrders, IWarriorInfo, UnitView<IWarriorOrders, IWarriorInfo>>
    {
        public void BeginAttack()
        {
            Interface.BeginAttack(SelectedViews);
        }
    }
}