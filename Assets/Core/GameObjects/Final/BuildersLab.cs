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

        protected override float MaxHealthBase => MaximumHealthConst;
        
        public BuildersLab(Vector2 position) 
            : base(position)
        {
        }

        public override void OnAddedToGame()
        {
            Size = BuildingSize;
            ViewRadius = 3;
            
            base.OnAddedToGame();
        }

        public Task QueueAttackUpgrade()
        {
            return QueueUpgrade(Player.Upgrades.TurretAttackUpgrade);
        }

        public Task QueueDefenceUpgrade()
        {
            return QueueUpgrade(Player.Upgrades.BuildingDefenceUpgrade);
        }
    }
}