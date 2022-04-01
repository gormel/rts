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
    interface IMinigCampInfo : IBuildingInfo
    {
        float MiningSpeed { get; }
        int WorkerCount { get; }
    }

    interface IMinigCampOrders : IBuildingOrders
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

        private double mMinedTemp;
        private int mMinedTotal;

        private Stack<Worker> mWorkers = new Stack<Worker>();

        protected override float MaxHealthBase => MaximumHealthConst;

        public MiningCamp(Game.Game game, Vector2 position, IPlacementService placementService)
        {
            mGame = game;
            mInitialPosition = position;
            PlacementService = placementService;
        }

        public override void OnAddedToGame()
        {
            Position = mInitialPosition;
            Size = BuildingSize;
            ViewRadius = 2;
            
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
            
            var point = await PlacementService.TryAllocatePoint();
            if (point == PlacementPoint.Invalid)
                return;

            var unit = mWorkers.Pop();
            unit.IsAttachedToMiningCamp = false;
            await unit.PathFinder.Teleport(point.Position, mGame.Map.Data);
            await PlacementService.ReleasePoint(point.ID);
        }

        public Task CollectWorkers()
        {
            if (WorkerCount >= MaxWorkers)
                return Task.CompletedTask;
            
            var found = mGame.QueryObjects(Position + Size / 2, ViewRadius * 2)
                .OfType<Worker>().Where(w => !w.IsBuilding && w.PathFinder.IsArrived).Take(4 - WorkerCount);
            return Task.WhenAll(found.Select(w => w.AttachToMiningCamp(ID)));
        }
    }
}
