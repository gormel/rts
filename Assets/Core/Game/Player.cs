using System;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using UnityEngine;

namespace Assets.Core.Game
{
    interface IPlayerState
    {
        Guid ID { get; }
        int Money { get; }
    }

    class Player : IGameObjectFactory, IPlayerState
    {
        private readonly IGameObjectFactory mExternalFactory;
        public ResourceStorage Money { get; } = new ResourceStorage();

        public Guid ID { get; } = Guid.NewGuid();

        int IPlayerState.Money => Money.Resources;

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
    }
}