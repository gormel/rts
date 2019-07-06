using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Final;
using Assets.Utils;
using Grpc.Core;
using UnityEngine;

namespace Assets.Networking.ClientListeners
{
    class ClientBarrakInfo : IBarrakInfo, IStateHolder<BarrakState> {
        public BarrakState State { get; } = new BarrakState();

        public Guid ID => Guid.Parse(State.Base.Base.ID.Value);
        public Guid PlayerID => Guid.Parse(State.Base.Base.PlayerID.Value);
        public Vector2 Position => State.Base.Base.Position.ToUnity();
        public float Health => State.Base.Base.Health;
        public float MaxHealth => State.Base.Base.MaxHealth;
        public Vector2 Size => State.Base.Size.ToUnity();
        public Vector2 Waypoint => State.Base.Waypoint.ToUnity();
    }

    class ClientBarrakOrders : IBarrakOrders
    {
        private readonly BarrakService.BarrakServiceClient mClient;
        private readonly string mID;

        public ClientBarrakOrders(BarrakService.BarrakServiceClient client, string id)
        {
            mClient = client;
            mID = id;
        }

        public Task SetWaypoint(Vector2 waypoint)
        {
            return mClient.SetWaypointAsync(new SetWaypointRequest
            {
                BuildingID = new ID { Value = mID },
                Waypoint = waypoint.ToGrpc()
            }).ResponseAsync;
        }
    }

    class BarrakCreationListener : CommonCreationStateListener<
        IBarrakOrders,
        IBarrakInfo,
        ClientBarrakOrders,
        ClientBarrakInfo,
        BarrakState,
        BarrakService.BarrakServiceClient
        >
    {
        public BarrakCreationListener(UnitySyncContext syncContext) 
            : base(syncContext)
        {
        }

        protected override IAsyncStreamReader<BarrakState> GetCreationStream(BarrakService.BarrakServiceClient client)
        {
            return client.ListenCreation(new Empty()).ResponseStream;
        }

        protected override IAsyncStreamReader<BarrakState> GetUpdatesStream(BarrakService.BarrakServiceClient client, ID id)
        {
            return client.ListenState(id).ResponseStream;
        }

        protected override BarrakService.BarrakServiceClient CreateClient(Channel channel)
        {
            return new BarrakService.BarrakServiceClient(channel);
        }

        protected override ClientBarrakOrders CreateOrders(BarrakService.BarrakServiceClient client, Guid id)
        {
            return new ClientBarrakOrders(client, id.ToString());
        }
    }
}
