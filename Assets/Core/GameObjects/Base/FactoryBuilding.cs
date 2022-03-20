using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Core.BehaviorTree;
using Assets.Core.Game;
using Assets.Core.GameObjects.Utils;
using UnityEngine;

namespace Assets.Core.GameObjects.Base {

    interface IFactoryBuildingInfo : IBuildingInfo, IQueueOrdersInfo
    {
        Vector2 Waypoint { get; }
    }

    interface IFactoryBuildingOrders : IBuildingOrders
    {
        Task SetWaypoint(Vector2 waypoint);
    }

    abstract class FactoryBuilding : Building, IFactoryBuildingInfo, IFactoryBuildingOrders
    {
        private class Order
        {
            public TimeSpan Time { get; }
            private readonly Func<bool> mPrepare;
            private Action mDoing;

            public Order(TimeSpan time, Func<bool> prepare, Action doing)
            {
                Time = time;
                mPrepare = prepare;
                mDoing = doing;
            }

            public void Doing()
            {
                mDoing();
            }

            public bool Prepare()
            {
                return mPrepare();
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

        private readonly Game.Game mGame;
        private readonly Vector2 mInitialPosition;
        private readonly IPlacementService mPlacementService;
        private readonly Queue<Order> mOrders = new Queue<Order>();
        private readonly BTree mIntelligence;
        private Order mLockedOrder;
        private TimeSpan mLockedProgress;

        public Vector2 Waypoint { get; protected set; }
        public int Queued { get; private set; }
        public float Progress { get; private set; }

        public FactoryBuilding(Game.Game game, Vector2 position, IPlacementService placementService)
        {
            mGame = game;
            mInitialPosition = position;
            mPlacementService = placementService;
            mIntelligence = BTree.Create().Sequence(b => b
                .Selector(b1 => b1
                    .Leaf(new CheckOrderLeaf(this))
                    .Leaf(new LockOrderLeaf(this)))
                .Leaf(new WaitOrderLeaf(this))
                .Leaf(new UnlockOrderLeaf(this))
            ).Build();
        }

        public override void OnAddedToGame()
        {
            Waypoint = Position = mInitialPosition;
            base.OnAddedToGame();
        }

        public Task SetWaypoint(Vector2 waypoint)
        {
            Waypoint = waypoint;
            return Task.CompletedTask;
        }

        protected async Task<bool> QueueUnit(int cost, TimeSpan productionTime, Func<IGameObjectFactory, Vector2, Task<Unit>> createUnit)
        {
            if (!Player.Money.Spend(cost))
                return false;

            if (!Player.Limit.Spend(1))
                return false;

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
                mPlacementService.TryAllocateNearestPoint(Waypoint).ContinueWith(t =>
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
                await mGame.PlaceObject(unit);
                await mPlacementService.ReleasePoint(allocatedPoint.ID);
                if (!new Rect(Position, Size).Contains(Waypoint))
                    await unit.GoTo(Waypoint);
            }));
            return true;
        }

        public override void Update(TimeSpan deltaTime)
        {
            mIntelligence.Update(deltaTime);
        }
    }
}