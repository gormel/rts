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
        public BarrakState State { get; private set; } = new BarrakState();
        public void ResetState()
        {
            State = new BarrakState();
        }

        public Guid ID => Guid.Parse(State.Base.Base.Base.ID.Value);
        public Guid PlayerID => Guid.Parse(State.Base.Base.Base.PlayerID.Value);
        public Vector2 Position => State.Base.Base.Base.Position.ToUnity();
        public float Health => State.Base.Base.Base.Health;
        public float MaxHealth => State.Base.Base.Base.MaxHealth;
        public Vector2 Size => State.Base.Base.Size.ToUnity();
        public Vector2 Waypoint => State.Base.Waypoint.ToUnity();
        public int Queued => State.Base.Queued;
        public float Progress => State.Base.Progress;
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

        public async Task<bool> QueueRanged()
        {
            var resp = await mClient.QueueRangedAsync(new QueueRangedRequest
            {
                Base = new QueueUnitRequest
                {
                    BuildingID = new ID { Value = mID }
                }
            }).ResponseAsync;
            return resp.Base.Result;
        }

        public async Task<bool> QueueMeelee()
        {
            var resp = await mClient.QueueMeeleeAsync(new QueueMeeleeRequest
            {
                Base = new QueueUnitRequest
                {
                    BuildingID = new ID { Value = mID }
                }
            }).ResponseAsync;
            return resp.Base.Result;
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

        protected override AsyncServerStreamingCall<BarrakState> GetCreationCall(BarrakService.BarrakServiceClient client)
        {
            return client.ListenCreation(new Empty());
        }

        protected override AsyncServerStreamingCall<BarrakState> GetUpdatesCall(BarrakService.BarrakServiceClient client, ID id)
        {
            return client.ListenState(id);
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
