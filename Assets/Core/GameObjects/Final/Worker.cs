using System;
using System.Threading.Tasks;
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
        Task AttachAsBuilder(Guid templateId);
    }

    interface IWorkerInfo : IUnitInfo
    {
    }

    internal class Worker : Unit, IWorkerInfo, IWorkerOrders
    {
        private class BuildOrder : UnitOrder
        {
            private readonly BuildingTemplate mTemplate;
            private readonly PlacementPoint mPlacementPoint;

            public BuildOrder(BuildingTemplate template, PlacementPoint placementPoint)
            {
                mTemplate = template;
                mPlacementPoint = placementPoint;
            }

            protected override Task OnBegin()
            {
                mTemplate.AttachedWorkers++;
                return Task.CompletedTask;
            }

            protected override void OnUpdate(TimeSpan deltaTime)
            {
                if (mTemplate.Progress >= 1)
                {
                    End();
                    OnCancel();
                }
            }

            protected override void OnCancel()
            {
                mTemplate.PlacementService.ReleasePoint(mPlacementPoint.ID);

                if (State == OrderState.Work)
                    mTemplate.AttachedWorkers--;
            }
        }

        public const int CentralBuildingCost = 400;
        public const int MiningCampCost = 100;

        public static TimeSpan CentralBuildingBuildTime { get; } = TimeSpan.FromSeconds(30);
        public static TimeSpan MiningCampBuildTime { get; } = TimeSpan.FromSeconds(20);

        public async Task<Guid> PlaceCentralBuildingTemplate(Vector2Int position)
        {
            if (!Player.Money.Spend(CentralBuildingCost))
                return Guid.Empty;

            if (!Game.GetIsAreaFree(position, CentralBuilding.BuildingSize))
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
            if (!Player.Money.Spend(MiningCampCost))
                return Guid.Empty;

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

        public Worker(Game.Game game, IPathFinder pathFinder, Vector2 position)
            : base(game, pathFinder, position)
        {
            Speed = 3;
            MaxHealth = Health = 40;
        }

        public async Task AttachAsBuilder(Guid templateId)
        {
            var template = Game.GetObject<BuildingTemplate>(templateId);
            var point = await template.PlacementService.TryAllocatePoint();
            if (point == PlacementPoint.Invalid)
                return;

            SetOrder(new GoToOrder(this, point.Position).Then(new BuildOrder(template, point)));
        }
    }
}