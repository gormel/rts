using System;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Utils;
using Assets.Utils;
using UnityEngine;

namespace Assets.Core.GameObjects.Final
{
    interface IBuildingTemplateInfo : IBuildingInfo
    {
        float Progress { get; }
        int AttachedWorkers { get; }
    }

    interface IBuildingTemplateOrders : IBuildingOrders
    {
        Task Cancel();
    }

    internal class BuildingTemplate : Building, IBuildingTemplateInfo, IBuildingTemplateOrders
    {
        private readonly Game.Game mGame;
        private readonly Func<Vector2, Building> mCreateBuilding;
        public IPlacementService PlacementService { get; }
        private TimeSpan mBuildTime;
        private readonly TimeSpan mFullBuildTime;

        private const float WorkerInvest = 0.3f;

        public float Progress { get; private set; }
        public int AttachedWorkers { get; set; }

        public BuildingTemplate(
            Game.Game game,
            Func<Vector2, Building> createBuilding, 
            TimeSpan buildTime, 
            Vector2 size, 
            Vector2 position, 
            float maxHealth, 
            IPlacementService placementService)
        {
            mGame = game;
            mCreateBuilding = createBuilding;
            PlacementService = placementService;
            mFullBuildTime = mBuildTime = buildTime;
            Size = size;
            Position = position;
            MaxHealth = maxHealth;
            Health = 5;
        }

        public override void Update(TimeSpan deltaTime)
        {
            var k = AttachedWorkers <= 0 ? 0 : 1;
            mBuildTime = mBuildTime.Subtract(TimeSpan.FromSeconds(deltaTime.TotalSeconds * MathUtils.Pow(1 + WorkerInvest, AttachedWorkers - 1) * k));
            Progress = (float)(1 - mBuildTime.TotalSeconds / mFullBuildTime.TotalSeconds);
            Health = Math.Max(Progress * MaxHealth, 5);

            if (mBuildTime <= TimeSpan.Zero)
            {
                mGame.RemoveObject(ID);
                mGame.PlaceObject(mCreateBuilding(Position));
            }
        }

        public async Task Cancel()
        {
            await mGame.RemoveObject(ID);
        }
    }
}