using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using Assets.Views;
using UnityEngine;

namespace Assets.Core.Game
{
    interface IPlayerState
    {
        Guid ID { get; }
        int Money { get; }
        
        bool TurretBuildingAvaliable { get; }
        int TurretAttackUpgradeLevel { get; }
        int BuildingDefenceUpgradeLevel { get; }
        bool TurretAttackUpgradeAvaliable { get; }
        bool BuildingDefenceUpgradeAvaliable { get; }
    }

    class Player : IGameObjectFactory, IPlayerState
    {
        public class UpgradeStorage
        {
            public Upgrade<int> TurretAttackUpgrade { get; } = new Upgrade<int>(1, (atk, lvl) => atk + lvl);
            public Upgrade<float> BuildingDefenceUpgrade { get; } = new Upgrade<float>(1, (hp, lvl) => hp + 50 * lvl);
        }
        
        private readonly IGameObjectFactory mExternalFactory;
        private readonly Dictionary<Type, int> mCreatedBuildingRegistrations = new Dictionary<Type, int>();
        public ResourceStorage Money { get; } = new ResourceStorage();
        public bool TurretBuildingAvaliable => GetCreatedBuildingCount<BuildersLab>() > 0;
        public int TurretAttackUpgradeLevel => Upgrades.TurretAttackUpgrade.Level;
        public int BuildingDefenceUpgradeLevel => Upgrades.BuildingDefenceUpgrade.Level;
        public bool TurretAttackUpgradeAvaliable => Upgrades.TurretAttackUpgrade.LevelUpAvaliable;
        public bool BuildingDefenceUpgradeAvaliable => Upgrades.BuildingDefenceUpgrade.LevelUpAvaliable;

        public Guid ID { get; } = Guid.NewGuid();

        int IPlayerState.Money => Money.Resources;

        public UpgradeStorage Upgrades { get; } = new UpgradeStorage();
        
        public Player(IGameObjectFactory externalFactory)
        {
            mExternalFactory = externalFactory;
        }

        public Task<Worker> CreateWorker(Vector2 position)
        {
            return AssignPlayer(mExternalFactory.CreateWorker(position));
        }

        public Task<RangedWarrior> CreateRangedWarrior(Vector2 position)
        {
            return AssignPlayer(mExternalFactory.CreateRangedWarrior(position));
        }

        public Task<MeeleeWarrior> CreateMeeleeWarrior(Vector2 position)
        {
            return AssignPlayer(mExternalFactory.CreateMeeleeWarrior(position));
        }

        public Task<BuildingTemplate> CreateBuildingTemplate(Vector2 position, Func<Vector2, Task<Building>> createBuilding, TimeSpan buildTime, Vector2 size,
            float maxHealth)
        {
            return AssignPlayer(mExternalFactory.CreateBuildingTemplate(
                position,
                createBuilding,
                buildTime,
                size,
                maxHealth
            ));
        }

        public Task<CentralBuilding> CreateCentralBuilding(Vector2 position)
        {
            return AssignPlayer(mExternalFactory.CreateCentralBuilding(position));
        }

        public Task<Barrak> CreateBarrak(Vector2 position)
        {
            return AssignPlayer(mExternalFactory.CreateBarrak(position));
        }

        public Task<Turret> CreateTurret(Vector2 position)
        {
            return AssignPlayer(mExternalFactory.CreateTurret(position));
        }

        private async Task<T> AssignPlayer<T>(Task<T> controlled) where T : class, IPlayerControlled
        {
            var obj = await controlled;
            if (obj == null)
                return null;

            obj.Player = this;
            return obj;
        }

        public Task<MiningCamp> CreateMiningCamp(Vector2 position)
        {
            return AssignPlayer(mExternalFactory.CreateMiningCamp(position));
        }

        public Task<BuildersLab> CreateBuildersLab(Vector2 position)
        {
            return AssignPlayer(mExternalFactory.CreateBuildersLab(position));
        }

        public void RegisterCreatedBuilding(Type buildingType)
        {
            if (!mCreatedBuildingRegistrations.ContainsKey(buildingType))
                mCreatedBuildingRegistrations[buildingType] = 0;
            
            mCreatedBuildingRegistrations[buildingType]++;
        }

        public void FreeCreatedBuilding(Type buildingType)
        {
            if (!mCreatedBuildingRegistrations.ContainsKey(buildingType))
                return;
            
            mCreatedBuildingRegistrations[buildingType]--;
        }

        public int GetCreatedBuildingCount<TBuilding>() where TBuilding : Building
        {
            if (mCreatedBuildingRegistrations.TryGetValue(typeof(TBuilding), out int count))
                return count;

            return 0;
        }
    }
}