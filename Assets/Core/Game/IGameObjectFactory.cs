using System;
using Assets.Core.GameObjects;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using UnityEngine;

namespace Assets.Core.Game
{
    interface IGameObjectFactory
    {
        Worker CreateWorker(Vector2 position);
        BuildingTemplate CreateBuildingTemplate(Vector2 position, Func<Vector2, Building> createBuilding, TimeSpan buildTime, Vector2 size, float maxHealth);
        CentralBuilding CreateCentralBuilding(Vector2 position);
    }
}