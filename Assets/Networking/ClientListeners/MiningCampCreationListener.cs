using System;
using Assets.Core.GameObjects.Final;
using Assets.Networking.ClientListeners;
using Assets.Utils;
using Grpc.Core;
using UnityEngine;

namespace Assets.Networking
{
    class ClientMiningCampOrders : IMinigCampOrders {}
    class ClientMiningCampInfo : IMinigCampInfo, IStateHolder<MiningCampState>
    {
        public MiningCampState State { get; private set; } = new MiningCampState();

        public float MiningSpeed => State.MiningSpeed;
        public Vector2 Size => State.Base.Size.ToUnity();
        public Guid ID => Guid.Parse(State.Base.Base.ID.Value);
        public Guid PlayerID => Guid.Parse(State.Base.Base.PlayerID.Value);
        public Vector2 Position => State.Base.Base.Position.ToUnity();
        public float Health => State.Base.Base.Health;
        public float MaxHealth => State.Base.Base.MaxHealth;

        public void ResetState()
        {
            State = new MiningCampState();
        }
    }

    class MiningCampCreationListener : CommonCreationStateListener<
        IMinigCampOrders,
        IMinigCampInfo,
        ClientMiningCampOrders,
        ClientMiningCampInfo,
        MiningCampState,
        MiningCampService.MiningCampServiceClient
        >
    {
        public MiningCampCreationListener(UnitySyncContext syncContext)
            : base(syncContext)
        {
        }

        protected override MiningCampService.MiningCampServiceClient CreateClient(Channel channel)
        {
            return new MiningCampService.MiningCampServiceClient(channel);
        }

        protected override ClientMiningCampOrders CreateOrders(MiningCampService.MiningCampServiceClient client, Guid id)
        {
            return new ClientMiningCampOrders();
        }

        protected override AsyncServerStreamingCall<MiningCampState> GetCreationCall(MiningCampService.MiningCampServiceClient client)
        {
            return client.ListenCreation(new Empty());
        }

        protected override AsyncServerStreamingCall<MiningCampState> GetUpdatesCall(MiningCampService.MiningCampServiceClient client, ID id)
        {
            return client.ListenState(id);
        }
    }
}