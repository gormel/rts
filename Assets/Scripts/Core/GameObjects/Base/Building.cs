using System;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Final;
using Assets.Core.GameObjects.Utils;
using UnityEngine;

namespace Assets.Core.GameObjects.Base
{
    interface IBuildingInfo : IGameObjectInfo
    {
        Vector2 Size { get; }
        BuildingProgress BuildingProgress { get; }
    }

    interface IBuildingOrders : IGameObjectOrders
    {
        Task CancelBuilding();
    }

    abstract class Building : RtsGameObject, IBuildingInfo, IBuildingOrders
    {
        private readonly int mBuildingCost;
        public abstract Vector2 Size { get; }
        public BuildingProgress BuildingProgress { get; private set; }
        public sealed override float MaxHealth => Player.Upgrades.BuildingHealthUpgrade.Calculate(MaxHealthBase);
        public sealed override int Armour => Player.Upgrades.BuildingArmourUpgrade.Calculate(ArmourBase);
        public int AttachedWorkers { get; set; }
        public Game.Game Game { get; }
        public IPlacementService PlacementService { get; }
        protected override int ArmourBase => 1;

        private TimeSpan mProgress;
        private TimeSpan mFullBuildTime;

        public Building(Game.Game game, TimeSpan buildingTime, int buildingCost, IPlacementService placementService)
        {
            mBuildingCost = buildingCost;
            Game = game;
            PlacementService = placementService;
            BuildingProgress = BuildingProgress.Building;
            mProgress = mFullBuildTime = buildingTime;
        }

        public void CompleteBuilding()
        {
            BuildingProgress = BuildingProgress.Complete;
            RecivedDamage = 0;
        }

        public override void Update(TimeSpan deltaTime)
        {
            if (BuildingProgress == BuildingProgress.Building)
            {
                var multiplyer = AttachedWorkers == 0 ? 0 : 1 + AttachedWorkers * 0.3f;
                mProgress -= deltaTime * multiplyer;
                RecivedDamage = Math.Max(RecivedDamage - (float)(deltaTime.TotalSeconds * multiplyer / mFullBuildTime.TotalSeconds * MaxHealth), 0);
                if (mProgress < TimeSpan.Zero)
                    BuildingProgress = BuildingProgress.Complete;
            }
        }

        public override void OnAddedToGame()
        {
            base.OnAddedToGame();

            RecivedDamage = MaxHealth - 1;
            Player.RegisterCreatedBuilding(GetType());
        }

        public override void OnRemovedFromGame()
        {
            base.OnRemovedFromGame();
            
            Player.FreeCreatedBuilding(GetType());
        }

        public Task CancelBuilding()
        {
            Player.Money.Store(mBuildingCost);
            Game.RemoveObject(ID);
            return Task.CompletedTask;
        }
    }
}