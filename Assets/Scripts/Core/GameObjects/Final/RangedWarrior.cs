using System;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Utils;
using Assets.Core.Map;
using UnityEngine;

namespace Assets.Core.GameObjects.Final {

    interface IRangedWarriorInfo : IWarriorInfo
    {
    }

    interface IRangedWarriorOrders : IWarriorOrders
    {
    }

    internal class RangedWarrior : WarriorUnit, IRangedWarriorInfo, IRangedWarriorOrders
    {
        protected override float MaxHealthBase => 75;
        
        public override float ViewRadius => 5;
        protected override int ArmourBase => 1;
        public override float AttackRange => Player.Upgrades.UnitAttackRangeUpgrade.Calculate(3);
        public override float AttackSpeed => 2;
        protected override int DamageBase => 5;
        public override float Speed => 2f;
        
        public RangedWarrior(Game.Game game, IPathFinder pathFinder, Vector2 position)
            : base(game, pathFinder, position)
        {
        }
    }
}