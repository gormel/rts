using System;
using Assets.Core.Game;
using UnityEngine;

namespace Assets.Core.GameObjects.Base
{
    interface IGameObjectInfo
    {
        Guid ID { get; }
        Guid PlayerID { get; }
        Vector2 Position { get; }
        float Health { get; }
        float MaxHealth { get; }
    }

    interface IGameObjectOrders
    {
    }

    interface IPlayerControlled
    {
        Player Player { get; set; }
    }

    abstract class RtsGameObject : IGameObjectOrders, IGameObjectInfo, IPlayerControlled
    {
        public Guid ID { get; } = Guid.NewGuid();
        public Vector2 Position { get; protected set; }

        public float Health { get; set; }
        public float MaxHealth { get; set; }

        public abstract void Update(TimeSpan deltaTime);

        public Guid PlayerID => Player.ID;

        protected Player Player => ((IPlayerControlled)this).Player;
        Player IPlayerControlled.Player { get; set; }
    }
}