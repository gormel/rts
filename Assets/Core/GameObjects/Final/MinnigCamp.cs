using Assets.Core.GameObjects.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Utils;
using UnityEngine;

namespace Assets.Core.GameObjects.Final
{
    interface IMinigCampInfo : IBuildingInfo, IWaypointInfo
    {
        float MiningSpeed { get; }
        int WorkerCount { get; }
    }

    interface IMinigCampOrders : IBuildingOrders, IWaypointOrders
    {
        Task FreeWorker();
        Task CollectWorkers();
    }

    class MiningCamp : Building, IMinigCampInfo, IMinigCampOrders
    {
        private readonly Game.Game mGame;
        private readonly Vector2 mInitialPosition;
        public IPlacementService PlacementService { get; }
        public static Vector2 BuildingSize { get; } = new Vector2(1, 1);
        public const float MaximumHealthConst = 100;
        public const int MaxWorkers = 4;
        public const float BaseMiningSpeed = 1.56f;
        public const float WorkerMiningSpeed = 0.39f;

        public float MiningSpeed => BaseMiningSpeed + WorkerMiningSpeed * mWorkers.Count;
        public int WorkerCount => mWorkers.Count;

        public Vector2 Waypoint { get; protected set; }

        private double mMinedTemp;
        private int mMinedTotal;

        private Stack<Worker> mWorkers = new Stack<Worker>();
        private List<Worker> mOrderedToWork = new List<Worker>();

        public override float ViewRadius => 2;
        protected override float MaxHealthBase => MaximumHealthConst;
        public override Vector2 Size => BuildingSize;

        public MiningCamp(Game.Game game, Vector2 position, IPlacementService placementService)
        {
            mGame = game;
            mInitialPosition = position;
            PlacementService = placementService;
        }

        public override void OnAddedToGame()
        {
            Waypoint = Position = mInitialPosition;
            
            base.OnAddedToGame();
        }

        public bool TryPutWorker(Worker worker)
        {
            if (mWorkers.Count >= MaxWorkers)
                return false;

            if (mWorkers.Contains(worker))
                return false;
            
            mWorkers.Push(worker);
            return true;
        }
        
        public override void Update(TimeSpan deltaTime)
        {
            mMinedTemp += MiningSpeed * deltaTime.TotalSeconds;
            if (mMinedTemp > 1)
            {
                var ceiled = Mathf.CeilToInt((float)mMinedTemp);
                Player.Money.Store(ceiled);
                mMinedTemp -= ceiled;
                mMinedTotal += ceiled;
            }
        }

        public override void OnRemovedFromGame()
        {
            base.OnRemovedFromGame();

            while (mWorkers.Count > 0)
            {
                var worker = mWorkers.Pop();
                mGame.RemoveObject(worker.ID);
            }
        }

        public async Task FreeWorker()
        {
            if (mWorkers.Count == 0)
                return;
            
            var point = await PlacementService.TryAllocateNearestPoint(Waypoint);
            if (point == PlacementPoint.Invalid)
                return;

            var unit = mWorkers.Pop();
            unit.IsAttachedToMiningCamp = false;
            await unit.PathFinder.Teleport(point.Position, mGame.Map.Data);
            if (!new Rect(Position, Size).Contains(Waypoint))
                await unit.GoTo(Waypoint);
            
            await PlacementService.ReleasePoint(point.ID);
        }

        public async Task CollectWorkers()
        {
            if (WorkerCount >= MaxWorkers)
                return;

            mOrderedToWork.RemoveAll(w =>
                w.IntelligenceTag != Worker.MiningIntelligenceTag || w.IsAttachedToMiningCamp);

            var found = mGame.QueryObjects(Position + Size / 2, ViewRadius * 2)
                .OfType<Worker>().Where(w => w.IntelligenceTag == Unit.IdleIntelligenceTag && !mOrderedToWork.Contains(w))
                .Take(4 - WorkerCount - mOrderedToWork.Count).ToList();
            
            mOrderedToWork.AddRange(found);

            foreach (var worker in found) 
                await worker.AttachToMiningCamp(ID);
        }

        public Task SetWaypoint(Vector2 waypoint)
        {
            Waypoint = waypoint;
            return Task.CompletedTask;
        }
    }
}
