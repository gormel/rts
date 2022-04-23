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
        private readonly Func<Vector2, Task<Building>> mCreateBuilding;
        private readonly Vector2 mInitialSize;
        private readonly Vector2 mInitialPosition;
        public IPlacementService PlacementService { get; }
        private TimeSpan mBuildTime;
        private readonly TimeSpan mFullBuildTime;
        private readonly float mMaxHealthBase;

        private const float WorkerInvest = 0.3f;

        public float Progress { get; private set; }
        public int AttachedWorkers { get; set; }

        public override float ViewRadius => 1;
        public override Vector2 Size => mInitialSize;
        protected override float MaxHealthBase => mMaxHealthBase;

        public BuildingTemplate(
            Game.Game game,
            Func<Vector2, Task<Building>> createBuilding, 
            TimeSpan buildTime, 
            Vector2 size, 
            Vector2 position, 
            float maxHealth, 
            IPlacementService placementService)
        {
            mGame = game;
            mCreateBuilding = createBuilding;
            mInitialSize = size;
            mInitialPosition = position;
            PlacementService = placementService;
            mFullBuildTime = mBuildTime = buildTime;
            mMaxHealthBase = maxHealth;
        }

        public override void OnAddedToGame()
        {
            Position = mInitialPosition;
            RecivedDamage = MaxHealth - 5;
            
            base.OnAddedToGame();
        }

        public override void Update(TimeSpan deltaTime)
        {
            var k = AttachedWorkers <= 0 ? 0 : 1;
            mBuildTime = mBuildTime.Subtract(TimeSpan.FromSeconds(deltaTime.TotalSeconds * MathUtils.Pow(1 + WorkerInvest, AttachedWorkers - 1) * k));
            var progressBefore = Progress;
            Progress = (float)(1 - mBuildTime.TotalSeconds / mFullBuildTime.TotalSeconds);
            var deltaDamage = (Progress - progressBefore) * MaxHealth;
            RecivedDamage = Math.Max(0, RecivedDamage - deltaDamage);

            if (mBuildTime <= TimeSpan.Zero)
            {
                mGame.RemoveObject(ID);
                mCreateBuilding(Position).ContinueWith(t => mGame.PlaceObject(t.Result));
            }
        }

        public async Task Cancel()
        {
            await mGame.RemoveObject(ID);
        }
    }
}