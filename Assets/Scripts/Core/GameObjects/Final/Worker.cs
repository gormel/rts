﻿using System;
using System.Threading.Tasks;
using Assets.Core.BehaviorTree;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Utils;
using Assets.Core.Map;
using UnityEngine;

namespace Assets.Core.GameObjects.Final
{
    interface IWorkerOrders : IUnitOrders
    {
        Task<Guid> PlaceCentralBuildingTemplate(Vector2Int position);
        Task<Guid> PlaceMiningCampTemplate(Vector2Int position);
        Task<Guid> PlaceBarrakTemplate(Vector2Int position);
        Task<Guid> PlaceTurretTemplate(Vector2Int position);
        Task<Guid> PlaceBuildersLabTemplate(Vector2Int position);
        Task<Guid> PlaceWarriorsLabTemplate(Vector2Int position);
        Task AttachAsBuilder(Guid templateId);
        Task AttachToMiningCamp(Guid campId);
    }

    interface IWorkerInfo : IUnitInfo
    {
        bool IsBuilding { get; }
        bool IsAttachedToMiningCamp { get; }
        
        WorkerMovementType MovementType { get; }
    }

    internal class Worker : Unit, IWorkerInfo, IWorkerOrders
    {
        class FreePlacementPointLeaf : IBTreeLeaf
        {
            private readonly PlacementPoint mPoint;
            private readonly IPlacementService mPlacementService;

            public FreePlacementPointLeaf(PlacementPoint point, IPlacementService placementService)
            {
                mPoint = point;
                mPlacementService = placementService;
            }
            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                mPlacementService.ReleasePoint(mPoint.ID);
                return BTreeLeafState.Successed;
            }
        }

        class BuildLeaf : IBTreeLeaf
        {
            private readonly Worker mWorker;
            private readonly Building mTemplate;

            public BuildLeaf(Worker worker, Building template)
            {
                mWorker = worker;
                mTemplate = template;
            }
            
            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                if (!mTemplate.IsInGame)
                    return BTreeLeafState.Failed;
                
                if (mWorker.IsBuilding)
                {
                    if (mTemplate.BuildingProgress == BuildingProgress.Building)
                        return BTreeLeafState.Processing;
                    
                    return BTreeLeafState.Successed;
                }

                mWorker.IsBuilding = true;
                mTemplate.AttachedWorkers++;
                return BTreeLeafState.Processing;
            }
        }

        class StopBuildLeaf : IBTreeLeaf
        {
            private readonly Worker mWorker;
            private readonly Building mTemplate;

            public StopBuildLeaf(Worker worker, Building template)
            {
                mWorker = worker;
                mTemplate = template;
            }

            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                if (!mWorker.IsBuilding)
                    return BTreeLeafState.Successed;
                
                mTemplate.AttachedWorkers--;
                mWorker.IsBuilding = false;
                return BTreeLeafState.Successed;
            }
        }

        class MiningLeaf : IBTreeLeaf
        {
            private readonly IMapData mMapData;
            private readonly Worker mWorker;
            private readonly MiningCamp mCamp;

            public MiningLeaf(IMapData mapData, Worker worker, MiningCamp camp)
            {
                mMapData = mapData;
                mWorker = worker;
                mCamp = camp;
            }
            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                if (mWorker.IsAttachedToMiningCamp)
                    return BTreeLeafState.Processing;
                
                if (!mCamp.TryPutWorker(mWorker))
                    return BTreeLeafState.Successed;

                mWorker.IsAttachedToMiningCamp = true;
                mWorker.PathFinder.Teleport(mCamp.Position + mCamp.Size / 2, mMapData);
                return BTreeLeafState.Processing;
            }
        }

        class CancelMiningLeaf : IBTreeLeaf
        {
            private readonly Worker mWorker;

            public CancelMiningLeaf(Worker worker)
            {
                mWorker = worker;
            }

            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                mWorker.IsAttachedToMiningCamp = false;
                return BTreeLeafState.Successed;
            }
        }

        public static TimeSpan CentralBuildingBuildTime { get; } = TimeSpan.FromSeconds(30);
        public static TimeSpan MiningCampBuildTime { get; } = TimeSpan.FromSeconds(20);
        public static TimeSpan BarrakBuildTime { get; } = TimeSpan.FromSeconds(25);
        public static TimeSpan TurretBuildTime { get; } = TimeSpan.FromSeconds(15);
        public static TimeSpan BuildersLabBuildTime { get; } = TimeSpan.FromSeconds(25);
        public static TimeSpan WarriorsLabBuildTime { get; } = TimeSpan.FromSeconds(25);
        
        public static int CentralBuildingCost { get; } = 400;
        public static int MiningCampCost { get; } = 100;
        public static int BarrakCost { get; } = 200;
        public static int TurretCost { get; } = 100;
        public static int BuildersLabCost { get; } = 200;
        public static int WarriorsLabCost { get; } = 200;

        public const string BuildingIntelligenceTag = "Building";
        public const string MiningIntelligenceTag = "Mining";

        public bool IsBuilding { get; private set; }
        
        public bool IsAttachedToMiningCamp { get; set; }

        public WorkerMovementType MovementType
        {
            get
            {
                switch (IntelligenceTag)
                {
                    case BuildingIntelligenceTag:
                        return WorkerMovementType.Attach;
                    case MiningIntelligenceTag:
                        return WorkerMovementType.Attach;
                }

                return WorkerMovementType.Common;
            }
        }

        public override float ViewRadius => 3;
        public override float Speed => 1.8f;
        public override int Armour => ArmourBase;
        
        protected override float MaxHealthBase => 40;
        protected override int ArmourBase => 1;

        public Worker(Game.Game game, IPathFinder pathFinder, Vector2 position)
            : base(game, pathFinder, position)
        {
        }

        public async Task<Guid> PlaceCentralBuildingTemplate(Vector2Int position)
        {
            if (!Game.GetIsAreaFree(position, CentralBuilding.BuildingSize))
                return Guid.Empty;

            if (!Player.Money.Spend(CentralBuildingCost))
                return Guid.Empty;

            var template = await Player.CreateCentralBuilding(position);

            var id = await Game.PlaceObject(template);
            await AttachAsBuilder(id);
            return id;
        }

        public async Task<Guid> PlaceMiningCampTemplate(Vector2Int position)
        {
            if (!Game.GetIsAreaFree(position, MiningCamp.BuildingSize))
                return Guid.Empty;

            if (!MiningCamp.CheckPlaceAllowed(Game.Map.Data, position))
                return Guid.Empty;

            if (!Player.Money.Spend(MiningCampCost))
                return Guid.Empty;

            var template = await Player.CreateMiningCamp(position);

            var id = await Game.PlaceObject(template);
            await AttachAsBuilder(id);
            return id;
        }

        public async Task<Guid> PlaceBarrakTemplate(Vector2Int position)
        {
            if (!Game.GetIsAreaFree(position, Barrak.BuildingSize))
                return Guid.Empty;

            if (!Player.Money.Spend(BarrakCost))
                return Guid.Empty;

            var template = await Player.CreateBarrak(position);

            var id = await Game.PlaceObject(template);
            await AttachAsBuilder(id);
            return id;
        }

        public async Task<Guid> PlaceTurretTemplate(Vector2Int position)
        {
            if (!Game.GetIsAreaFree(position, Turret.BuildingSize))
                return Guid.Empty;
            
            if (!Player.TurretBuildingAvaliable)
                return Guid.Empty;

            if (!Player.Money.Spend(TurretCost))
                return Guid.Empty;

            var template = await Player.CreateTurret(position);

            var id = await Game.PlaceObject(template);
            await AttachAsBuilder(id);
            return id;
        }

        public async Task<Guid> PlaceBuildersLabTemplate(Vector2Int position)
        {
            if (!Game.GetIsAreaFree(position, BuildersLab.BuildingSize))
                return Guid.Empty;

            if (!Player.Money.Spend(BuildersLabCost))
                return Guid.Empty;

            var template = await Player.CreateBuildersLab(position);

            var id = await Game.PlaceObject(template);
            await AttachAsBuilder(id);
            return id;
        }

        public async Task<Guid> PlaceWarriorsLabTemplate(Vector2Int position)
        {
            if (!Game.GetIsAreaFree(position, WarriorsLab.BuildingSize))
                return Guid.Empty;
            
            if (!Player.WarriorsLabBuildingAvaliable)
                return Guid.Empty;

            if (!Player.Money.Spend(WarriorsLabCost))
                return Guid.Empty;

            var template = await Player.CreateWarriorsLab(position);

            var id = await Game.PlaceObject(template);
            await AttachAsBuilder(id);
            return id;
        }

        public async Task AttachAsBuilder(Guid templateId)
        {
            var template = Game.GetObject<Building>(templateId);
            var point = await template.PlacementService.TryAllocateNearestPoint(Position);
            if (point == PlacementPoint.Invalid)
                return;

            await ApplyIntelligence(
                b => b
                    .Sequence(b1 => b1
                        .Leaf(new GoToTargetLeaf(PathFinder, point.Position, Game.Map.Data))
                        .Leaf(new RotateToLeaf(PathFinder, template.Position + template.Size / 2, Game.Map.Data))
                        .Success(b2 => b2.Leaf(new BuildLeaf(this, template)))
                        .Leaf(new StopBuildLeaf(this, template))
                        .Leaf(new FreePlacementPointLeaf(point, template.PlacementService))), 
                b => b
                    .Sequence(b1 => b1
                        .Leaf(new CancelGotoLeaf(PathFinder))
                        .Leaf(new StopBuildLeaf(this, template))
                        .Leaf(new FreePlacementPointLeaf(point, template.PlacementService))),
                BuildingIntelligenceTag
                );
        }

        public async Task AttachToMiningCamp(Guid campId)
        {
            var camp = Game.GetObject<MiningCamp>(campId);
            if (camp.PlayerID != PlayerID)
                return;
            
            var point = await camp.PlacementService.TryAllocateNearestPoint(Position);
            if (point == PlacementPoint.Invalid)
                return;

            await ApplyIntelligence(
                b => b
                    .Sequence(b1 => b1
                        .Leaf(new GoToTargetLeaf(PathFinder, point.Position, Game.Map.Data))
                        .Leaf(new FreePlacementPointLeaf(point, camp.PlacementService))
                        .Leaf(new MiningLeaf(Game.Map.Data, this, camp))
                    ),
                b => b
                    .Leaf(new CancelGotoLeaf(PathFinder))
                    .Leaf(new FreePlacementPointLeaf(point, camp.PlacementService))
                    .Leaf(new CancelMiningLeaf(this)),
                MiningIntelligenceTag
            );
        }
    }
}