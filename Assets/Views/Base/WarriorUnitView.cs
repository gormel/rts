using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using Assets.Views.Utils;
using UnityEngine;

namespace Assets.Views.Base
{
    abstract class WarriorUnitView<TOrders, TInfo> : UnitView<TOrders, TInfo>
        where TOrders : IWarriorOrders
        where TInfo : IWarriorInfo
    {
        public GameObject ShootEffect;

        protected override void OnLoad()
        {
            base.OnLoad();
        }

        protected override void Update()
        {
            base.Update();

            ShootEffect.SetActive(Info.IsAttacks);
        }

        public override void OnEnemyRightClick(SelectableView view)
        {
            Orders.Attack(view.InfoBase.ID);
        }
    }
}
