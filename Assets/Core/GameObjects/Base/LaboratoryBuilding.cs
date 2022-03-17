using System;
using System.Threading.Tasks;
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

    class LaboratoryBuilding : Building, ILaboratoryBuildingInfo, ILaboratoryBuildingOrders
    {
        public float Progress { get; }
        public int Queued { get; }

        public LaboratoryBuilding(Vector2 position)
        {
            Position = position;
        }
        
        protected Task QueueUpgrade()
        {
            return Task.CompletedTask;
        }

        public override void Update(TimeSpan deltaTime)
        {
        }
    }
}