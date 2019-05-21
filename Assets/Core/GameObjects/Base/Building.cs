using UnityEngine;

namespace Assets.Core.GameObjects.Base
{
    interface IBuildingInfo : IGameObjectInfo
    {
        Vector2 Size { get; }
        Vector2 Waypoint { get; }
    }

    interface IBuildingOrders : IGameObjectOrders
    {
    }

    abstract class Building : RtsGameObject, IBuildingInfo, IBuildingOrders
    {
        public Vector2 Size { get; protected set; }
        public Vector2 Waypoint { get; protected set; }
    }
}