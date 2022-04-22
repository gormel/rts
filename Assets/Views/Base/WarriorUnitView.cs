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

        public Color AgressiveMovementColor;

        protected override void Update()
        {
            base.Update();

            ShootEffect.SetActive(Info.IsAttacks);
            switch (Info.MovementState)
            {
                case WarriorMovementState.Common:
                    TargetLine.startColor = TargetLine.endColor = TargetLineMovementColor;
                    break;
                case WarriorMovementState.Agressive:
                    TargetLine.startColor = TargetLine.endColor = AgressiveMovementColor;
                    break;
            }
        }

        public override void OnEnemyRightClick(SelectableView view)
        {
            Orders.Attack(view.InfoBase.ID);
        }
    }
}
