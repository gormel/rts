﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Utils;
using UnityEngine;

namespace Assets.Core.GameObjects.Final
{
    interface ICentralBuildingOrders : IFactoryBuildingOrders
    {
        Task<bool> QueueWorker();
    }

    interface ICentralBuildingInfo : IFactoryBuildingInfo
    {
    }

    internal class CentralBuilding : FactoryBuilding, ICentralBuildingInfo, ICentralBuildingOrders
    {
        public static readonly TimeSpan WorkerProductionTime = TimeSpan.FromSeconds(10);
        
        public static int WorkerCost { get; } = 30;

        public static Vector2 BuildingSize { get; } = new Vector2(2, 2);
        public const float MaximumHealthConst = 400;

        public float MiningSpeed { get; } = 0.3f;
        private double mMinedTemp;

        public override float ViewRadius => 3;
        protected override float MaxHealthBase => MaximumHealthConst;

        public override Vector2 Size => BuildingSize;

        public CentralBuilding(Game.Game game, Vector2 position, IPlacementService placementService)
            : base(game, position, Worker.CentralBuildingBuildTime, Worker.CentralBuildingCost, placementService)
        {
        }

        public Task<bool> QueueWorker()
        {
            return QueueUnit(WorkerCost, WorkerProductionTime, async (f, p) => await f.CreateWorker(p));
        }

        public override void Update(TimeSpan deltaTime)
        {
            base.Update(deltaTime);

            if (BuildingProgress == BuildingProgress.Complete)
            {
                mMinedTemp += MiningSpeed * deltaTime.TotalSeconds;
                if (mMinedTemp > 1)
                {
                    var ceiled = Mathf.CeilToInt((float)mMinedTemp);
                    Player.Money.Store(ceiled);
                    mMinedTemp -= ceiled;
                }
            }
        }
    }
}