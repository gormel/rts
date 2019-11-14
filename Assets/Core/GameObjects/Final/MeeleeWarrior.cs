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
        public MeeleeWarrior(Game.Game game, IPathFinder pathFinder, Vector2 position) 
            : base(game, pathFinder, position)
        {
            Speed = 2.5f;
            MaxHealth = Health = 50;
            AttackRange = 1;
            AttackSpeed = 2;
            Damage = 3;
            ViewRadius = 4;
        }
    }
}