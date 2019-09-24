using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using Assets.Views;
using Assets.Views.Base;

namespace Assets.Interaction
{
    sealed class SelectedWarriorActionsInterface : SelectedUnitsActionsInterface<IRangedWarriorOrders, IRangedWarriorInfo, RangedWarriorView>
    {
        public void BeginAttack()
        {
            Interface.BeginAttack(SelectedViews);
        }
    }
}