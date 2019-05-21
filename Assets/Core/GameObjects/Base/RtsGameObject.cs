using System;
using UnityEngine;

namespace Assets.Core.GameObjects.Base
{
    interface IGameObjectInfo
    {
        Guid ID { get; }
        Vector2 Position { get; }
        float Health { get; }
        float MaxHealth { get; }
    }

    interface IGameObjectOrders
    {
    }

    abstract class RtsGameObject : IGameObjectOrders, IGameObjectInfo
    {
        public Guid ID { get; set; }
        public Vector2 Position { get; protected set; }

        public float Health { get; set; }
        public float MaxHealth { get; set; }

        public abstract void Update(TimeSpan deltaTime);
    }
}