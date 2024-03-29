using System;
using System.Threading;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Final;
using Assets.Networking.ClientListeners;
using Assets.Networking.Services;
using Assets.Utils;
using Grpc.Core;
using UnityEngine;

namespace Assets.Networking
{
    class ClientCentralBuildingOrders : ICentralBuildingOrders
    {
        private readonly CentralBuildingService.CentralBuildingServiceClient mClient;
        private readonly string mID;

        public ClientCentralBuildingOrders(CentralBuildingService.CentralBuildingServiceClient client, string id)
        {
            mClient = client;
            mID = id;
        }

        public async Task<bool> QueueWorker()
        {
            var result = await mClient.QueueWorkerAsync(new QueueWorkerRequest { Base = new QueueUnitRequest { BuildingID = new ID { Value = mID } } });
            return result.Base.Result;
        }

        public Task SetWaypoint(Vector2 waypoint)
        {
            return mClient.SetWaypointAsync(new SetWaypointRequest { BuildingID = new ID { Value = mID }, Waypoint = waypoint.ToGrpc() }).ResponseAsync;
        }

        public Task CancelOrderAt(int index)
        {
            return mClient.CancelOredrAsync(new CancelQueuedRequest
            {
                ObjectID = new ID { Value = mID },
                Index = index,
            }).ResponseAsync;
        }

        public Task CancelBuilding()
        {
            return mClient.CancelBuildingAsync(new CancelBuildingRequest()
            {
                BuildingID = new ID() { Value = mID }
            }).ResponseAsync;
        }
    }

    class ClientCentralBuildingState : ICentralBuildingInfo, IStateHolder<CentralBuildingState>
    {
        public CentralBuildingState State { get; private set; } = new CentralBuildingState();

        public Guid ID => Guid.Parse(State.Base.Base.Base.ID.Value);
        public Guid PlayerID => Guid.Parse(State.Base.Base.Base.PlayerID.Value);
        public Vector2 Position => State.Base.Base.Base.Position.ToUnity();
        public float RecivedDamage => State.Base.Base.Base.RecivedDamage;
        public float MaxHealth => State.Base.Base.Base.MaxHealth;
        public Vector2 Size => State.Base.Base.Size.ToUnity();
        public Vector2 Waypoint => State.Base.Waypoint.ToUnity();
        public int Queued => State.Base.Queued;
        public float Progress => State.Base.Progress;
        public float ViewRadius => State.Base.Base.Base.ViewRadius;
        public int Armour => State.Base.Base.Base.Armour;
        public BuildingProgress BuildingProgress => State.Base.Base.Progress;

        public void ResetState()
        {
            State = new CentralBuildingState();
        }
    }

    class CentralBuildingCreationListener : CommonCreationStateListener<
        ICentralBuildingOrders,
        ICentralBuildingInfo,
        ClientCentralBuildingOrders,
        ClientCentralBuildingState,
        CentralBuildingState,
        CentralBuildingService.CentralBuildingServiceClient
        >
    {
        public CentralBuildingCreationListener(UnitySyncContext syncContext)
            : base(syncContext) {}

        protected override AsyncServerStreamingCall<CentralBuildingState> GetCreationCall(CentralBuildingService.CentralBuildingServiceClient client, CancellationToken token)
        {
            return client.ListenCreation(new Empty(), cancellationToken: token);
        }

        protected override AsyncServerStreamingCall<CentralBuildingState> GetUpdatesCall(CentralBuildingService.CentralBuildingServiceClient client, ID id, CancellationToken token)
        {
            return client.ListenState(id, cancellationToken: token);
        }

        protected override CentralBuildingService.CentralBuildingServiceClient CreateClient(Channel channel)
        {
            return new CentralBuildingService.CentralBuildingServiceClient(channel);
        }

        protected override ClientCentralBuildingOrders CreateOrders(CentralBuildingService.CentralBuildingServiceClient client, Guid id)
        {
            return new ClientCentralBuildingOrders(client, id.ToString());
        }
    }
}