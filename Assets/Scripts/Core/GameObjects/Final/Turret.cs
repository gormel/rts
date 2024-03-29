﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Assets.Core.BehaviorTree;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Utils;
using UnityEngine;

namespace Assets.Core.GameObjects.Final
{
    interface ITurretInfo : IBuildingInfo, IAttackerInfo
    {
        Vector2 Direction { get; }
    }

    interface ITurretOrders : IBuildingOrders, IAttackerOrders
    {
        Task Stop();
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
                if (mTargetStorage.Target == null || !mTargetStorage.Target.IsInGame)
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

                if (!target.IsInGame || target.RecivedDamage >= target.MaxHealth)
                    return BTreeLeafState.Successed;

                mTurret.Direction = PositionUtils.PositionOf(target);
                mTurret.IsAttacks = true;
                mAttackSpeedTimer -= deltaTime;
                if (mAttackSpeedTimer > TimeSpan.Zero)
                    return BTreeLeafState.Processing;
                
                mAttackSpeedTimer = TimeSpan.FromSeconds(1 / mTurret.AttackSpeed);
                target.RecivedDamage += Math.Max(mTurret.Damage - target.Armour, 1);

                if (target.RecivedDamage >= target.MaxHealth)
                {
                    mTurret.IsAttacks = false;
                    mTurret.Game.RemoveObject(target.ID);
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
                mTurret.IsAttacks = false;
                return BTreeLeafState.Successed;
            }
        }

        class QueryEnemyLeaf : IBTreeLeaf
        {
            private readonly Game.Game mGame;
            private readonly Turret mTurret;
            private readonly TargetStorage mTargetStorage;

            public QueryEnemyLeaf(Game.Game game, Turret turret, TargetStorage targetStorage)
            {
                mGame = game;
                mTurret = turret;
                mTargetStorage = targetStorage;
            }
            
            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                mTargetStorage.Target = mTurret.Game.QueryObjects(mTurret.Position + mTurret.Size / 2, mTurret.AttackRange)
                    .OrderBy(go => go.MaxHealth)
                    .ThenBy(go => Vector2.Distance(mTurret.Position, PositionUtils.PositionOf(go)))
                    .FirstOrDefault(go => mGame.GetPlayer(go.PlayerID).Team != mTurret.Player.Team);

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

        class WaitConstructionCompleteLeaf : IBTreeLeaf
        {
            private readonly Building mBuilding;

            public WaitConstructionCompleteLeaf(Building building)
            {
                mBuilding = building;
            }

            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                return mBuilding.BuildingProgress == BuildingProgress.Building
                    ? BTreeLeafState.Processing
                    : BTreeLeafState.Successed;
            }
        }
        
        public static Vector2 BuildingSize { get; } = new Vector2(1, 1);
        public const float MaximumHealthConst = 188;
        
        private readonly Vector2 mInitialPosition;
        private BTree mIntelligence;

        public bool IsAttacks { get; private set; }
        public Vector2 Direction { get; private set; }
        public float AttackRange => 4;
        public float AttackSpeed => 2;
        public int Damage => Player.Upgrades.TurretAttackUpgrade.Calculate(BaseDamage);

        private TargetStorage mStorage = new TargetStorage();

        private int BaseDamage { get; } = 6;

        public override float ViewRadius => 5;
        protected override float MaxHealthBase => MaximumHealthConst;
        public override Vector2 Size => BuildingSize;

        public Turret(Game.Game game, Vector2 position, IPlacementService placementService)
            : base(game, Worker.TurretBuildTime, Worker.TurretCost, placementService)
        {
            mInitialPosition = position;
        }

        public override void OnAddedToGame()
        {
            Position = mInitialPosition;

            mIntelligence = BTree.Create("FindAndKill")
                .Success(b1 => b1
                    .Selector(b2 => b2
                        .Sequence(b3 => b3
                            .Leaf(new WaitConstructionCompleteLeaf(this))
                            .Selector(b4 => b4
                                .Leaf(new CheckDistanceLeaf(this, mStorage))
                                .Fail(b5 => b5.Leaf(new ClearTargetLeaf(mStorage)))
                                .Leaf(new QueryEnemyLeaf(Game, this, mStorage)))
                            .Leaf(new KillTargetLeaf(this, mStorage)))
                        .Leaf(new CancelKillLeaf(this)))).Build();
            
            base.OnAddedToGame();
        }

        public override void Update(TimeSpan deltaTime)
        {
            base.Update(deltaTime);
            mIntelligence.Update(deltaTime);
        }

        public async Task Attack(Guid targetId)
        {
            if (targetId == ID)
                return;

            mStorage.Target = Game.GetObject<RtsGameObject>(targetId);
        }

        public Task Stop()
        {
            mStorage.Target = null;
            return Task.CompletedTask;
        }
    }
}