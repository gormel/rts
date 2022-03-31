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
        public static readonly TimeSpan WorkerProductionTime = TimeSpan.FromSeconds(10);

        public static Vector2 BuildingSize { get; } = new Vector2(2, 2);
        public const float MaximumHealthConst = 400;

        public float MiningSpeed { get; } = 0.3f;
        private double mMinedTemp;

        protected override float MaxHealthBase => MaximumHealthConst;

        public CentralBuilding(Game.Game game, Vector2 position, IPlacementService placementService)
            : base(game, position, placementService)
        {
        }

        public override void OnAddedToGame()
        {
            Size = BuildingSize;
            ViewRadius = 3;
            
            base.OnAddedToGame();
        }

        public Task<bool> QueueWorker()
        {
            return QueueUnit(Player.WorkerCost, WorkerProductionTime, async (f, p) => await f.CreateWorker(p));
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