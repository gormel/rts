using System;
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
        Task AttachAsBuilder(Guid templateId);
        Task AttachToMiningCamp(Guid campId);
    }

    interface IWorkerInfo : IUnitInfo
    {
        bool IsBuilding { get; }
        bool IsAttachedToMiningCamp { get; }
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
            private readonly BuildingTemplate mTemplate;

            public BuildLeaf(Worker worker, BuildingTemplate template)
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
                    if (mTemplate.Progress < 1)
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
            private readonly BuildingTemplate mTemplate;

            public StopBuildLeaf(Worker worker, BuildingTemplate template)
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

        class MoveToMiningCampLeaf : IBTreeLeaf
        {
            private readonly Worker mWorker;
            private readonly MiningCamp mCamp;

            public MoveToMiningCampLeaf(Worker worker, MiningCamp camp)
            {
                mWorker = worker;
                mCamp = camp;
            }
            public BTreeLeafState Update(TimeSpan deltaTime)
            {
                if (!mCamp.TryPutWorker(mWorker))
                    return BTreeLeafState.Failed;

                mWorker.IsAttachedToMiningCamp = true;
                return BTreeLeafState.Successed;
            }
        }
        
        public const int CentralBuildingCost = 400;
        public const int MiningCampCost = 100;
        public const int BarrakCost = 200;

        public static TimeSpan CentralBuildingBuildTime { get; } = TimeSpan.FromSeconds(30);
        public static TimeSpan MiningCampBuildTime { get; } = TimeSpan.FromSeconds(20);
        public static TimeSpan BarrakBuildTime { get; } = TimeSpan.FromSeconds(25);

        public bool IsBuilding { get; private set; }
        
        public bool IsAttachedToMiningCamp { get; set; }

        public async Task<Guid> PlaceCentralBuildingTemplate(Vector2Int position)
        {
            if (!Game.GetIsAreaFree(position, CentralBuilding.BuildingSize))
                return Guid.Empty;

            if (!Player.Money.Spend(CentralBuildingCost))
                return Guid.Empty;

            var template = await Player.CreateBuildingTemplate(
                position,
                async pos => await Player.CreateCentralBuilding(pos),
                CentralBuildingBuildTime,
                CentralBuilding.BuildingSize,
                CentralBuilding.MaximumHealthConst
            );

            var id = await Game.PlaceObject(template);
            await AttachAsBuilder(id);
            return id;
        }

        public async Task<Guid> PlaceMiningCampTemplate(Vector2Int position)
        {
            if (!Game.GetIsAreaFree(position, MiningCamp.BuildingSize))
                return Guid.Empty;

            var crystalExist = false;
            var dir = new Vector2Int(-1, 0);
            for (int i = 0; i < 4; i++)
            {
                var localPos = position + dir;
                if (Game.Map.Data.GetMapObjectAt(localPos.x, localPos.y) == MapObject.Crystal)
                {
                    crystalExist = true;
                    break;
                }

                dir = new Vector2Int(dir.y, -dir.x); //rotate 90 deg
            }

            if (!crystalExist)
                return Guid.Empty;

            if (!Player.Money.Spend(MiningCampCost))
                return Guid.Empty;

            var template = await Player.CreateBuildingTemplate(
                position,
                async pos => await Player.CreateMiningCamp(pos),
                MiningCampBuildTime,
                MiningCamp.BuildingSize,
                MiningCamp.MaximumHealthConst
            );

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

            var template = await Player.CreateBuildingTemplate(
                position,
                async pos => await Player.CreateBarrak(pos),
                BarrakBuildTime,
                Barrak.BuildingSize,
                Barrak.MaximumHealthConst
            );

            var id = await Game.PlaceObject(template);
            await AttachAsBuilder(id);
            return id;
        }

        public Worker(Game.Game game, IPathFinder pathFinder, Vector2 position)
            : base(game, pathFinder, position)
        {
            Speed = 1.8f;
            MaxHealth = Health = 40;
            ViewRadius = 3;
        }

        public async Task AttachAsBuilder(Guid templateId)
        {
            var template = Game.GetObject<BuildingTemplate>(templateId);
            var point = await template.PlacementService.TryAllocatePoint();
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
                        .Leaf(new FreePlacementPointLeaf(point, template.PlacementService)))
                );
        }

        public async Task AttachToMiningCamp(Guid campId)
        {
            var camp = Game.GetObject<MiningCamp>(campId);
            var point = await camp.PlacementService.TryAllocatePoint();
            if (point == PlacementPoint.Invalid)
                return;

            await ApplyIntelligence(
                b => b
                    .Sequence(b1 => b1
                        .Leaf(new GoToTargetLeaf(PathFinder, point.Position, Game.Map.Data))
                        .Leaf(new MoveToMiningCampLeaf(this, camp))
                        .Leaf(new FreePlacementPointLeaf(point, camp.PlacementService))),
                b => b
                    .Leaf(new CancelGotoLeaf(PathFinder))
                    .Leaf(new FreePlacementPointLeaf(point, camp.PlacementService))
            );
        }
    }
}