using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using Assets.Utils;
using Grpc.Core;
using UnityEngine;

namespace Assets.Networking.ClientListeners
{

    class ClientMeeleeWarriorState : IMeeleeWarriorInfo, IStateHolder<MeeleeWarriorState>
    {
        public MeeleeWarriorState State { get; private set; } = new MeeleeWarriorState();

        public Guid ID => Guid.Parse(State.Base.Base.Base.ID.Value);
        public Guid PlayerID => Guid.Parse(State.Base.Base.Base.PlayerID.Value);
        public Vector2 Position => State.Base.Base.Base.Position.ToUnity();
        public float Health => State.Base.Base.Base.Health;
        public float MaxHealth => State.Base.Base.Base.MaxHealth;
        public float Speed => State.Base.Base.Speed;
        public Vector2 Direction => State.Base.Base.Direction.ToUnity();
        public Vector2 Destignation => State.Base.Base.Destignation.ToUnity();
        public bool IsAttacks => State.Base.IsAttacks;
        public float AttackRange => State.Base.AttackRange;
        public float AttackSpeed => State.Base.AttackSpeed;
        public int Damage => State.Base.Damage;
        public Strategy Strategy => (Strategy)State.Base.Strategy;
        public float ViewRadius => State.Base.Base.Base.ViewRadius;

        public void ResetState()
        {
            State = new MeeleeWarriorState();
        }
    }

    class ClientMeeleeWarriorOrders : IMeeleeWarriorOrders
    {
        private readonly MeeleeWarriorService.MeeleeWarriorServiceClient mClient;
        private readonly string mID;

        public ClientMeeleeWarriorOrders(MeeleeWarriorService.MeeleeWarriorServiceClient client, Guid id)
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

        public Task SetStrategy(Strategy strategy)
        {
            return mClient.SetStrategyAsync(new SetStrategyRequest
            {
                WarriorID = new ID() { Value = mID },
                Strategy = (int)strategy
            }).ResponseAsync;
        }
    }

    class MeeleeWarriorCreationStateListener : CommonCreationStateListener<
        IMeeleeWarriorOrders,
        IMeeleeWarriorInfo,
        ClientMeeleeWarriorOrders,
        ClientMeeleeWarriorState,
        MeeleeWarriorState,
        MeeleeWarriorService.MeeleeWarriorServiceClient>
    {
        public MeeleeWarriorCreationStateListener(UnitySyncContext syncContext)
            : base(syncContext) { }

        protected override AsyncServerStreamingCall<MeeleeWarriorState> GetCreationCall(MeeleeWarriorService.MeeleeWarriorServiceClient client, CancellationToken token)
        {
            return client.ListenCreation(new Empty(), cancellationToken: token);
        }

        protected override AsyncServerStreamingCall<MeeleeWarriorState> GetUpdatesCall(MeeleeWarriorService.MeeleeWarriorServiceClient client, ID id, CancellationToken token)
        {
            return client.ListenState(id, cancellationToken: token);
        }

        protected override MeeleeWarriorService.MeeleeWarriorServiceClient CreateClient(Channel channel)
        {
            return new MeeleeWarriorService.MeeleeWarriorServiceClient(channel);
        }

        protected override ClientMeeleeWarriorOrders CreateOrders(MeeleeWarriorService.MeeleeWarriorServiceClient client, Guid id)
        {
            return new ClientMeeleeWarriorOrders(client, id);
        }
    }
}
