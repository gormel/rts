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
        public RangedWarrior(Game.Game game, IPathFinder pathFinder, Vector2 position)
            : base(game, pathFinder, position)
        {
            Speed = 2;
            MaxHealth = Health = 75;
            AttackRange = 3;
            AttackSpeed = 2;
            Damage = 5;
            ViewRadius = 5;
        }
    }
}