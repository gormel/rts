using System;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Utils;
using Assets.Core.Map;
using UnityEngine;

namespace Assets.Core.GameObjects.Base
{
    interface IWarriorInfo : IUnitInfo
    {
        bool IsAttacks { get; }
        float AttackRange { get; }
        float AttackSpeed { get; }
        int Damage { get; }
    }

    interface IWarriorOrders : IUnitOrders
    {
        Task Attack(Guid targetID);
    }

    abstract class WarriorUnit : Unit, IWarriorInfo, IWarriorOrders
    {
        class AttackOrder : UnitOrder
        {
            private readonly WarriorUnit mWarrior;
            private readonly RtsGameObject mTarget;
            private double mAttackCooldown;
            private float mTimeToAttack;

            private Vector2 TargetPosition
            {
                get
                {
                    if (mTarget is Building)
                        return ((Building)mTarget).Size / 2 + mTarget.Position;

                    return mTarget.Position;
                }
            }

            private float TargetDistance
            {
                get
                {
                    if (mTarget is Building)
                    {
                        var p = TargetPosition;
                        var s = ((Building)mTarget).Size;
                        var dx = Math.Max(Math.Abs(mWarrior.Position.x - p.x) - s.x / 2, 0);
                        var dy = Math.Max(Math.Abs(mWarrior.Position.y - p.y) - s.y / 2, 0);

                        return dx * dx + dy * dy;
                    }

                    return Vector2.Distance(mWarrior.Position, TargetPosition);
                }
            }

            public AttackOrder(WarriorUnit warrior, RtsGameObject target)
            {
                mWarrior = warrior;
                mTarget = target;
                mTarget.RemovedFromGame += TargetOnRemovedFromGame;
                mWarrior.PathFinder.Arrived += WarriorOnArrived;
                mTimeToAttack = 1 / mWarrior.AttackSpeed;
                mAttackCooldown = mTimeToAttack;
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
                if (TargetDistance > mWarrior.AttackRange)
                {
                    mWarrior.IsAttacks = false;
                    if (mAttackCooldown > mTimeToAttack)
                        mWarrior.PathFinder.SetTarget(TargetPosition, mWarrior.Game.Map.Data);
                }
                else
                {
                    if (!mWarrior.IsAttacks)
                    {
                        mWarrior.PathFinder.Stop();
                        mWarrior.PathFinder.SetLookAt(TargetPosition, mWarrior.Game.Map.Data);
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

        public bool IsAttacks { get; protected set; }
        public float AttackRange { get; protected set; }
        public float AttackSpeed { get; protected set; }
        public int Damage { get; protected set; }

        protected WarriorUnit(Game.Game game, IPathFinder pathFinder, Vector2 position)
            : base(game, pathFinder, position)
        {
        }

        public Task Attack(Guid targetID)
        {
            if (targetID == ID)
                return Task.CompletedTask;

            SetOrder(new AttackOrder(this, Game.GetObject<RtsGameObject>(targetID)));
            return Task.CompletedTask;
        }
    }
}
