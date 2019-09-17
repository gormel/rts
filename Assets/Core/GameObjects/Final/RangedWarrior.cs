using System;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Utils;
using Assets.Core.Map;
using UnityEngine;

namespace Assets.Core.GameObjects.Final {

    interface IRangedWarriorInfo : IUnitInfo
    {
    }

    interface IRangedWarriorOrders : IUnitOrders
    {
        Task Attack(Guid targetID);
    }

    internal class RangedWarrior : Unit, IRangedWarriorInfo, IRangedWarriorOrders
    {
        class AttackOrder : UnitOrder
        {
            private readonly RangedWarrior mWarrior;
            private readonly RtsGameObject mTarget;
            private double mAttackCooldown;

            public AttackOrder(RangedWarrior warrior, RtsGameObject target)
            {
                mWarrior = warrior;
                mTarget = target;
            }
            protected override Task OnBegin()
            {
                return Task.CompletedTask;
            }

            protected override void OnUpdate(TimeSpan deltaTime)
            {
                mWarrior.Position = mWarrior.PathFinder.CurrentPosition;
                mWarrior.Direction = mWarrior.PathFinder.CurrentDirection;
                mWarrior.Destignation = mTarget.Position;

                var d = Vector2.Distance(mWarrior.Position, mTarget.Position);
                if (d > mWarrior.AttackRange)
                {
                    mWarrior.IsAttacks = false;
                    mAttackCooldown = 0;
                    mWarrior.PathFinder.SetTarget(mTarget.Position, mWarrior.Game.Map.Data);
                }
                else
                {
                    mWarrior.PathFinder.Stop();
                    if (mAttackCooldown > 1 / mWarrior.AttackSpeed)
                    {
                        mTarget.Health -= mWarrior.Damage;
                        if (mTarget.Health <= 0)
                            mWarrior.Game.RemoveObject(mTarget.ID);
                        mAttackCooldown = 0;
                    }

                    mAttackCooldown += deltaTime.TotalSeconds;
                    mWarrior.IsAttacks = true;
                }
            }

            protected override void OnCancel()
            {
                mWarrior.IsAttacks = false;
                mWarrior.PathFinder.Stop();
            }
        }

        public bool IsAttacks { get; protected set; }
        public float AttackRange { get; protected set; }
        public float AttackSpeed { get; protected set; }
        public int Damage { get; protected set; }

        public RangedWarrior(Game.Game game, IPathFinder pathFinder, Vector2 position)
            : base(game, pathFinder, position)
        {
            Speed = 4;
            MaxHealth = Health = 50;
            AttackRange = 5;
            AttackSpeed = 2;
            Damage = 5;
        }

        public Task Attack(Guid targetID)
        {
            SetOrder(new AttackOrder(this, Game.GetObject<RtsGameObject>(targetID)));
            return Task.CompletedTask;
        }
    }
}