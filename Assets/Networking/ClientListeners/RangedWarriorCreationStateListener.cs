using System;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using Assets.Networking.ClientListeners;
using Assets.Utils;
using Grpc.Core;
using UnityEngine;

namespace Assets.Networking
{
    class ClientRangedWarriorState : IRangedWarriorInfo, IStateHolder<RangedWarriorState>
    {
        public RangedWarriorState State { get; } = new RangedWarriorState();

        public Guid ID => Guid.Parse(State.Base.Base.ID.Value);
        public Guid PlayerID => Guid.Parse(State.Base.Base.PlayerID.Value);
        public Vector2 Position => State.Base.Base.Position.ToUnity();
        public float Health => State.Base.Base.Health;
        public float MaxHealth => State.Base.Base.MaxHealth;
        public float Speed => State.Base.Speed;
        public Vector2 Direction => State.Base.Direction.ToUnity();
        public Vector2 Destignation => State.Base.Destignation.ToUnity();
    }

    class ClientRangedWarriorOrders : IRangedWarriorOrders
    {
        private readonly RangedWarriorService.RangedWarriorServiceClient mClient;
        private readonly string mID;

        public ClientRangedWarriorOrders(RangedWarriorService.RangedWarriorServiceClient client, Guid id)
        {
            mClient = client;
            mID = id.ToString();
        }

        public Task GoTo(Vector2 position)
        {
            return mClient.GoToAsync(new GoToRequest
            {
                Destignation = position.ToGrpc(),
                UnitID = new ID { Value = mID }
            }).ResponseAsync;
        }

        public Task Attack(Guid targetID)
        {
            return mClient.AttackAsync(new AttackRequest
            {
                TargetID = new ID { Value = targetID.ToString() },
                WarriorID = new ID { Value = mID }
            }).ResponseAsync;
        }
    }

    class RangedWarriorCreationStateListener : CommonCreationStateListener<
        IRangedWarriorOrders,
        IRangedWarriorInfo,
        ClientRangedWarriorOrders,
        ClientRangedWarriorState,
        RangedWarriorState,
        RangedWarriorService.RangedWarriorServiceClient>
    {
        public RangedWarriorCreationStateListener(UnitySyncContext syncContext)
            : base(syncContext) { }

        protected override IAsyncStreamReader<RangedWarriorState> GetCreationStream(RangedWarriorService.RangedWarriorServiceClient client)
        {
            return client.ListenCreation(new Empty()).ResponseStream;
        }

        protected override IAsyncStreamReader<RangedWarriorState> GetUpdatesStream(RangedWarriorService.RangedWarriorServiceClient client, ID id)
        {
            return client.ListenState(id).ResponseStream;
        }

        protected override RangedWarriorService.RangedWarriorServiceClient CreateClient(Channel channel)
        {
            return new RangedWarriorService.RangedWarriorServiceClient(channel);
        }

        protected override ClientRangedWarriorOrders CreateOrders(RangedWarriorService.RangedWarriorServiceClient client, Guid id)
        {
            return new ClientRangedWarriorOrders(client, id);
        }
    }
}