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

        public MeeleeWarrior(Game.Game game, IPathFinder pathFinder, Vector2 position) 
            : base(game, pathFinder, position)
        {
        }

        public override void OnAddedToGame()
        {
            Speed = 2.5f;
            AttackRange = 1;
            AttackSpeed = 2;
            Damage = 3;
            ViewRadius = 4;
            
            base.OnAddedToGame();
        }
    }
}