using System;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using UnityEngine;

namespace Assets.Core.GameObjects.Base
{
    interface IBuildersLabInfo : ILaboratoryBuildingInfo
    {
    }

    interface IBuildersLabOrders : ILaboratoryBuildingOrders
    {
        Task QueueAttackUpgrade();
        Task QueueDefenceUpgrade();
    }

    class BuildersLab : LaboratoryBuilding, IBuildersLabInfo, IBuildersLabOrders
    {
        public static Vector2 BuildingSize = new Vector2(2, 2);
        public const int MaximumHealthConst = 250;
        
        public BuildersLab(Vector2 position) 
            : base(position)
        {
            Size = BuildingSize;
            Health = MaxHealth = MaximumHealthConst;
            ViewRadius = 3;
        }
        
        public Task QueueAttackUpgrade()
        {
            return QueueUpgrade();
        }

        public Task QueueDefenceUpgrade()
        {
            return QueueUpgrade();
        }
    }
}