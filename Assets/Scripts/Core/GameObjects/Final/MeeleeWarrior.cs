using Assets.Core.GameObjects.Base;
using Assets.Core.Map;
using UnityEngine;

namespace Assets.Core.GameObjects.Final
{
    interface IMeeleeWarriorInfo : IWarriorInfo
    {
    }

    interface IMeeleeWarriorOrders : IWarriorOrders
    {
    }

    internal class MeeleeWarrior : WarriorUnit, IMeeleeWarriorInfo, IMeeleeWarriorOrders
    {
        protected override float MaxHealthBase => 50;
        public override float ViewRadius => 4;
        protected override int ArmourBase => 1;
        public override float AttackRange => 1;
        public override float AttackSpeed => 2;
        protected override int DamageBase => 3;
        public override float Speed => 2.5f;

        public MeeleeWarrior(Game.Game game, IPathFinder pathFinder, Vector2 position) 
            : base(game, pathFinder, position)
        {
        }
    }
}