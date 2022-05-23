﻿using System;
using System.Threading.Tasks;
using Assets.Core.GameObjects;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using Core.GameObjects.Final;
using UnityEngine;

namespace Assets.Core.Game
{
    interface IGameObjectFactory
    {
        Task<Worker> CreateWorker(Vector2 position);
        Task<RangedWarrior> CreateRangedWarrior(Vector2 position);
        Task<MeeleeWarrior> CreateMeeleeWarrior(Vector2 position);
        Task<Artillery> CreateArtillery(Vector2 position);

        Task<CentralBuilding> CreateCentralBuilding(Vector2 position);
        Task<Barrak> CreateBarrak(Vector2 position);
        Task<Turret> CreateTurret(Vector2 position);
        Task<MiningCamp> CreateMiningCamp(Vector2 position);
        Task<BuildersLab> CreateBuildersLab(Vector2 position);
        Task<WarriorsLab> CreateWarriorsLab(Vector2 position);
    }
}