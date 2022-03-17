using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Core.BehaviorTree;
using Assets.Core.Game;
using Assets.Core.GameObjects.Utils;
using UnityEngine;

namespace Assets.Core.GameObjects.Base {

    interface IFactoryBuildingInfo : IBuildingInfo
    {
        Vector2 Waypoint { get; }
        int Queued { get; }
        float Progress { get; }
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
            private Action mDoing;

            public Order(TimeSpan time, Action doing)
            {
                Time = time;
                mDoing = doing;
            }

            public void Doing()
            {
                mDoing();
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

                mBuilding.mLockedProgress -= deltaTime;
                mBuilding.Progress = (float)(1 - mBuilding.mLockedProgress.TotalSeconds / mBuilding.mLockedOrder.Time.TotalSeconds);
                return mBuilding.mLockedProgress.TotalSeconds > 0 ? BTreeLeafState.Processing : BTreeLeafState.Successed;
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
            var point = await mPlacementService.TryAllocatePoint();
            if (point == PlacementPoint.Invalid)
                return false;

            if (!Player.Money.Spend(cost))
            {
                await mPlacementService.ReleasePoint(point.ID);
                return false;
            }

            Queued++;
            mOrders.Enqueue(new Order(productionTime, async () =>
            {
                Queued--;
                var unit = await createUnit(Player, point.Position);
                await mGame.PlaceObject(unit);
                await mPlacementService.ReleasePoint(point.ID);
                if (!new Rect(Position, Size).Contains(Waypoint))
                    await unit.GoTo(Waypoint); ;
            }));
            return true;
        }

        public override void Update(TimeSpan deltaTime)
        {
            mIntelligence.Update(deltaTime);
        }
    }
}