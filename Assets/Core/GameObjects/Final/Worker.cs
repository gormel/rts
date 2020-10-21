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
    }

    interface IWorkerInfo : IUnitInfo
    {
        bool IsBuilding { get; }
    }

    internal class Worker : Unit, IWorkerInfo, IWorkerOrders
    {
        private class BuildOrder : Order
        {
            private readonly Worker mWorker;
            private readonly BuildingTemplate mTemplate;
            private PlacementPoint mPlacementPoint = PlacementPoint.Invalid;
            private bool mAllocating = true;
            private bool mArrived = false;

            public BuildOrder(Worker worker, BuildingTemplate template)
            {
                mWorker = worker;
                mTemplate = template;
            }

            public override BTreeLeafState Update(TimeSpan deltaTime)
            {
                if (mAllocating)
                    return BTreeLeafState.Processing;

                if (mPlacementPoint == PlacementPoint.Invalid)
                    return BTreeLeafState.Failed;

                if (!mArrived)
                    return BTreeLeafState.Processing;

                mWorker.IsBuilding = true;
                mWorker.PathFinder.SetLookAt(mTemplate.Position + mTemplate.Size / 2, mWorker.Game.Map.Data);

                return mTemplate.Progress >= 1 ? BTreeLeafState.Successed : BTreeLeafState.Processing;
            }

            public override async void Begin()
            {
                //pin as unmovable
                mPlacementPoint = await mTemplate.PlacementService.TryAllocatePoint();
                mAllocating = false;

                if (mPlacementPoint != PlacementPoint.Invalid)
                {
                    mWorker.PathFinder.Arrived += PathFinderOnArrived;
                    await mWorker.PathFinder.SetTarget(mPlacementPoint.Position, mWorker.Game.Map.Data);
                }
            }

            private void PathFinderOnArrived()
            {
                mTemplate.AttachedWorkers++;
                mArrived = true;
            }

            public override async void End()
            {
                mWorker.PathFinder.Arrived -= PathFinderOnArrived;
                mWorker.IsBuilding = false;
                if (await mTemplate.PlacementService.ReleasePoint(mPlacementPoint.ID) && mArrived)
                    mTemplate.AttachedWorkers--;
            }
        }

        public const int CentralBuildingCost = 400;
        public const int MiningCampCost = 100;
        public const int BarrakCost = 200;

        public static TimeSpan CentralBuildingBuildTime { get; } = TimeSpan.FromSeconds(30);
        public static TimeSpan MiningCampBuildTime { get; } = TimeSpan.FromSeconds(20);
        public static TimeSpan BarrakBuildTime { get; } = TimeSpan.FromSeconds(25);

        public bool IsBuilding { get; private set; }

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
            SetOrder(new BuildOrder(this, template));
        }
    }
}