using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using Assets.Views;
using Core.GameObjects.Final;
using UnityEngine;

namespace Assets.Core.Game
{
    interface IPlayerState
    {
        Guid ID { get; }
        int Money { get; }
        
        int Limit { get; }
        
        PlayerGameplateState GameplayState { get; }
        
        bool TurretBuildingAvaliable { get; }
        bool WarriorsLabBuildingAvaliable { get; }
        bool TurretAttackUpgradeAvaliable { get; }
        bool BuildingDefenceUpgradeAvaliable { get; }
        bool BuildingArmourUpgradeAvaliable { get; }
        bool UnitArmourUpgradeAvaliable { get; }
        bool UnitDamageUpgradeAvaliable { get; }
        bool UnitAttackRangeUpgradeAvaliable { get; }
        
        int Team { get; }
    }

    class Player : IGameObjectFactory, IPlayerState
    {
        public class UpgradeStorage
        {
            public Upgrade<int> TurretAttackUpgrade { get; } = new Upgrade<int>(1, (atk, lvl) => atk + lvl);
            public Upgrade<float> BuildingHealthUpgrade { get; } = new Upgrade<float>(1, (hp, lvl) => hp + 50 * lvl);
            public Upgrade<int> BuildingArmourUpgrade { get; } = new Upgrade<int>(1, (arm, lvl) => arm + lvl);//unset

            public Upgrade<int> UnitArmourUpgrade { get; } = new Upgrade<int>(1, (arm, lvl) => arm + lvl);
            public Upgrade<int> UnitDamageUpgrade { get; } = new Upgrade<int>(1, (dmg, lvl) => dmg + lvl);
            public Upgrade<int> UnitAttackRangeUpgrade { get; } = new Upgrade<int>(1, (rng, lvl) => rng + lvl);
        }

        public const int MaxLimit = 200;
        
        private readonly IGameObjectFactory mExternalFactory;
        private readonly Dictionary<Type, int> mCreatedBuildingRegistrations = new Dictionary<Type, int>();
        public ResourceStorage Money { get; } = new ResourceStorage();
        public ResourceStorage Limit { get; } = new ResourceStorage(MaxLimit);
        public PlayerGameplateState GameplayState { get; set; } = PlayerGameplateState.None;
        public bool TurretBuildingAvaliable => GetCreatedBuildingCount<BuildersLab>() > 0;
        public bool WarriorsLabBuildingAvaliable => GetCreatedBuildingCount<Barrak>() > 0;
        public bool TurretAttackUpgradeAvaliable => Upgrades.TurretAttackUpgrade.LevelUpAvaliable;
        public bool BuildingDefenceUpgradeAvaliable => Upgrades.BuildingHealthUpgrade.LevelUpAvaliable;
        public bool BuildingArmourUpgradeAvaliable => Upgrades.BuildingArmourUpgrade.LevelUpAvaliable;
        public bool UnitArmourUpgradeAvaliable => Upgrades.UnitArmourUpgrade.LevelUpAvaliable;
        public bool UnitDamageUpgradeAvaliable => Upgrades.UnitDamageUpgrade.LevelUpAvaliable;
        public bool UnitAttackRangeUpgradeAvaliable => Upgrades.UnitAttackRangeUpgrade.LevelUpAvaliable;

        public Guid ID { get; } = Guid.NewGuid();

        int IPlayerState.Money => Money.Resources;
        int IPlayerState.Limit => Limit.Resources;
        public int Team { get; }

        public UpgradeStorage Upgrades { get; } = new UpgradeStorage();
        
        public Player(IGameObjectFactory externalFactory, int team)
        {
            mExternalFactory = externalFactory;
            Team = team;
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

        public Task<Artillery> CreateArtillery(Vector2 position)
        {
            return AssignPlayer(mExternalFactory.CreateArtillery(position));
        }

        public Task<BuildingTemplate> CreateBuildingTemplate(Vector2 position, Func<Vector2, Task<Building>> createBuilding, TimeSpan buildTime, Vector2 size,
            float maxHealth, int cost)
        {
            return AssignPlayer(mExternalFactory.CreateBuildingTemplate(
                position,
                createBuilding,
                buildTime,
                size,
                maxHealth,
                cost
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
            
            if (obj is RtsGameObject rtsGameObj)
                OnObjectCreated(rtsGameObj);
            
            return obj;
        }

        protected virtual void OnObjectCreated(RtsGameObject obj)
        {
        }

        public Task<MiningCamp> CreateMiningCamp(Vector2 position)
        {
            return AssignPlayer(mExternalFactory.CreateMiningCamp(position));
        }

        public Task<BuildersLab> CreateBuildersLab(Vector2 position)
        {
            return AssignPlayer(mExternalFactory.CreateBuildersLab(position));
        }

        public Task<WarriorsLab> CreateWarriorsLab(Vector2 position)
        {
            return AssignPlayer(mExternalFactory.CreateWarriorsLab(position));
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