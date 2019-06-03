using System;
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
            var result = await mClient.QueueWorkerAsync(new QueueWorkerRequest { CentralBuildingID = new ID { Value = mID } });
            return result.Result;
        }

        public Task SetWaypoint(Vector2 waypoint)
        {
            return mClient.SetWaypointAsync(new SetWaypointRequest { CentralBuildingID = new ID { Value = mID }, Waypoint = waypoint.ToGrpc() }).ResponseAsync;
        }
    }

    class ClientCentralBuildingState : ICentralBuildingInfo, IStateHolder<CentralBuildingState>
    {
        public CentralBuildingState State { get; } = new CentralBuildingState();

        public Guid ID => Guid.Parse(State.Base.Base.ID.Value);
        public Guid PlayerID => Guid.Parse(State.Base.Base.PlayerID.Value);
        public Vector2 Position => State.Base.Base.Position.ToUnity();
        public float Health => State.Base.Base.Health;
        public float MaxHealth => State.Base.Base.MaxHealth;
        public Vector2 Size => State.Base.Size.ToUnity();
        public Vector2 Waypoint => State.Base.Waypoint.ToUnity();
        public float Progress => State.Progress;
        public int WorkersQueued => State.WorkersQueued;
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

        protected override IAsyncStreamReader<CentralBuildingState> GetCreationStream(CentralBuildingService.CentralBuildingServiceClient client)
        {
            return client.ListenCreation(new Empty()).ResponseStream;
        }

        protected override IAsyncStreamReader<CentralBuildingState> GetUpdatesStream(CentralBuildingService.CentralBuildingServiceClient client, ID id)
        {
            return client.ListenState(id).ResponseStream;
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