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
        float ViewRadius { get; }
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

        public Guid PlayerID => Player.ID;

        protected Player Player => ((IPlayerControlled)this).Player;
        Player IPlayerControlled.Player { get; set; }

        public float Health { get; set; }
        public float MaxHealth { get; set; }
        public float ViewRadius { get; set; }

        public event Action<RtsGameObject> AddedToGame;
        public event Action<RtsGameObject> RemovedFromGame;

        public abstract void Update(TimeSpan deltaTime);

        public void OnAddedToGame()
        {
            AddedToGame?.Invoke(this);
        }

        public void OnRemovedFromGame()
        {
            RemovedFromGame?.Invoke(this);
        }
    }
}