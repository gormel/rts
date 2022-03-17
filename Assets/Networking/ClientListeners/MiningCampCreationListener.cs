using System;
using System.Threading;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Final;
using Assets.Networking.ClientListeners;
using Assets.Utils;
using Grpc.Core;
using UnityEngine;

namespace Assets.Networking
{
    class ClientMiningCampOrders : IMinigCampOrders
    {
        private readonly MiningCampService.MiningCampServiceClient mClient;
        private readonly string mID;

        public ClientMiningCampOrders(MiningCampService.MiningCampServiceClient client, Guid id)
        {
            mClient = client;
            mID = id.ToString();
        }
        
        public Task FreeWorker()
        {
            return mClient.FreeWorkerAsync(new FreeWorkerRequest
            {
                CampID = new ID { Value = mID },
            }).ResponseAsync;
        }
    }
    class ClientMiningCampInfo : IMinigCampInfo, IStateHolder<MiningCampState>
    {
        public MiningCampState State { get; private set; } = new MiningCampState();

        public float MiningSpeed => State.MiningSpeed;
        public Vector2 Size => State.Base.Size.ToUnity();
        public Guid ID => Guid.Parse(State.Base.Base.ID.Value);
        public Guid PlayerID => Guid.Parse(State.Base.Base.PlayerID.Value);
        public Vector2 Position => State.Base.Base.Position.ToUnity();
        public float RecivedDamage => State.Base.Base.RecivedDamage;
        public float MaxHealth => State.Base.Base.MaxHealth;
        public float ViewRadius => State.Base.Base.ViewRadius;
        public int WorkerCount => State.WorkerCount;

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
            return new ClientMiningCampOrders(client, id);
        }

        protected override AsyncServerStreamingCall<MiningCampState> GetCreationCall(MiningCampService.MiningCampServiceClient client, CancellationToken token)
        {
            return client.ListenCreation(new Empty(), cancellationToken: token);
        }

        protected override AsyncServerStreamingCall<MiningCampState> GetUpdatesCall(MiningCampService.MiningCampServiceClient client, ID id, CancellationToken token)
        {
            return client.ListenState(id, cancellationToken: token);
        }
    }
}