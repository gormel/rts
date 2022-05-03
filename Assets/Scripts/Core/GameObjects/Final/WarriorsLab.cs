using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Core.GameObjects.Base
{
    interface IWarriorsLabInfo : ILaboratoryBuildingInfo
    {
    }

    interface IWarriorsLabOrders : ILaboratoryBuildingOrders
    {
        Task QueueDamageUpgrade();
        Task QueueArmourUpgrade();
        Task QueueAttackRangeUpgrade();
    }
    class WarriorsLab : LaboratoryBuilding, IWarriorsLabInfo, IWarriorsLabOrders
    {
        public static Vector2 BuildingSize { get; } = new Vector2(2, 2);
        public const int MaximumHealthConst = 250;
        public static TimeSpan DamageUpgradeTime { get; } = TimeSpan.FromSeconds(20);
        public static TimeSpan ArmourUpgradeTime { get; } = TimeSpan.FromSeconds(15);
        public static TimeSpan AttackRangeUpgradeTime { get; } = TimeSpan.FromSeconds(22);
        
        public static int UnitArmourUpgradeCost { get; } = 300;
        public static int UnitDamageUpgradeCost { get; } = 350;
        public static int UnitAttackRangeUpgradeCost { get; } = 380;

        public override float ViewRadius => 3;
        protected override float MaxHealthBase => MaximumHealthConst;
        public override Vector2 Size => BuildingSize;
        
        public WarriorsLab(Vector2 position) 
            : base(position)
        {
        }

        public Task QueueDamageUpgrade()
        {
            return QueueUpgrade(Player.Upgrades.UnitDamageUpgrade, DamageUpgradeTime, UnitDamageUpgradeCost);
        }

        public Task QueueArmourUpgrade()
        {
            return QueueUpgrade(Player.Upgrades.UnitArmourUpgrade, ArmourUpgradeTime, UnitArmourUpgradeCost);
        }

        public Task QueueAttackRangeUpgrade()
        {
            return QueueUpgrade(Player.Upgrades.UnitAttackRangeUpgrade, AttackRangeUpgradeTime, UnitAttackRangeUpgradeCost);
        }
    }
}