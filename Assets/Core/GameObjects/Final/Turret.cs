using System;
using System.Linq;
using Assets.Core.BehaviorTree;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Utils;
using UnityEngine;

namespace Assets.Core.GameObjects.Final
{
    interface ITurretInfo : IBuildingInfo
    {
        bool IsShooting { get; }
        Vector2 Direction { get; }
        float AttackRange { get; }
        float AttackSpeed { get; }
        int Damage { get; }
    }

    interface ITurretOrders : IBuildingOrders
    {
    }
    
    class Turret : Building, ITurretInfo, ITurretOrders
    {
        class TargetStorage
        {
            public RtsGameObject Target { get; set; }
        }
            
        class CheckDistanceLeaf : IBTreeLeaf
        {
            private readonly Turret mTurret;
            private readonly TargetStorage mTargetStorage;

            public CheckDistanceLeaf(Turret turret, TargetStorage targetStorage)
            {
                mTurret = turret;
                mTargetStorage = targetStorage;
            }
            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                if (mTargetStorage.Target == null)
                    return BTreeLeafState.Failed;
                    
                return mTurret.DistanceTo(mTargetStorage.Target) <= mTurret.AttackRange ? BTreeLeafState.Successed : BTreeLeafState.Failed;
            }
        }

        class KillTargetLeaf : IBTreeLeaf
        {
            private readonly Turret mTurret;
            private readonly TargetStorage mTargetStorage;

            private TimeSpan mAttackSpeedTimer;
            private readonly TimeSpan mAttackDuration;

            public KillTargetLeaf(Turret turret, TargetStorage targetStorage)
            {
                mTurret = turret;
                mTargetStorage = targetStorage;
                mAttackSpeedTimer = TimeSpan.FromSeconds(1 / mTurret.AttackSpeed);
            }

            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                var target = mTargetStorage.Target;
                if (mTurret.DistanceTo(target) > mTurret.AttackRange)
                {
                    mAttackSpeedTimer = TimeSpan.FromSeconds(1 / mTurret.AttackSpeed);
                    return BTreeLeafState.Failed;
                }

                if (!target.IsInGame || target.Health <= 0)
                    return BTreeLeafState.Successed;

                mTurret.Direction = PositionUtils.PositionOf(target);
                mTurret.IsShooting = true;
                mAttackSpeedTimer -= deltaTime;
                if (mAttackSpeedTimer > TimeSpan.Zero)
                    return BTreeLeafState.Processing;
                
                mAttackSpeedTimer = TimeSpan.FromSeconds(1 / mTurret.AttackSpeed);
                target.Health -= mTurret.Damage;

                if (target.Health <= 0)
                {
                    mTurret.IsShooting = false;
                    mTurret.mGame.RemoveObject(target.ID);
                    return BTreeLeafState.Successed;
                }

                return BTreeLeafState.Processing;
            }
        }

        class CancelKillLeaf : IBTreeLeaf
        {
            private readonly Turret mTurret;
            public CancelKillLeaf(Turret turret)
            {
                mTurret = turret;
            }
            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                mTurret.IsShooting = false;
                return BTreeLeafState.Successed;
            }
        }

        class QueryEnemyLeaf : IBTreeLeaf
        {
            private readonly Turret mTurret;
            private readonly TargetStorage mTargetStorage;

            public QueryEnemyLeaf(Turret turret, TargetStorage targetStorage)
            {
                mTurret = turret;
                mTargetStorage = targetStorage;
            }
            
            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                mTargetStorage.Target = mTurret.mGame.QueryObjects(mTurret.Position, mTurret.AttackRange)
                    .OrderBy(go => go.MaxHealth)
                    .ThenBy(go => Vector2.Distance(mTurret.Position, PositionUtils.PositionOf(go)))
                    .FirstOrDefault(go => /*go.ID != mUnit.ID ||*/ go.PlayerID != mTurret.PlayerID);

                return mTargetStorage.Target == null ? BTreeLeafState.Failed : BTreeLeafState.Successed;
            }
        }
        
        class ClearTargetLeaf : IBTreeLeaf
        {
            private readonly TargetStorage mStorage;

            public ClearTargetLeaf(TargetStorage storage)
            {
                mStorage = storage;
            }
            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                mStorage.Target = null;
                return BTreeLeafState.Successed;
            }
        }
        public static Vector2 BuildingSize { get; } = new Vector2(1, 1);
        public const float MaximumHealthConst = 100;
        
        private readonly Game.Game mGame;
        private BTree mIntelligence;

        public bool IsShooting { get; private set; }
        public Vector2 Direction { get; private set; }
        public float AttackRange { get; private set; }
        public float AttackSpeed { get; private set; }
        public int Damage { get; private set; }

        public Turret(Game.Game game, Vector2 position)
        {
            mGame = game;
            Position = position;
            Size = BuildingSize;
            Health = MaxHealth = MaximumHealthConst;
            ViewRadius = 2;
            
            AttackRange = 4;
            AttackSpeed = 2;
            Damage = 3;

            var storage = new TargetStorage();
            mIntelligence = BTree.Create()
                .Success(b1 => b1
                    .Selector(b2 => b2
                        .Sequence(b3 => b3
                            .Selector(b4 => b4
                                .Leaf(new CheckDistanceLeaf(this, storage))
                                .Fail(b5 => b5.Leaf(new ClearTargetLeaf(storage)))
                                .Leaf(new QueryEnemyLeaf(this, storage)))
                            .Leaf(new KillTargetLeaf(this, storage)))
                        .Leaf(new CancelKillLeaf(this)))).Build();
        }
        public override void Update(TimeSpan deltaTime)
        {
            mIntelligence.Update(deltaTime);
        }
    }
}