using System;
using System.Collections.Generic;
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

    interface IWarriorInfo : IUnitInfo, IAttackerInfo
    {
        Strategy Strategy { get; }
        
        WarriorMovementState MovementState { get; }
    }

    interface IWarriorOrders : IUnitOrders, IAttackerOrders
    {
        Task GoToAndAttack(Vector2 position);
        Task SetStrategy(Strategy strategy);
    }

    abstract class WarriorUnit : Unit, IWarriorInfo, IWarriorOrders
    {
        class TargetStorage
        {
            public RtsGameObject Target { get; set; }
        }
        
        class CheckDistanceLeaf : IBTreeLeaf
        {
            private readonly WarriorUnit mUnit;
            private readonly TargetStorage mTargetStorage;

            public CheckDistanceLeaf(WarriorUnit unit, TargetStorage targetStorage)
            {
                mUnit = unit;
                mTargetStorage = targetStorage;
            }
            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                if (mTargetStorage.Target == null)
                    return BTreeLeafState.Failed;
                
                return mUnit.DistanceTo(mTargetStorage.Target) <= mUnit.AttackRange ? BTreeLeafState.Successed : BTreeLeafState.Failed;
            }
        }

        class FollowTargetLeaf : IBTreeLeaf
        {
            private readonly WarriorUnit mUnit;
            private readonly TargetStorage mTargetStorage;

            public FollowTargetLeaf(WarriorUnit unit, TargetStorage targetStorage)
            {
                mUnit = unit;
                mTargetStorage = targetStorage;
            }

            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                if (mTargetStorage.Target == null)
                    return BTreeLeafState.Failed;
                
                mUnit.PathFinder.SetTarget(PositionUtils.PositionOf(mTargetStorage.Target), mUnit.Game.Map.Data);
                return BTreeLeafState.Processing;
            }
        }

        class KillTargetLeaf : IBTreeLeaf
        {
            private readonly WarriorUnit mUnit;
            private readonly TargetStorage mTargetStorage;

            private TimeSpan mAttackSpeedTimer;
            private readonly TimeSpan mAttackDuration;

            public KillTargetLeaf(WarriorUnit unit, TargetStorage targetStorage)
            {
                mUnit = unit;
                mTargetStorage = targetStorage;
                mAttackSpeedTimer = TimeSpan.FromSeconds(1 / mUnit.AttackSpeed);
            }

            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                var target = mTargetStorage.Target;
                if (mUnit.DistanceTo(target) > mUnit.AttackRange)
                {
                    mAttackSpeedTimer = TimeSpan.FromSeconds(1 / mUnit.AttackSpeed);
                    return BTreeLeafState.Failed;
                }

                if (!target.IsInGame || target.RecivedDamage >= target.MaxHealth)
                    return BTreeLeafState.Successed;

                mUnit.PathFinder.SetLookAt(PositionUtils.PositionOf(target), mUnit.Game.Map.Data);
                mUnit.IsAttacks = true;
                mAttackSpeedTimer -= deltaTime;
                if (mAttackSpeedTimer > TimeSpan.Zero)
                    return BTreeLeafState.Processing;
                
                mAttackSpeedTimer = TimeSpan.FromSeconds(1 / mUnit.AttackSpeed);
                target.RecivedDamage += Math.Max(mUnit.Damage - target.Armour, 1);

                if (target.RecivedDamage >= target.MaxHealth)
                {
                    mUnit.IsAttacks = false;
                    mUnit.Game.RemoveObject(target.ID);
                    return BTreeLeafState.Successed;
                }

                return BTreeLeafState.Processing;
            }
        }

        class CancelKillLeaf : IBTreeLeaf
        {
            private readonly WarriorUnit mUnit;

            public CancelKillLeaf(WarriorUnit unit)
            {
                mUnit = unit;
            }
            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                mUnit.IsAttacks = false;
                return BTreeLeafState.Successed;
            }
        }

        class QueryEnemyLeaf : IBTreeLeaf
        {
            private readonly Game.Game mGame;
            private readonly WarriorUnit mUnit;
            private readonly TargetStorage mTargetStorage;

            private List<RtsGameObject> mQueried = new();

            public QueryEnemyLeaf(Game.Game game, WarriorUnit unit, TargetStorage targetStorage)
            {
                mGame = game;
                mUnit = unit;
                mTargetStorage = targetStorage;
            }
            
            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                mQueried.Clear();
                mUnit.Game.QueryObjectsNoAlloc(mUnit.Position, mUnit.AttackRange, mQueried);
                mTargetStorage.Target = mQueried.OrderBy(go => go.MaxHealth)
                    .ThenBy(go => Vector2.Distance(mUnit.Position, PositionUtils.PositionOf(go)))
                    .FirstOrDefault(go => mGame.GetPlayer(go.PlayerID).Team != mUnit.Player.Team);

                return mTargetStorage.Target == null ? BTreeLeafState.Failed : BTreeLeafState.Successed;
            }
        }

        class CheckTargetLeaf : IBTreeLeaf
        {
            private readonly TargetStorage mStorage;

            public CheckTargetLeaf(TargetStorage storage)
            {
                mStorage = storage;
            }
            
            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                if (mStorage.Target == null)
                    return BTreeLeafState.Failed;

                if (!mStorage.Target.IsInGame)
                {
                    mStorage.Target = null;
                    return BTreeLeafState.Failed;
                }
                
                return BTreeLeafState.Successed;
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

        public bool IsAttacks { get; protected set; }
        public Strategy Strategy { get; private set; }

        public WarriorMovementState MovementState
        {
            get
            {
                switch (IntelligenceTag)
                {
                    case AggressiveIdleIntelligenceTag:
                        return WarriorMovementState.Agressive;
                    case AggressiveWalkingIntelligenceTag:
                        return WarriorMovementState.Agressive;
                    case KillTargetIntelligenceTag:
                        return WarriorMovementState.Agressive;
                }

                return WarriorMovementState.Common;
            }
        }

        private IBTreeBuilder mAgressiveIntelligence;
        private IBTreeBuilder mDefenciveIntelligence;

        public const string AggressiveIdleIntelligenceTag = "AggressiveIdle";
        public const string DefenciveIdleIntelligenceTag = "DefenciveIdel";
        public const string KillTargetIntelligenceTag = "KillTarget";
        public const string AggressiveWalkingIntelligenceTag = "AggressiveWalking";

        public override int Armour => Player.Upgrades.UnitArmourUpgrade.Calculate(ArmourBase);
        public int Damage => Player.Upgrades.UnitDamageUpgrade.Calculate(DamageBase);
        
        public abstract float AttackRange { get; }
        public abstract float AttackSpeed { get; }
        
        protected abstract int DamageBase { get; }

        protected WarriorUnit(Game.Game game, IPathFinder pathFinder, Vector2 position)
            : base(game, pathFinder, position)
        {
        }

        public override void OnAddedToGame()
        {
            Strategy = Strategy.Aggressive;
            
            mAgressiveIntelligence = CreateAggressiveIntelligence();
            mDefenciveIntelligence = CreateDefenciveIntelligence();
            
            base.OnAddedToGame();
        }

        protected override IBTreeBuilder GetDefaultIntelligence()
        {
            if (Strategy == Strategy.Aggressive)
                return mAgressiveIntelligence;

            if (Strategy == Strategy.Defencive)
                return mDefenciveIntelligence;
            
            return base.GetDefaultIntelligence();
        }
        private IBTreeBuilder CreateAggressiveIntelligence()
        {
            var storage = new TargetStorage();
            return WrapCancellation(
                b => b.Success(b1 => 
                        CreateFollowAndKillIntelligence(b1
                            .Selector(b2 => b2
                                .Leaf(new CheckTargetLeaf(storage))
                                .Leaf(new QueryEnemyLeaf(Game, this, storage))), storage)),
                b => b
                    .Leaf(new ClearTargetLeaf(storage))
                    .Leaf(new CancelGotoLeaf(PathFinder))
                    .Leaf(new CancelKillLeaf(this)),
                AggressiveIdleIntelligenceTag
                );
        }

        private IBTreeBuilder CreateDefenciveIntelligence()
        {
            var storage = new TargetStorage();
            return WrapCancellation(
                b => b.Success(b1 => b1
                        .Selector(b2 => b2
                            .Sequence(b3 => b3
                                .Selector(b4 => b4
                                    .Leaf(new CheckDistanceLeaf(this, storage))
                                    .Leaf(new QueryEnemyLeaf(Game, this, storage)))
                                .Leaf(new KillTargetLeaf(this, storage)))
                            .Leaf(new CancelKillLeaf(this)))),
                b => b
                    .Leaf(new ClearTargetLeaf(storage))
                    .Leaf(new CancelKillLeaf(this)),
                DefenciveIdleIntelligenceTag
                );
        }

        private IBTreeBuilder CreateFollowAndKillIntelligence(IBTreeBuilder parent, TargetStorage targetStorage)
        {
            return parent
                .Sequence(b1 => b1
                    .Selector(b2 => b2
                        .Leaf(new CheckDistanceLeaf(this, targetStorage))
                        .Fail(b3 => b3.Leaf(new CancelKillLeaf(this)))
                        .Leaf(new FollowTargetLeaf(this, targetStorage)))
                    .Leaf(new CancelGotoLeaf(PathFinder))
                    .Leaf(new KillTargetLeaf(this, targetStorage)));
        }

        private Task ApplyAttackTargetIntelligence(RtsGameObject target)
        {
            return ApplyIntelligence(
                b => CreateFollowAndKillIntelligence(b, new TargetStorage { Target = target }),
                b => b
                    .Leaf(new CancelGotoLeaf(PathFinder))
                    .Leaf(new CancelKillLeaf(this)),
                KillTargetIntelligenceTag
                );
        }

        public async Task Attack(Guid targetID)
        {
            if (targetID == ID)
                return;

            var target = Game.GetObject<RtsGameObject>(targetID);
            await ApplyAttackTargetIntelligence(target);
        }

        public Task GoToAndAttack(Vector2 position)
        {
            var storage = new TargetStorage();
            return ApplyIntelligence(
                b => b
                    .Selector(b1 => b1
                        .Fail(b2 => b2
                            .Sequence(b3 => CreateFollowAndKillIntelligence(b3
                                .Selector(b4 => b4
                                    .Leaf(new CheckTargetLeaf(storage))
                                    .Leaf(new QueryEnemyLeaf(Game, this, storage))), storage)))
                        .Sequence(b5 => b5
                            .Leaf(new CancelKillLeaf(this))
                            .Leaf(new GoToTargetLeaf(PathFinder, position, Game.Map.Data)))),
                b => b
                    .Leaf(new CancelKillLeaf(this))
                    .Leaf(new CancelGotoLeaf(PathFinder))
                    .Leaf(new ClearTargetLeaf(storage)),
                AggressiveWalkingIntelligenceTag
                );
        }

        public Task SetStrategy(Strategy strategy)
        {
            Strategy = strategy;
            return Task.CompletedTask;
        }
    }
}