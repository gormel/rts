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
        private class CheckStrategyLeaf : IBTreeLeaf
        {
            private readonly WarriorUnit mUnit;
            private readonly Strategy mStrategy;

            public CheckStrategyLeaf(WarriorUnit unit, Strategy strategy)
            {
                mUnit = unit;
                mStrategy = strategy;
            }

            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                return mUnit.Strategy == mStrategy ? BTreeLeafState.Successed : BTreeLeafState.Failed;
            }
        }

        private class LockTargetLeaf : IBTreeLeaf
        {
            private readonly WarriorUnit mUnit;
            private readonly bool mForce;

            public LockTargetLeaf(WarriorUnit unit, bool force)
            {
                mUnit = unit;
                mForce = force;
            }

            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                if (mUnit.mTarget != null && !mForce)
                    return BTreeLeafState.Failed;

                mUnit.mTarget = FindEnemy(mUnit.AttackRange);
                return mUnit.mTarget == null ? BTreeLeafState.Failed : BTreeLeafState.Successed;
            }

            private RtsGameObject FindEnemy(float radius)
            {
                return mUnit.Game.QueryObjects(mUnit.Position, radius)
                    .OrderBy(go => go.MaxHealth)
                    .ThenBy(go => Vector2.Distance(mUnit.Position, PositionOf(go)))
                    .FirstOrDefault(go => go.PlayerID != mUnit.PlayerID);
            }
        }

        private class GoToTargetLeaf : IBTreeLeaf
        {
            private readonly WarriorUnit mUnit;

            public GoToTargetLeaf(WarriorUnit unit)
            {
                mUnit = unit;
            }

            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                if (mUnit.mTarget == null)
                    return BTreeLeafState.Failed;

                mUnit.PathFinder.SetTarget(PositionOf(mUnit.mTarget), mUnit.Game.Map.Data);
                if (mUnit.DistanceTo(mUnit.mTarget) <= mUnit.AttackRange)
                {
                    mUnit.PathFinder.Stop();
                    return BTreeLeafState.Successed;
                }

                return BTreeLeafState.Processing;
            }
        }

        private class KillTargetLeaf : IBTreeLeaf
        {
            private readonly WarriorUnit mUnit;
            private TimeSpan mAttackTimer;

            public KillTargetLeaf(WarriorUnit unit)
            {
                mUnit = unit;
                mAttackTimer = TimeSpan.Zero;
            }

            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                mUnit.IsAttacks = false;
                if (mUnit.mTarget == null)
                    return BTreeLeafState.Failed;

                if (mUnit.DistanceTo(mUnit.mTarget) > mUnit.AttackRange)
                    return BTreeLeafState.Processing;

                mUnit.PathFinder.SetLookAt(PositionOf(mUnit.mTarget), mUnit.Game.Map.Data);
                mUnit.IsAttacks = true;
                mAttackTimer -= deltaTime;
                if (mAttackTimer > TimeSpan.Zero)
                    return BTreeLeafState.Processing;

                mUnit.mTarget.Health -= mUnit.Damage;
                if (mUnit.mTarget.Health <= 0)
                {
                    mUnit.Game.RemoveObject(mUnit.mTarget.ID);
                    mUnit.mTarget = null;
                    mUnit.IsAttacks = false;
                    return BTreeLeafState.Successed;
                }

                mAttackTimer = TimeSpan.FromSeconds(1 / mUnit.AttackSpeed);
                return BTreeLeafState.Processing;
            }
        }

        private class CheckTargetInRangeLeaf : IBTreeLeaf
        {
            private readonly WarriorUnit mUnit;

            public CheckTargetInRangeLeaf(WarriorUnit unit)
            {
                mUnit = unit;
            }

            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                if (mUnit.mTarget == null)
                    return BTreeLeafState.Failed;

                return mUnit.DistanceTo(mUnit.mTarget) > mUnit.AttackRange ? BTreeLeafState.Failed : BTreeLeafState.Successed;
            }
        }

        private class AttackOrder : Order
        {
            private readonly WarriorUnit mUnit;
            private readonly RtsGameObject mTarget;
            private BTree mOrderLogic;

            public AttackOrder(WarriorUnit unit, RtsGameObject target)
            {
                mUnit = unit;
                mTarget = target;
            }
            public override BTreeLeafState Update(TimeSpan deltaTime)
            {
                if (mOrderLogic == null)
                    return BTreeLeafState.Failed;

                return mOrderLogic.Update(deltaTime);
            }

            public override void Begin()
            {
                mUnit.mTarget = mTarget;
                mOrderLogic = BTree.Build()
                    .Sequence(b => b
                        .Leaf(new GoToTargetLeaf(mUnit))
                        .Leaf(new KillTargetLeaf(mUnit))).Build();
            }

            public override void End()
            {
                mUnit.IsAttacks = false;
            }
        }

        public bool IsAttacks { get; protected set; }
        public float AttackRange { get; protected set; }
        public float AttackSpeed { get; protected set; }
        public int Damage { get; protected set; }
        public Strategy Strategy { get; private set; }

        private RtsGameObject mTarget;

        protected WarriorUnit(Game.Game game, IPathFinder pathFinder, Vector2 position)
            : base(game, pathFinder, position)
        {
            Strategy = Strategy.Aggressive;
        }

        protected override IBTreeBuilder ExtendLogic(IBTreeBuilder baseLogic)
        {
            return BTree.Build()
                .Selector(b =>  
                    baseLogic
                    .Sequence(b1 => b1
                        .Leaf(new CheckStrategyLeaf(this, Strategy.Aggressive))
                        .Leaf(new LockTargetLeaf(this, false))
                        .Leaf(new GoToTargetLeaf(this))
                        .Leaf(new KillTargetLeaf(this)))
                    .Sequence(b2 => b2
                        .Leaf(new CheckStrategyLeaf(this, Strategy.Defencive))
                        .Selector(b21 => b21
                            .Leaf(new CheckTargetInRangeLeaf(this))
                            .Leaf(new LockTargetLeaf(this, true)))
                        .Leaf(new KillTargetLeaf(this))));
        }

        public Task Attack(Guid targetID)
        {
            if (targetID == ID)
                return Task.CompletedTask;

            SetOrder(new AttackOrder(this, Game.GetObject<RtsGameObject>(targetID)));
            return Task.CompletedTask;
        }

        public Task SetStrategy(Strategy strategy)
        {
            Strategy = strategy;
            return Task.CompletedTask;
        }

        private static Vector2 PositionOf(RtsGameObject target)
        {
            if (target is Building)
                return ((Building)target).Size / 2 + target.Position;

            return target.Position;
        }

        private float DistanceTo(RtsGameObject target)
        {
            if (target is Building)
            {
                var p = PositionOf(target);
                var s = ((Building)target).Size;
                var dx = Math.Max(Math.Abs(Position.x - p.x) - s.x / 2, 0);
                var dy = Math.Max(Math.Abs(Position.y - p.y) - s.y / 2, 0);

                return dx * dx + dy * dy;
            }

            return Vector2.Distance(Position, PositionOf(target));
        }
    }
}