using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Utils;
using UnityEngine;

namespace Assets.Core.GameObjects.Final
{
    interface ICentralBuildingOrders : IBuildingOrders
    {
        Task<bool> QueueWorker();
        Task SetWaypoint(Vector2 waypoint);
    }

    interface ICentralBuildingInfo : IBuildingInfo
    {
        float Progress { get; }
        int WorkersQueued { get; }
    }

    internal class CentralBuilding : Building, ICentralBuildingInfo, ICentralBuildingOrders
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

        public float Progress { get; private set; }
        public int WorkersQueued { get; private set; }

        public const int WorkerCost = 30;
        public static Vector2 BuildingSize { get; } = new Vector2(2, 2);
        public const float MaximumHealthConst = 400;

        public CentralBuilding(Game.Game game, Vector2 position, IPlacementService placementService)
        {
            mGame = game;
            mPlacementService = placementService;
            Size = BuildingSize;
            Waypoint = Position = position;
            Health = MaxHealth = MaximumHealthConst;
        }

        public async Task<bool> QueueWorker()
        {
            if (!Player.Money.Spend(WorkerCost))
                return false;

            PlacementPoint point;
            if (!mPlacementService.TryAllocatePoint(out point))
                return false;

            WorkersQueued++;
            var productionTime = TimeSpan.FromSeconds(10);
            mOrders.Enqueue(new Order(productionTime, () =>
            {
                WorkersQueued--;
                var worker = Player.CreateWorker(point.Position);
                mGame.PlaceObject(worker);
                mPlacementService.ReleasePoint(point.ID);
                if (!new Rect(Position, Size).Contains(Waypoint))
                    worker.GoTo(Waypoint);
            }));
            return true;
        }

        public async Task SetWaypoint(Vector2 waypoint)
        {
            Waypoint = waypoint;
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