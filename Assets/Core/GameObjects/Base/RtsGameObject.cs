using System;
using System.Threading.Tasks;
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
        int Armour { get; }
    }

    interface IGameObjectOrders
    {
    }

    interface IQueueOrdersInfo : IGameObjectInfo
    {
        int Queued { get; }
        float Progress { get; }
    }

    interface IQueueOrdersOrders : IGameObjectOrders
    {
        Task CancelOrderAt(int index);
    }

    interface IWaypointInfo : IGameObjectInfo
    {
        Vector2 Waypoint { get; }
    }
    
    interface IWaypointOrders : IGameObjectOrders
    {
        Task SetWaypoint(Vector2 waypoint);
    }

    interface IAttackerInfo : IGameObjectInfo
    {
        bool IsAttacks { get; }
        float AttackRange { get; }
        float AttackSpeed { get; }
        int Damage { get; }
    }

    interface IAttackerOrders : IGameObjectOrders
    {
        Task Attack(Guid targetID);
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
        public abstract float ViewRadius { get; }
        public abstract int Armour { get; }

        protected abstract float MaxHealthBase { get; }
        protected abstract int ArmourBase { get; }

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