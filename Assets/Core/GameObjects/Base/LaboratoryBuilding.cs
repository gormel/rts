using System;
using System.Threading.Tasks;
using Assets.Core.Game;
using Assets.Core.GameObjects.Base;
using UnityEngine;

namespace Assets.Core.GameObjects.Base
{
    interface ILaboratoryBuildingInfo : IBuildingInfo
    {
        float Progress { get; }
        int Queued { get; }
    }

    interface ILaboratoryBuildingOrders : IBuildingOrders
    {
    }

    abstract class LaboratoryBuilding : Building, ILaboratoryBuildingInfo, ILaboratoryBuildingOrders
    {
        private readonly Vector2 mInitialPosition;
        public float Progress { get; }
        public int Queued { get; }

        public LaboratoryBuilding(Vector2 position)
        {
            mInitialPosition = position;
        }

        public override void OnAddedToGame()
        {
            Position = mInitialPosition;
            base.OnAddedToGame();
        }

        protected Task QueueUpgrade<T>(Upgrade<T> upgrade)
        {
            upgrade.LevelUp();
            return Task.CompletedTask;
        }

        public override void Update(TimeSpan deltaTime)
        {
        }
    }
}