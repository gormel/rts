using System;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using UnityEngine;

namespace Assets.Core.Game
{
    class Player : IGameObjectFactory
    {
        private readonly IGameObjectFactory mExternalFactory;
        public ResourceStorage Money { get; } = new ResourceStorage();

        public Player(IGameObjectFactory externalFactory)
        {
            mExternalFactory = externalFactory;
        }

        public Worker CreateWorker(Vector2 position)
        {
            return AssignPlayer(mExternalFactory.CreateWorker(position));
        }

        public BuildingTemplate CreateBuildingTemplate(Vector2 position, Func<Vector2, Building> createBuilding, TimeSpan buildTime, Vector2 size,
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

        public CentralBuilding CreateCentralBuilding(Vector2 position)
        {
            return AssignPlayer(mExternalFactory.CreateCentralBuilding(position));
        }

        private T AssignPlayer<T>(T controlled) where T : class, IPlayerControlled
        {
            if (controlled == null)
                return null;

            controlled.Player = this;
            return controlled;
        }
    }
}