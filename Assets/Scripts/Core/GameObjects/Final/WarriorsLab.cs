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
        public static Vector2 BuildingSize = new Vector2(2, 2);
        public const int MaximumHealthConst = 250;
        public static TimeSpan DamageUpgradeTime { get; } = TimeSpan.FromSeconds(20);
        public static TimeSpan ArmourUpgradeTime { get; } = TimeSpan.FromSeconds(15);
        public static TimeSpan AttackRangeUpgradeTime { get; } = TimeSpan.FromSeconds(22);

        public override float ViewRadius => 3;
        protected override float MaxHealthBase => MaximumHealthConst;
        public override Vector2 Size => BuildingSize;
        
        public WarriorsLab(Vector2 position) 
            : base(position)
        {
        }

        public Task QueueDamageUpgrade()
        {
            if (!Player.Money.Spend(Player.UnitDamageUpgradeCost))
                return Task.CompletedTask;

            return QueueUpgrade(Player.Upgrades.UnitDamageUpgrade, DamageUpgradeTime);
        }

        public Task QueueArmourUpgrade()
        {
            if (!Player.Money.Spend(Player.UnitArmourUpgradeCost))
                return Task.CompletedTask;

            return QueueUpgrade(Player.Upgrades.UnitArmourUpgrade, ArmourUpgradeTime);
        }

        public Task QueueAttackRangeUpgrade()
        {
            if (!Player.Money.Spend(Player.UnitAttackRangeUpgradeCost))
                return Task.CompletedTask;

            return QueueUpgrade(Player.Upgrades.UnitAttackRangeUpgrade, AttackRangeUpgradeTime);
        }
    }
}