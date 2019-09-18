using Assets.Core.GameObjects.Final;
using Assets.Views.Base;
using Assets.Views.Utils;
using UnityEngine;

namespace Assets.Views {
    sealed class RangedWarriorView : UnitView<IRangedWarriorOrders, IRangedWarriorInfo>
    {
        public override string Name => "Стрелятель";

        public GameObject ShootEffect;

        protected override void Update()
        {
            base.Update();

            ShootEffect.SetActive(Info.IsAttacks);
        }

        public override void OnEnemyRightClick(SelectableView view)
        {
            if (view is IInfoIdProvider)
                Orders.Attack(((IInfoIdProvider)view).ID);
        }
    }
}