using Assets.Core.GameObjects.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Utils;
using Assets.Core.Map;
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
        Task<Guid> FreeWorker();
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
        private List<RtsGameObject> mQueriedWorkers;

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

        public static bool CheckPlaceAllowed(IMapData mapData, Vector2Int at)
        {
            var dir = new Vector2Int(-1, 0);
            for (int i = 0; i < 4; i++)
            {
                var localPos = at + dir;
                if (!mapData.IsOutOfBounds(localPos) && mapData.GetMapObjectAt(localPos.x, localPos.y) == MapObject.Crystal)
                        return true;

                dir = new Vector2Int(dir.y, -dir.x); //rotate 90 deg
            }

            return false;
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

            var t = FreeAllWorkers();
        }

        private Task FreeAllWorkers()
        {
            return Task.WhenAll(mWorkers.Select(w => w.Stop()));
        }

        public async Task<Guid> FreeWorker()
        {
            if (mWorkers.Count == 0)
                return Guid.Empty;
            
            var point = await PlacementService.TryAllocateNearestPoint(Waypoint);
            if (point == PlacementPoint.Invalid)
                return Guid.Empty;

            var unit = mWorkers.Pop();
            await unit.Stop();
            await unit.PathFinder.Teleport(point.Position, mGame.Map.Data);
            if (!new Rect(Position, Size).Contains(Waypoint))
                await unit.GoTo(Waypoint);
            
            await PlacementService.ReleasePoint(point.ID);
            return unit.ID;
        }

        public async Task CollectWorkers()
        {
            if (WorkerCount >= MaxWorkers)
                return;

            mOrderedToWork.RemoveAll(w =>
                w.IntelligenceTag != Worker.MiningIntelligenceTag || w.IsAttachedToMiningCamp);

            mQueriedWorkers.Clear();
            mGame.QueryObjectsNoAlloc(PositionUtils.PositionOf(this), ViewRadius * 2, mQueriedWorkers);
            var found = mQueriedWorkers.OfType<Worker>()
                .Where(w => w.PlayerID == PlayerID)
                .Where(w => w.IntelligenceTag == Unit.IdleIntelligenceTag && !mOrderedToWork.Contains(w))
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
