using System;
using System.Linq;
using System.Threading.Tasks;
using Assets.Core.BehaviorTree;
using Assets.Core.GameObjects.Utils;
using Assets.Core.Map;
using UnityEngine;

namespace Assets.Core.GameObjects.Base
{
    enum Strategy
    {
        Aggressive,
        Defencive,
        Idle
    }

    interface IWarriorInfo : IUnitInfo
    {
        bool IsAttacks { get; }
        float AttackRange { get; }
        float AttackSpeed { get; }
        int Damage { get; }
        Strategy Strategy { get; }
    }

    interface IWarriorOrders : IUnitOrders
    {
        Task Attack(Guid targetID);
        Task SetStrategy(Strategy strategy);
    }

    abstract class WarriorUnit : Unit, IWarriorInfo, IWarriorOrders
    {
        public bool IsAttacks { get; protected set; }
        public float AttackRange { get; protected set; }
        public float AttackSpeed { get; protected set; }
        public int Damage { get; protected set; }
        public Strategy Strategy { get; private set; }

        protected WarriorUnit(Game.Game game, IPathFinder pathFinder, Vector2 position)
            : base(game, pathFinder, position)
        {
            Strategy = Strategy.Aggressive;
        }

        public Task Attack(Guid targetID)
        {
            if (targetID == ID)
                return Task.CompletedTask;

            return Task.CompletedTask;
        }

        public Task SetStrategy(Strategy strategy)
        {
            Strategy = strategy;
            return Task.CompletedTask;
        }

        private RtsGameObject FindEnemy(float radius)
        {
            return Game.QueryObjects(Position, radius)
                .OrderBy(go => go.MaxHealth)
                .ThenBy(go => Vector2.Distance(Position, PositionOf(go)))
                .FirstOrDefault(go => go.PlayerID != PlayerID);
        }

        private static Vector2 PositionOf(RtsGameObject target)
        {
            if (target is Building)
                return ((Building) target).Size / 2 + target.Position;

            return target.Position;
        }

        private float DistanceTo(RtsGameObject target)
        {
            if (target is Building)
            {
                var p = PositionOf(target);
                var s = ((Building) target).Size;
                var dx = Math.Max(Math.Abs(Position.x - p.x) - s.x / 2, 0);
                var dy = Math.Max(Math.Abs(Position.y - p.y) - s.y / 2, 0);

                return dx * dx + dy * dy;
            }

            return Vector2.Distance(Position, PositionOf(target));
        }
    }
}