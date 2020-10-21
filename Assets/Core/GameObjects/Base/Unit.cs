using System;
using System.Collections.Generic;
using System.Linq;
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
    }

    interface IUnitInfo : IGameObjectInfo
    {
        float Speed { get; }
        Vector2 Direction { get; }
        Vector2 Destignation { get; }
    }

    abstract class Unit : RtsGameObject, IUnitInfo, IUnitOrders
    {
        private class TrySetOrderLeaf : IBTreeLeaf
        {
            private readonly Unit mOwner;

            public TrySetOrderLeaf(Unit owner)
            {
                mOwner = owner;
            }

            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                if (mOwner.mOrders.Count == 0)
                    return mOwner.mLockedOrder == null ? BTreeLeafState.Failed : BTreeLeafState.Successed;

                if (mOwner.mLockedOrder != null)
                    return BTreeLeafState.Successed;

                mOwner.mLockedOrder = mOwner.mOrders.Dequeue();
                mOwner.mLockedOrder.Begin();
                return BTreeLeafState.Successed;
            }
        }

        private class OrderLeaf : IBTreeLeaf
        {
            private readonly Unit mOwner;

            public OrderLeaf(Unit owner)
            {
                mOwner = owner;
            }

            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                if (mOwner.mLockedOrder == null)
                    return BTreeLeafState.Failed;

                return mOwner.mLockedOrder.Update(deltaTime);
            }
        }

        private class RemoveOrderLeaf : IBTreeLeaf
        {
            private readonly Unit mOwner;

            public RemoveOrderLeaf(Unit owner)
            {
                mOwner = owner;
            }

            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                if (mOwner.mLockedOrder == null)
                    return BTreeLeafState.Failed;

                mOwner.mLockedOrder.End();
                mOwner.mLockedOrder = null;
                return BTreeLeafState.Successed;
            }
        }

        protected abstract class Order
        {
            public abstract BTreeLeafState Update(TimeSpan deltaTime);
            public abstract void Begin();
            public abstract void End();
        }

        protected class GoToOrder : Order
        {
            private readonly Unit mOwner;
            private readonly Vector2 mTarget;
            private bool mArrived = false;

            public GoToOrder(Unit owner, Vector2 target)
            {
                mOwner = owner;
                mTarget = target;
            }

            public override BTreeLeafState Update(TimeSpan deltaTime)
            {
                return mArrived ? BTreeLeafState.Successed : BTreeLeafState.Processing;
            }

            public override void Begin()
            {
                mOwner.PathFinder.Arrived += PathFinderOnArrived;
                mOwner.PathFinder.SetTarget(mTarget, mOwner.Game.Map.Data);
            }

            private void PathFinderOnArrived()
            {
                mArrived = true;
            }

            public override void End()
            {
                mOwner.PathFinder.Arrived -= PathFinderOnArrived;
                mOwner.PathFinder.Stop();
            }
        }

        protected Game.Game Game { get; }

        public float Speed { get; protected set; }
        public Vector2 Direction { get; protected set; }
        public Vector2 Destignation { get; protected set; }

        protected IPathFinder PathFinder { get; }

        private Order mLockedOrder;
        private Queue<Order> mOrders = new Queue<Order>();
        private BTree mIntelligence;
        
        public Unit(Game.Game game, IPathFinder pathFinder, Vector2 position)
        {
            Game = game;
            PathFinder = pathFinder;
            Destignation = Position = position;
            mIntelligence = ExtendLogic(BTree.Build()
                .Sequence(b => b
                    .Leaf(new TrySetOrderLeaf(this))
                    .Leaf(new OrderLeaf(this))
                    .Leaf(new RemoveOrderLeaf(this)))).Build();
        }

        protected virtual IBTreeBuilder ExtendLogic(IBTreeBuilder baseLogic)
        {
            return baseLogic;
        }

        protected void SetOrder(Order order)
        {
            mLockedOrder?.End();
            mLockedOrder = null;
            mOrders.Clear();
            mOrders.Enqueue(order);
        }

        protected void AddOrder(Order order)
        {
            mOrders.Enqueue(order);
        }

        public async Task GoTo(Vector2 position)
        {
            SetOrder(new GoToOrder(this, position));
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