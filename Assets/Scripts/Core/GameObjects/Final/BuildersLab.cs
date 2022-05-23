using System;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using Assets.Core.GameObjects.Utils;
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
        
        public static readonly TimeSpan AttackUpgradeTime = TimeSpan.FromSeconds(20); 
        public static readonly TimeSpan DefenceUpgradeTime = TimeSpan.FromSeconds(15);

        public static int TurretAttackUpgradeCost { get; } = 250;
        public static int BuildingDefenceUpgradeCost { get; } = 200;

        public override float ViewRadius => 3;
        protected override float MaxHealthBase => MaximumHealthConst;
        public override Vector2 Size => BuildingSize;
        
        public BuildersLab(Vector2 position, IPlacementService placementService)
            : base(position, Worker.BuildersLabBuildTime, placementService)
        {
        }

        public Task QueueAttackUpgrade()
        {
            return QueueUpgrade(Player.Upgrades.TurretAttackUpgrade, AttackUpgradeTime, TurretAttackUpgradeCost);
        }

        public Task QueueDefenceUpgrade()
        {
            return QueueUpgrade(Player.Upgrades.BuildingHealthUpgrade, DefenceUpgradeTime, BuildingDefenceUpgradeCost);
        }
    }
}