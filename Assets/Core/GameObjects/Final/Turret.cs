using System;
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
                target.RecivedDamage += mTurret.Damage;

                if (target.RecivedDamage >= target.MaxHealth)
                {
                    mTurret.IsAttacks = false;
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
                mTurret.IsAttacks = false;
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
                mTargetStorage.Target = mTurret.mGame.QueryObjects(mTurret.Position + mTurret.Size / 2, mTurret.AttackRange)
                    .OrderBy(go => go.MaxHealth)
                    .ThenBy(go => Vector2.Distance(mTurret.Position, PositionUtils.PositionOf(go)))
                    .FirstOrDefault(go => /*go.ID != mTurret.ID ||*/ go.PlayerID != mTurret.PlayerID);

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
        public const float MaximumHealthConst = 188;
        
        private readonly Game.Game mGame;
        private readonly Vector2 mInitialPosition;
        private BTree mIntelligence;

        public bool IsAttacks { get; private set; }
        public Vector2 Direction { get; private set; }
        public float AttackRange { get; private set; }
        public float AttackSpeed { get; private set; }
        public int Damage => Player.Upgrades.TurretAttackUpgrade.Calculate(BaseDamage);

        private TargetStorage mStorage = new TargetStorage();

        private int BaseDamage { get; } = 6;

        protected override float MaxHealthBase => MaximumHealthConst;

        public Turret(Game.Game game, Vector2 position)
        {
            mGame = game;
            mInitialPosition = position;
        }

        public override void OnAddedToGame()
        {
            Position = mInitialPosition;
            Size = BuildingSize;
            ViewRadius = 5;
            
            AttackRange = 4;
            AttackSpeed = 2;

            mIntelligence = BTree.Create("FindAndKill")
                .Success(b1 => b1
                    .Selector(b2 => b2
                        .Sequence(b3 => b3
                            .Selector(b4 => b4
                                .Leaf(new CheckDistanceLeaf(this, mStorage))
                                .Fail(b5 => b5.Leaf(new ClearTargetLeaf(mStorage)))
                                .Leaf(new QueryEnemyLeaf(this, mStorage)))
                            .Leaf(new KillTargetLeaf(this, mStorage)))
                        .Leaf(new CancelKillLeaf(this)))).Build();
            
            base.OnAddedToGame();
        }

        public override void Update(TimeSpan deltaTime)
        {
            mIntelligence.Update(deltaTime);
        }

        public async Task Attack(Guid targetId)
        {
            if (targetId == ID)
                return;

            mStorage.Target = mGame.GetObject<RtsGameObject>(targetId);
        }

        public Task Stop()
        {
            mStorage.Target = null;
            return Task.CompletedTask;
        }
    }
}