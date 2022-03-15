﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Final;
using Assets.Utils;
using Grpc.Core;
using UnityEngine;

namespace Assets.Networking.ClientListeners
{
    class ClientTurretInfo : ITurretInfo, IStateHolder<TurretState> {
        public TurretState State { get; private set; } = new TurretState();
        public void ResetState()
        {
            State = new TurretState();
        }

        public Guid ID => Guid.Parse(State.Base.Base.ID.Value);
        public Guid PlayerID => Guid.Parse(State.Base.Base.PlayerID.Value);
        public Vector2 Position => State.Base.Base.Position.ToUnity();
        public float Health => State.Base.Base.Health;
        public float MaxHealth => State.Base.Base.MaxHealth;
        public float ViewRadius => State.Base.Base.ViewRadius;
        public Vector2 Size => State.Base.Size.ToUnity();

        public bool IsShooting => State.IsAttacks;
        public Vector2 Direction => State.Direction.ToUnity();
        public float AttackRange => State.AttackRange;
        public float AttackSpeed => State.AttackSpeed;
        public int Damage => State.Damage;
    }

    class ClientTurretOrders : ITurretOrders
    {
        private readonly TurretService.TurretServiceClient mClient;
        private readonly string mID;

        public ClientTurretOrders(TurretService.TurretServiceClient client, string id)
        {
            mClient = client;
            mID = id;
        }
    }
    
    class TurretCreationListener : CommonCreationStateListener<
        ITurretOrders,
        ITurretInfo,
        ClientTurretOrders,
        ClientTurretInfo,
        TurretState,
        TurretService.TurretServiceClient
    >
    {
        public TurretCreationListener(UnitySyncContext syncContext) 
            : base(syncContext)
        {
        }

        protected override AsyncServerStreamingCall<TurretState> GetCreationCall(TurretService.TurretServiceClient client, CancellationToken token)
        {
            return client.ListenCreation(new Empty(), cancellationToken: token);
        }

        protected override AsyncServerStreamingCall<TurretState> GetUpdatesCall(TurretService.TurretServiceClient client, ID id, CancellationToken token)
        {
            return client.ListenState(id, cancellationToken: token);
        }

        protected override TurretService.TurretServiceClient CreateClient(Channel channel)
        {
            return new TurretService.TurretServiceClient(channel);
        }

        protected override ClientTurretOrders CreateOrders(TurretService.TurretServiceClient client, Guid id)
        {
            return new ClientTurretOrders(client, id.ToString());
        }
    }
}