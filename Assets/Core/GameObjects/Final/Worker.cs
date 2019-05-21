﻿using System;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Utils;
using Assets.Core.Map;
using UnityEngine;

namespace Assets.Core.GameObjects.Final
{
    interface IWorkerOrders : IUnitOrders
    {
        Task<BuildingTemplate> PlaceCentralBuildingTemplate(Vector2Int position);
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
            private bool mBegan;

            public BuildOrder(BuildingTemplate template, PlacementPoint placementPoint)
            {
                mTemplate = template;
                mPlacementPoint = placementPoint;
            }

            protected override void OnBegin()
            {
                mTemplate.AttachedWorkers++;
                mBegan = true;
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

                if (mBegan)
                    mTemplate.AttachedWorkers--;
            }
        }

        public const int CentralBuildingCost = 400;
        public static TimeSpan CentralBuildingBuildTime { get; } = TimeSpan.FromSeconds(30);
        
        public async Task<BuildingTemplate> PlaceCentralBuildingTemplate(Vector2Int position)
        {
            if (!Player.Money.Spend(CentralBuildingCost))
                return null;

            if (!mGame.GetIsAreaFree(position, CentralBuilding.BuildingSize))
                return null;

            var template = Player.CreateBuildingTemplate(
                position,
                pos => Player.CreateCentralBuilding(pos),
                CentralBuildingBuildTime,
                CentralBuilding.BuildingSize,
                CentralBuilding.MaximumHealthConst
            );

            var id = await mGame.PlaceObject(template);
            await AttachAsBuilder(id);
            return template;
        }

        public Worker(Game.Game game, IPathFinder pathFinder, Vector2 position)
            : base(game, pathFinder, position)
        {
            Speed = 3;
            MaxHealth = Health = 40;
        }

        public async Task AttachAsBuilder(Guid templateId)
        {
            var template = mGame.GetObject<BuildingTemplate>(templateId);
            PlacementPoint point;
            if (!template.PlacementService.TryAllocatePoint(out point))
                return;

            SetOrder(new OrderSequence(
                new GoToOrder(this, point.Position), 
                new BuildOrder(template, point))
            );
        }
    }
}