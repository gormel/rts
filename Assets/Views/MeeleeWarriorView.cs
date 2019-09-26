using Assets.Core.GameObjects.Final;
using Assets.Views.Base;
using UnityEngine;

namespace Assets.Views
{
    sealed class MeeleeWarriorView : UnitView<IMeeleeWarriorOrders, IMeeleeWarriorInfo>
    {
        public override string Name => "Колотитель";

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