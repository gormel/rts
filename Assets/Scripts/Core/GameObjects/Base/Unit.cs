using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Assets.Core.BehaviorTree;
using Assets.Core.GameObjects.Utils;
using Assets.Core.Map;
using UnityEngine;

namespace Assets.Core.GameObjects.Base
{
    interface IUnitOrders : IGameObjectOrders
    {
        Task GoTo(Vector2 position);
        Task Stop();
    }

    interface IUnitInfo : IGameObjectInfo
    {
        float Speed { get; }
        Vector2 Direction { get; }
        Vector2 Destignation { get; }
    }

    abstract class Unit : RtsGameObject, IUnitInfo, IUnitOrders
    {
        private readonly Vector2 mInitialPosition;

        class CommandCancellation
        {
            private TaskCompletionSource<bool> mTaskSource = null;

            public bool IsCancellationRequested => mTaskSource != null; 
            
            public Task Cancel()
            {
                lock(this)
                {
                    if (mTaskSource == null)
                        mTaskSource = new TaskCompletionSource<bool>();
                }
                
                return mTaskSource.Task;
            }

            public void ConfirmCancel()
            {
                if (mTaskSource == null)
                    return;
                
                mTaskSource.SetResult(true);
                mTaskSource = null;
            }
        }

        class CheckCancellationLeaf : IBTreeLeaf
        {
            private readonly CommandCancellation mCancellation;

            public CheckCancellationLeaf(CommandCancellation cancellation)
            {
                mCancellation = cancellation;
            }
            
            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                return mCancellation.IsCancellationRequested ? BTreeLeafState.Successed : BTreeLeafState.Failed;
            }
        }

        class ConfirmCancellationLeaf : IBTreeLeaf
        {
            private readonly CommandCancellation mCancellation;

            public ConfirmCancellationLeaf(CommandCancellation cancellation)
            {
                mCancellation = cancellation;
            }
            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                mCancellation.ConfirmCancel();
                return BTreeLeafState.Successed;
            }
        }

        protected class RotateToLeaf : IBTreeLeaf
        {
            private readonly IPathFinder mPathFinder;
            private readonly Vector2 mTarget;
            private readonly IMapData mMapData;

            public RotateToLeaf(IPathFinder pathFinder, Vector2 target, IMapData mapData)
            {
                mPathFinder = pathFinder;
                mTarget = target;
                mMapData = mapData;
            }
            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                mPathFinder.SetLookAt(mTarget, mMapData);
                return BTreeLeafState.Successed;
            }
        }

        protected class GoToTargetLeaf : IBTreeLeaf
        {
            private readonly IPathFinder mPathFinder;
            private readonly Vector2 mTarget;
            private readonly IMapData mMapData;

            public GoToTargetLeaf(IPathFinder pathFinder, Vector2 target, IMapData mapData)
            {
                mPathFinder = pathFinder;
                mTarget = target;
                mMapData = mapData;
            }
            
            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                if (mPathFinder.InProgress)
                    return BTreeLeafState.Processing;

                if (mPathFinder.IsArrived && Vector2.Distance(mTarget, mPathFinder.Target) < 0.01f)
                    return BTreeLeafState.Successed;
                
                mPathFinder.SetTarget(mTarget, mMapData);
                return BTreeLeafState.Processing;
            }
        }

        protected class CancelGotoLeaf : IBTreeLeaf
        {
            private readonly IPathFinder mPathFinder;

            public CancelGotoLeaf(IPathFinder pathFinder)
            {
                mPathFinder = pathFinder;
            }
            
            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                if (mPathFinder.IsArrived)
                    return BTreeLeafState.Successed;

                mPathFinder.Stop();
                return BTreeLeafState.Successed;
            }
        }

        private class FreeIntelligenceLeaf : IBTreeLeaf
        {
            private readonly Unit mUnit;

            public FreeIntelligenceLeaf(Unit unit)
            {
                mUnit = unit;
            }
            
            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                mUnit.ApplyDefaultIntelligence();
                return BTreeLeafState.Successed;
            }
        }

        public const string IdleIntelligenceTag = "Idle";
        public const string WalkingIntelligenceTag = "Walking";
        protected Game.Game Game { get; }

        public abstract float Speed { get; }
        public Vector2 Direction { get; protected set; }
        public Vector2 Destignation { get; protected set; }

        public IPathFinder PathFinder { get; }
        private readonly CommandCancellation mCancellation = new CommandCancellation();
        private BTree mIntelligence;

        private readonly IBTreeBuilder mDefaultIntelligence;

        public sealed override float MaxHealth => MaxHealthBase;
        
        public string IntelligenceTag => mIntelligence.Tag;

        public Unit(Game.Game game, IPathFinder pathFinder, Vector2 position)
        {
            mInitialPosition = position;
            Game = game;
            PathFinder = pathFinder;
            mIntelligence = (mDefaultIntelligence = WrapCancellation(b => b, b => b, IdleIntelligenceTag)).Build();
        }
        
        protected virtual IBTreeBuilder GetDefaultIntelligence()
        {
            return mDefaultIntelligence;
        }

        public override void OnAddedToGame()
        {
            Destignation = Position = mInitialPosition;
            ApplyDefaultIntelligence();
            
            base.OnAddedToGame();
            PathFinder.SetTarget(Position, Game.Map.Data);
        }

        protected void ApplyDefaultIntelligence()
        {
            mIntelligence = GetDefaultIntelligence().Build();
        }

        protected IBTreeBuilder WrapCancellation(Func<IBTreeBuilder, IBTreeBuilder> createBody, Func<IBTreeBuilder, IBTreeBuilder> createCancel, string tag)
        {
            return BTree.Create(tag)
                .Sequence(b => b
                    .Selector(b1 => b1
                        .Leaf(new CheckCancellationLeaf(mCancellation))
                        .Fail(b2 => b2
                            .Sequence(b3 => 
                                createBody(b3)
                                .Leaf(new FreeIntelligenceLeaf(this)))))
                    .Sequence(b1 => 
                        createCancel(b1)
                        .Leaf(new FreeIntelligenceLeaf(this))
                        .Leaf(new ConfirmCancellationLeaf(mCancellation))));
        }

        protected async Task ApplyIntelligence(Func<IBTreeBuilder, IBTreeBuilder> createBody, Func<IBTreeBuilder, IBTreeBuilder> createCancel, string tag)
        {
            await mCancellation.Cancel();
            mIntelligence = WrapCancellation(createBody, createCancel, tag).Build();
        }

        public async Task GoTo(Vector2 position)
        {
            await ApplyIntelligence(
                b => b.Leaf(new GoToTargetLeaf(PathFinder, position, Game.Map.Data)),
                b => b.Leaf(new CancelGotoLeaf(PathFinder)), WalkingIntelligenceTag);
        }

        public Task Stop()
        {
            return mCancellation.Cancel();
        }

        public override void Update(TimeSpan deltaTime)
        {
            mIntelligence.Update(deltaTime);

            Position = PathFinder.CurrentPosition;
            Direction = PathFinder.CurrentDirection;
            Destignation = PathFinder.Target;
        }
    }
}