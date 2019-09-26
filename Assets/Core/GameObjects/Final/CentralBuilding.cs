using System;
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
        public const int WorkerCost = 30;
        public static Vector2 BuildingSize { get; } = new Vector2(2, 2);
        public const float MaximumHealthConst = 400;

        public float MiningSpeed { get; } = 2;
        private double mMinedTemp;

        public CentralBuilding(Game.Game game, Vector2 position, IPlacementService placementService)
            : base(game, position, placementService)
        {
            Size = BuildingSize;
            Health = MaxHealth = MaximumHealthConst;
        }

        public Task<bool> QueueWorker()
        {
            return QueueUnit(WorkerCost, async (f, p) => await f.CreateWorker(p));
        }

        public override void Update(TimeSpan deltaTime)
        {
            base.Update(deltaTime);

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