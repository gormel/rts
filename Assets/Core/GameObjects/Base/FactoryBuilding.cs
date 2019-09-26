using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

            public bool Working => mCurrentRemanding > TimeSpan.Zero;
            public float Progress => Mathf.Min((float)(1 - mCurrentRemanding.TotalSeconds / Time.TotalSeconds), 1);

            private TimeSpan mCurrentRemanding;

            public Order(TimeSpan time, Action doing)
            {
                mCurrentRemanding = Time = time;
                mDoing = doing;
            }

            public void Update(TimeSpan deltaTime)
            {
                mCurrentRemanding -= deltaTime;
            }

            public void Doing()
            {
                mDoing();
            }
        }

        private readonly Game.Game mGame;
        private readonly IPlacementService mPlacementService;
        private readonly Queue<Order> mOrders = new Queue<Order>();

        public Vector2 Waypoint { get; protected set; }
        public int Queued { get; private set; }
        public float Progress { get; private set; }

        public FactoryBuilding(Game.Game game, Vector2 position, IPlacementService placementService)
        {
            mGame = game;
            mPlacementService = placementService;
            Waypoint = Position = position;
        }

        public Task SetWaypoint(Vector2 waypoint)
        {
            Waypoint = waypoint;
            return Task.CompletedTask;
        }

        protected async Task<bool> QueueUnit(int cost, Func<IGameObjectFactory, Vector2, Task<Unit>> createUnit)
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
            var productionTime = TimeSpan.FromSeconds(10);
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
            if (mOrders.Count <= 0)
                return;

            mOrders.Peek().Update(deltaTime);

            Progress = mOrders.Peek().Progress;
            if (mOrders.Peek().Working)
                return;

            var toExecute = mOrders.Dequeue();
            toExecute.Doing();
            Progress = 0;
        }
    }
}