using System;
using System.Linq;
using System.Threading.Tasks;
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
        class AttackOrder : UnitOrder
        {
            private readonly WarriorUnit mWarrior;
            private readonly RtsGameObject mTarget;
            private double mAttackCooldown;
            private float mTimeToAttack;

            public AttackOrder(WarriorUnit warrior, RtsGameObject target)
            {
                mWarrior = warrior;
                mTarget = target;
                mTarget.RemovedFromGame += TargetOnRemovedFromGame;
                mWarrior.PathFinder.Arrived += WarriorOnArrived;
                mTimeToAttack = 1 / mWarrior.AttackSpeed;
                mAttackCooldown = 0;
            }

            private void WarriorOnArrived()
            {
                mWarrior.PathFinder.SetLookAt(mTarget.Position, mWarrior.Game.Map.Data);
                mWarrior.PathFinder.Arrived -= WarriorOnArrived;
            }

            private void TargetOnRemovedFromGame(RtsGameObject obj)
            {
                mTarget.RemovedFromGame -= TargetOnRemovedFromGame;
                End();
                OnCancel();
            }

            protected override Task OnBegin()
            {
                return Task.CompletedTask;
            }

            protected override void OnUpdate(TimeSpan deltaTime)
            {
                if (mWarrior.DistanceTo(mTarget) > mWarrior.AttackRange)
                {
                    mWarrior.IsAttacks = false;
                    if (mAttackCooldown > mTimeToAttack)
                        mWarrior.PathFinder.SetTarget(PositionOf(mTarget), mWarrior.Game.Map.Data);
                }
                else
                {
                    if (!mWarrior.IsAttacks)
                    {
                        mWarrior.PathFinder.Stop();
                        mWarrior.PathFinder.SetLookAt(PositionOf(mTarget), mWarrior.Game.Map.Data);
                    }

                    if (mAttackCooldown > mTimeToAttack)
                    {
                        mTarget.Health -= mWarrior.Damage;
                        if (mTarget.Health <= 0)
                            mWarrior.Game.RemoveObject(mTarget.ID);
                        mAttackCooldown = 0;
                    }

                    mWarrior.IsAttacks = true;
                }
                mAttackCooldown += deltaTime.TotalSeconds;
            }

            protected override void OnCancel()
            {
                mWarrior.PathFinder.Arrived -= WarriorOnArrived;
                mWarrior.IsAttacks = false;
                mWarrior.PathFinder.Stop();
            }
        }

        class StandAttackOrder : UnitOrder
        {
            private readonly WarriorUnit mWarrior;
            private RtsGameObject mTarget;
            private float mWarriorSpeed;
            private double mAttackCooldown;
            private float mTimeToAttack;

            public StandAttackOrder(WarriorUnit warrior)
            {
                mWarrior = warrior;
                mTimeToAttack = 1 / mWarrior.AttackSpeed;
                mAttackCooldown = 0;
            }

            protected override Task OnBegin()
            {
                mWarriorSpeed = mWarrior.Speed;
                mWarrior.Speed = 0;
                return Task.CompletedTask;
            }

            protected override void OnUpdate(TimeSpan deltaTime)
            {
                if (mTarget == null || mWarrior.DistanceTo(mTarget) > mWarrior.AttackRange)
                {
                    if (mTarget != null)
                        mTarget.RemovedFromGame -= TargetOnRemovedFromGame;

                    mTarget = mWarrior.FindEnemy(mWarrior.AttackRange);
                    if (mTarget != null)
                        mTarget.RemovedFromGame += TargetOnRemovedFromGame;

                    mWarrior.IsAttacks = mTarget != null;
                    return;
                }

                mWarrior.PathFinder.SetLookAt(PositionOf(mTarget), mWarrior.Game.Map.Data);
                if (mAttackCooldown > mTimeToAttack)
                {
                    mTarget.Health -= mWarrior.Damage;
                    if (mTarget.Health <= 0)
                        mWarrior.Game.RemoveObject(mTarget.ID);
                    mAttackCooldown = 0;
                }
                mAttackCooldown += deltaTime.TotalSeconds;
            }

            private void TargetOnRemovedFromGame(RtsGameObject obj)
            {
                mTarget.RemovedFromGame -= TargetOnRemovedFromGame;
                mTarget = null;
            }

            protected override void OnCancel()
            {
                mWarrior.IsAttacks = false;
                mWarrior.Speed = mWarriorSpeed;
                if (mTarget != null)
                {
                    mTarget.RemovedFromGame -= TargetOnRemovedFromGame;
                    mTarget = null;
                }
            }
        }

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

            SetOrder(new AttackOrder(this, Game.GetObject<RtsGameObject>(targetID)));
            return Task.CompletedTask;
        }

        public Task SetStrategy(Strategy strategy)
        {
            Strategy = strategy;
            return Task.CompletedTask;
        }

        private RtsGameObject FindEnemy(float radius)
        {
            return Game.QueryObjects(Position, radius).OrderBy(go => Vector2.Distance(Position, PositionOf(go))).FirstOrDefault(go => go.PlayerID != PlayerID);
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

        public override void Update(TimeSpan deltaTime)
        {
            base.Update(deltaTime);

            if (HasNoOrder)
            {
                if (Strategy == Strategy.Aggressive)
                {
                    var queried = Game.QueryObjects(Position, ViewRadius).FirstOrDefault(go => go.PlayerID != PlayerID);
                    if (queried != null)
                        SetOrder(new AttackOrder(this, queried));
                }

                if (Strategy == Strategy.Defencive)
                {
                    SetOrder(new StandAttackOrder(this));
                }
            }
        }
    }
}
