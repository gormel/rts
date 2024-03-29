﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Core.BehaviorTree;
using Assets.Core.Game;
using Assets.Core.GameObjects.Utils;
using UnityEngine;

namespace Assets.Core.GameObjects.Base {

    interface IFactoryBuildingInfo : IBuildingInfo, IQueueOrdersInfo, IWaypointInfo
    {
    }

    interface IFactoryBuildingOrders : IBuildingOrders, IQueueOrdersOrders, IWaypointOrders
    {
    }

    abstract class FactoryBuilding : Building, IFactoryBuildingInfo, IFactoryBuildingOrders
    {
        private class Order
        {
            public TimeSpan Time { get; }
            private readonly Func<bool> mPrepare;
            private Action mDoing;
            private readonly Action mCancel;

            public Order(TimeSpan time, Func<bool> prepare, Action doing, Action cancel)
            {
                Time = time;
                mPrepare = prepare;
                mDoing = doing;
                mCancel = cancel;
            }

            public void Doing()
            {
                mDoing();
            }

            public bool Prepare()
            {
                return mPrepare();
            }

            public void Cancel()
            {
                mCancel();
            }
        }

        private class CheckOrderLeaf : IBTreeLeaf
        {
            private readonly FactoryBuilding mBuilding;

            public CheckOrderLeaf(FactoryBuilding building)
            {
                mBuilding = building;
            }

            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                return mBuilding.mLockedOrder == null ? BTreeLeafState.Failed : BTreeLeafState.Successed;
            }
        }

        private class LockOrderLeaf : IBTreeLeaf
        {
            private readonly FactoryBuilding mBuilding;

            public LockOrderLeaf(FactoryBuilding building)
            {
                mBuilding = building;
            }

            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                if (mBuilding.mOrders.Count < 1)
                    return BTreeLeafState.Failed;

                mBuilding.Progress = 0;
                mBuilding.mLockedOrder = mBuilding.mOrders.Dequeue();
                mBuilding.mLockedProgress = mBuilding.mLockedOrder.Time;
                return BTreeLeafState.Successed;
            }
        }

        private class WaitOrderLeaf : IBTreeLeaf
        {
            private readonly FactoryBuilding mBuilding;

            public WaitOrderLeaf(FactoryBuilding building)
            {
                mBuilding = building;
            }

            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                if (mBuilding.mLockedOrder == null)
                    return BTreeLeafState.Failed;

                if (mBuilding.mLockedProgress.TotalSeconds > 0)
                {
                    mBuilding.mLockedProgress -= deltaTime;
                    mBuilding.Progress = (float) (1 - mBuilding.mLockedProgress.TotalSeconds / mBuilding.mLockedOrder.Time.TotalSeconds);
                    return BTreeLeafState.Processing;
                }

                return mBuilding.mLockedOrder.Prepare() ? BTreeLeafState.Successed : BTreeLeafState.Processing;
            }
        }

        private class UnlockOrderLeaf : IBTreeLeaf
        {
            private readonly FactoryBuilding mBuilding;

            public UnlockOrderLeaf(FactoryBuilding building)
            {
                mBuilding = building;
            }

            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                if (mBuilding.mLockedOrder == null)
                    return BTreeLeafState.Failed;

                mBuilding.mLockedOrder.Doing();
                mBuilding.mLockedOrder = null;
                return BTreeLeafState.Successed;
            }
        }

        private readonly IndexedQueue<Order> mOrders = new IndexedQueue<Order>();
        private readonly BTree mIntelligence;
        private Order mLockedOrder;
        private TimeSpan mLockedProgress;

        public Vector2 Waypoint { get; protected set; }
        public int Queued { get; private set; }
        public float Progress { get; private set; }

        public FactoryBuilding(Game.Game game, Vector2 position, TimeSpan buildingTime, int buildingCost, IPlacementService placementService)
            : base(game, buildingTime, buildingCost, placementService)
        {
            Position = position;
            Waypoint = Position + Size / 2;
            mIntelligence = BTree.Create("Production").Sequence(b => b
                .Selector(b1 => b1
                    .Leaf(new CheckOrderLeaf(this))
                    .Leaf(new LockOrderLeaf(this)))
                .Leaf(new WaitOrderLeaf(this))
                .Leaf(new UnlockOrderLeaf(this))
            ).Build();
        }

        public Task SetWaypoint(Vector2 waypoint)
        {
            Waypoint = waypoint;
            return Task.CompletedTask;
        }

        protected async Task<bool> QueueUnit(int cost, TimeSpan productionTime, Func<IGameObjectFactory, Vector2, Task<Unit>> createUnit)
        {
            if (BuildingProgress != BuildingProgress.Complete)
                return false;
            
            if (!Player.Money.Spend(cost))
                return false;

            if (!Player.Limit.Spend(1))
            {
                Player.Money.Store(cost);
                return false;
            }

            bool taskProcessing = false;
            var allocatedPoint = PlacementPoint.Invalid;
            Queued++;
            mOrders.Enqueue(new Order(productionTime, () =>
            {
                if (taskProcessing)
                    return false;

                if (allocatedPoint != PlacementPoint.Invalid)
                    return true;

                taskProcessing = true;
                PlacementService.TryAllocateNearestPoint(Waypoint).ContinueWith(t =>
                {
                    allocatedPoint = t.Result;
                    taskProcessing = false;
                });
                return false;
            }, async () =>
            {
                Queued--;
                var unit = await createUnit(Player, allocatedPoint.Position);
                unit.RemovedFromGame += u => Player.Limit.Store(1);
                await Game.PlaceObject(unit);
                await PlacementService.ReleasePoint(allocatedPoint.ID);
                if (!new Rect(Position, Size).Contains(Waypoint))
                    await unit.GoTo(Waypoint);
            }, () =>
            {
                Queued--;
                Player.Limit.Store(1);
                Player.Money.Store(cost);
                PlacementService.ReleasePoint(allocatedPoint.ID);
            }));
            return true;
        }

        public override void Update(TimeSpan deltaTime)
        {
            base.Update(deltaTime);
            mIntelligence.Update(deltaTime);
        }

        public Task CancelOrderAt(int index)
        {
            if (index == 0)
            {
                if (mLockedOrder == null)
                    return Task.CompletedTask;
                
                mLockedOrder.Cancel();
                Progress = 0;
                mLockedProgress = TimeSpan.Zero;
                mLockedOrder = null;
                return Task.CompletedTask;
            }
            
            if (mOrders.TryRemoveAt(index - 1, out var order))
                order.Cancel();
            
            return Task.CompletedTask;
        }
    }
}