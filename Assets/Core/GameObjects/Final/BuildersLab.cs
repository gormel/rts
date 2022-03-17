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
        
        public static readonly TimeSpan AttackUpgradeTime = TimeSpan.FromSeconds(20); 
        public static readonly TimeSpan DefenceUpgradeTime = TimeSpan.FromSeconds(15);

        public const int AttackUpgradeCost = 250;
        public const int DefenceUpgradeCost = 200;

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
            if (!Player.Money.Spend(AttackUpgradeCost))
                return Task.CompletedTask;
            
            return QueueUpgrade(Player.Upgrades.TurretAttackUpgrade, AttackUpgradeTime);
        }

        public Task QueueDefenceUpgrade()
        {
            if (!Player.Money.Spend(DefenceUpgradeCost))
                return Task.CompletedTask;

            return QueueUpgrade(Player.Upgrades.BuildingDefenceUpgrade, DefenceUpgradeTime);
        }
    }
}