using Assets.Core.GameObjects.Final;
using Assets.Views.Base;
using Assets.Views.Utils;

namespace Assets.Views {
    sealed class RangedWarriorView : UnitView<IRangedWarriorOrders, IRangedWarriorInfo>
    {
        public override string Name => "Стрелятель";

        public override void OnEnemyRightClick(SelectableView view)
        {
            if (view is IInfoIdProvider)
                Orders.Attack(((IInfoIdProvider)view).ID);
        }
    }
}