using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Assets.Core.BehaviorTree;
using Assets.Core.Game;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Utils;
using Assets.Core.Map;
using Assets.Networking.Services;
using Core.Projectiles;
using UnityEngine;

namespace Core.GameObjects.Final
{
    interface IArtilleryInfo : IUnitInfo
    {
        float LaunchCooldown { get; }
        float MissileSpeed { get; }
        float MissileRadius { get; }
        float MissileDamage { get; }
        float LaunchRange { get; }
    }

    interface IArtilleryOrders : IUnitOrders
    {
        Task Launch(Vector2 target);
    }
    
    class Artillery : Unit, IArtilleryInfo, IArtilleryOrders
    {
        class CheckCooldownLeaf : IBTreeLeaf
        {
            private readonly Artillery mArtillery;
            private readonly TimeSpan mCooldown;

            public CheckCooldownLeaf(Artillery artillery, TimeSpan cooldown)
            {
                mArtillery = artillery;
                mCooldown = cooldown;
            }

            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                return mArtillery.mLaunchTimer.Elapsed > mCooldown ? BTreeLeafState.Successed : BTreeLeafState.Failed;
            }
        }
        
        class ResetCooldownLeaf : IBTreeLeaf
        {
            private readonly Artillery mArtillery;

            public ResetCooldownLeaf(Artillery artillery)
            {
                mArtillery = artillery;
            }

            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                mArtillery.mLaunchTimer = Stopwatch.StartNew();
                return BTreeLeafState.Successed;
            }
        }

        class CheckDistanceLeaf : IBTreeLeaf
        {
            private readonly Artillery mArtillery;
            private readonly Vector2 mToPoint;
            private readonly float mDist;

            public CheckDistanceLeaf(Artillery artillery, Vector2 toPoint, float dist)
            {
                mArtillery = artillery;
                mToPoint = toPoint;
                mDist = dist;
            }

            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                return Vector2.Distance(mArtillery.Position, mToPoint) < mDist ? BTreeLeafState.Successed : BTreeLeafState.Failed;
            }
        }

        class LaunchMissileLeaf : IBTreeLeaf
        {
            private readonly Artillery mArtillery;
            private readonly Vector2 mToPoint;

            public LaunchMissileLeaf(Artillery artillery, Vector2 toPoint)
            {
                mArtillery = artillery;
                mToPoint = toPoint;
            }

            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                mArtillery.mSpawner.SpawnMissile(
                    mArtillery.Position,
                    mToPoint,
                    mArtillery.MissileSpeed,
                    mArtillery.MissileRadius,
                    mArtillery.MissileDamage
                );
                return BTreeLeafState.Successed;
            }
        }

        class ResolveTaskLeaf<T> : IBTreeLeaf
        {
            private readonly TaskCompletionSource<T> mTcs;
            private readonly T mResult;

            public ResolveTaskLeaf(TaskCompletionSource<T> tcs, T result)
            {
                mTcs = tcs;
                mResult = result;
            }
            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                if (!mTcs.TrySetResult(mResult))
                    return BTreeLeafState.Failed;

                return BTreeLeafState.Successed;
            }
        }

        class CancelTaskLeaf<T> : IBTreeLeaf
        {
            private readonly TaskCompletionSource<T> mTcs;

            public CancelTaskLeaf(TaskCompletionSource<T> tcs)
            {
                mTcs = tcs;
            }
            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                mTcs.TrySetCanceled(); 
                return BTreeLeafState.Successed;
            }
        }

        public const string LaunchIntelligenceTag = "Launch";
        public static TimeSpan LaunchCooldownTime { get; } = TimeSpan.FromSeconds(2);
        
        private readonly IProjectileSpawner mSpawner;
        public override float ViewRadius => 2;
        public override int Armour => Player.Upgrades.UnitArmourUpgrade.Calculate(ArmourBase);
        protected override float MaxHealthBase => 30;
        protected override int ArmourBase => 1;
        public override float Speed => 1.5f;
        public bool LaunchAvaliable => mLaunchTimer.Elapsed.TotalSeconds > 2;
        
        public float LaunchCooldown => (float)Math.Max(0, (LaunchCooldownTime - mLaunchTimer.Elapsed).TotalSeconds);
        public float MissileSpeed => 5;
        public float MissileRadius => 1;
        public float MissileDamage => 60;
        public float LaunchRange => 8;
        
        private Stopwatch mLaunchTimer = Stopwatch.StartNew();
        
        public Artillery(Game game, IPathFinder pathFinder, Vector2 position, IProjectileSpawner spawner) 
            : base(game, pathFinder, position)
        {
            mSpawner = spawner;
        }

        public async Task Launch(Vector2 target)
        {
            await ApplyIntelligence(
                b => b
                    .Selector(b0 => b0
                        .Invert(b1 => b1
                            .Leaf(new CheckCooldownLeaf(this, LaunchCooldownTime))
                        )
                        .Sequence(b1 => b1
                            .Selector(b2 => b2
                                .Leaf(new CheckDistanceLeaf(this, target, LaunchRange))
                                .Leaf(new GoToTargetLeaf(PathFinder, target, Game.Map.Data))
                            )
                            .Leaf(new CancelGotoLeaf(PathFinder))
                            .Leaf(new ResetCooldownLeaf(this))
                            .Leaf(new RotateToLeaf(PathFinder, target, Game.Map.Data))
                            .Leaf(new LaunchMissileLeaf(this, target))
                        )
                    ),
                b => b
                    .Leaf(new CancelGotoLeaf(PathFinder)),
                LaunchIntelligenceTag
            );
        }
    }
}