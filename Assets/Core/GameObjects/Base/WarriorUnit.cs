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

    interface IWarriorInfo : IUnitInfo, IAttackerInfo
    {
        Strategy Strategy { get; }
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
                target.RecivedDamage += mUnit.Damage;

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
            private readonly WarriorUnit mUnit;
            private readonly TargetStorage mTargetStorage;

            public QueryEnemyLeaf(WarriorUnit unit, TargetStorage targetStorage)
            {
                mUnit = unit;
                mTargetStorage = targetStorage;
            }
            
            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                mTargetStorage.Target = mUnit.Game.QueryObjects(mUnit.Position, mUnit.AttackRange)
                    .OrderBy(go => go.MaxHealth)
                    .ThenBy(go => Vector2.Distance(mUnit.Position, PositionUtils.PositionOf(go)))
                    .FirstOrDefault(go => /*go.ID != mUnit.ID ||*/ go.PlayerID != mUnit.PlayerID);

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
        public float AttackRange { get; protected set; }
        public float AttackSpeed { get; protected set; }
        public int Damage { get; protected set; }
        public Strategy Strategy { get; private set; }

        private IBTreeBuilder mAgressiveIntelligence;
        private IBTreeBuilder mDefenciveIntelligence;
        
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
                                .Leaf(new QueryEnemyLeaf(this, storage))), storage)),
                b => b
                    .Leaf(new ClearTargetLeaf(storage))
                    .Leaf(new CancelGotoLeaf(PathFinder))
                    .Leaf(new CancelKillLeaf(this)));
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
                                    .Leaf(new QueryEnemyLeaf(this, storage)))
                                .Leaf(new KillTargetLeaf(this, storage)))
                            .Leaf(new CancelKillLeaf(this)))),
                b => b
                    .Leaf(new ClearTargetLeaf(storage))
                    .Leaf(new CancelKillLeaf(this)));
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
                    .Leaf(new CancelKillLeaf(this))
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
                                    .Leaf(new QueryEnemyLeaf(this, storage))), storage)))
                        .Sequence(b5 => b5
                            .Leaf(new CancelKillLeaf(this))
                            .Leaf(new GoToTargetLeaf(PathFinder, position, Game.Map.Data)))),
                b => b
                    .Leaf(new CancelKillLeaf(this))
                    .Leaf(new CancelGotoLeaf(PathFinder))
                    .Leaf(new ClearTargetLeaf(storage))
                );
        }

        public Task SetStrategy(Strategy strategy)
        {
            Strategy = strategy;
            return Task.CompletedTask;
        }
    }
}