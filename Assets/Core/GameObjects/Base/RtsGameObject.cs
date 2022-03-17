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
        float RecivedDamage { get; }
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

        public float RecivedDamage { get; set; }
        public abstract float MaxHealth { get; }
        public float ViewRadius { get; set; }
        
        protected abstract float MaxHealthBase { get; }

        public bool IsInGame { get; private set; } = false;

        public event Action<RtsGameObject> AddedToGame;
        public event Action<RtsGameObject> RemovedFromGame;

        public abstract void Update(TimeSpan deltaTime);

        public virtual void OnAddedToGame()
        {
            IsInGame = true;
            AddedToGame?.Invoke(this);
        }

        public virtual void OnRemovedFromGame()
        {
            IsInGame = false;
            RemovedFromGame?.Invoke(this);
        }
    }
}