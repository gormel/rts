using System;
using System.Threading.Tasks;
using Assets.Core.GameObjects;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using UnityEngine;

namespace Assets.Core.Game
{
    interface IGameObjectFactory
    {
        Task<Worker> CreateWorker(Vector2 position);
        Task<RangedWarrior> CreateRangedWarrior(Vector2 position);
        Task<MeeleeWarrior> CreateMeeleeWarrior(Vector2 position);

        Task<BuildingTemplate> CreateBuildingTemplate(Vector2 position, Func<Vector2, Task<Building>> createBuilding, TimeSpan buildTime, Vector2 size, float maxHealth);

        Task<CentralBuilding> CreateCentralBuilding(Vector2 position);
        Task<Barrak> CreateBarrak(Vector2 position);
        Task<Turret> CreateTurret(Vector2 position);
        Task<MiningCamp> CreateMiningCamp(Vector2 position);
    }
}