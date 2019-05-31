using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Game;
using Assets.Core.GameObjects.Final;
using Assets.Core.Map;
using Assets.Utils;
using Grpc.Core;
using UnityEngine;

namespace Assets.Networking
{
    class RtsClient
    {
        private class ClientMapData : IMapData
        {
            public MapState State { get; } = new MapState();

            public int Length => State.Lenght;
            public int Width => State.Width;

            public float GetHeightAt(int x, int y)
            {
                return State.Heights[y * Width + x];
            }
        }

        private class ClientPlayerState : IPlayerState
        {
            public PlayerState PlayerState { get; } = new PlayerState();

            public Guid ID => Guid.Parse(PlayerState.ID.Value);
            public int Money => PlayerState.Money;
        }

        private class ClientWorkerState : IWorkerInfo
        {
            public WorkerState State { get; } = new WorkerState();

            public Guid ID => Guid.Parse(State.Base.Base.ID.Value);
            public Vector2 Position => State.Base.Base.Position.ToUnity();
            public float Health => State.Base.Base.Health;
            public float MaxHealth => State.Base.Base.MaxHealth;
            public float Speed => State.Base.Speed;
            public Vector2 Direction => State.Base.Direction.ToUnity();
            public Vector2 Destignation => State.Base.Destignation.ToUnity();
        }

        private class ClientWorkerOrders : IWorkerOrders
        {
            private readonly WorkerService.WorkerServiceClient mClient;
            private readonly string mID;

            public ClientWorkerOrders(WorkerService.WorkerServiceClient client, Guid id)
            {
                mClient = client;
                mID = id.ToString();
            }

            public Task GoTo(Vector2 position)
            {
                return mClient.GoToAsync(new GoToRequest
                {
                    Destignation = position.ToGrpc(),
                    WorkerID = new ID { Value = mID }
                }).ResponseAsync;
            }

            public async Task<Guid> PlaceCentralBuildingTemplate(Vector2Int position)
            {
                var resp = await mClient.PlaceCentralBuildingTemplateAsync(new PlaceCentralBuildingTemplateRequest
                {
                    Position = new Vector { X = position.x, Y = position.y },
                    WorkerID = new ID { Value = mID }
                });
                return Guid.Parse(resp.Value);
            }

            public Task AttachAsBuilder(Guid templateId)
            {
                return mClient.AttachAsBuilderAsync(new AttachAsBuilderRequest
                {
                    BuildingTemplateID = new ID { Value = templateId.ToString() },
                    WorkerID = new ID { Value = mID }
                }).ResponseAsync;
            }
        }

        public event Action<IPlayerState> PlayerConnected;
        public event Action<IMapData> MapLoaded;

        public event Action<IWorkerOrders, IWorkerInfo> WorkerCreated;

        private bool mMapLoaded;
        private readonly UnitySyncContext mSyncContext;
        private Channel mChannel;

        public RtsClient(UnitySyncContext syncContext)
        {
            mSyncContext = syncContext;
        }

        public Task Listen()
        {
            mChannel = new Channel(GameUtils.IP.ToString(), GameUtils.Port, ChannelCredentials.Insecure);
            return Task.WhenAll(
                ListenGameState(mChannel),
                ListenWorkerCreations(mChannel)
            );
        }

        public Task Shutdown()
        {
            return mChannel.ShutdownAsync();
        }

        private async Task ListenWorkerCreations(Channel channel)
        {
            var client = new WorkerService.WorkerServiceClient(channel);

            using (var creationsStream = client.ListenCreation(new Empty()).ResponseStream)
            {
                while (await creationsStream.MoveNext())
                {
                    var createdState = creationsStream.Current;
                    var info = new ClientWorkerState();
                    info.State.MergeFrom(createdState);
                    
                    var orders = new ClientWorkerOrders(client, info.ID);

                    await mSyncContext.Execute(() => WorkerCreated?.Invoke(orders, info));
                    var t = ListenStateUpdates(info, client);
                }
            }
        }

        private async Task ListenStateUpdates(ClientWorkerState state, WorkerService.WorkerServiceClient client)
        {
            using (var updateStream = client.ListenState(new ID { Value = state.ID.ToString() }).ResponseStream)
            {
                while (await updateStream.MoveNext())
                {
                    var recivedState = updateStream.Current;
                    await mSyncContext.Execute(() => state.State.MergeFrom(recivedState));
                }
            }
        }

        private async Task ListenGameState(Channel channel)
        {
            var mapState = new ClientMapData();
            var playerState = new ClientPlayerState();
            await mSyncContext.Execute(() => PlayerConnected?.Invoke(playerState), channel.ShutdownToken);

            var client = new GameService.GameServiceClient(channel);
            using (var stateStream = client.ConnectAndListenState(new Empty()).ResponseStream)
            {
                while (await stateStream.MoveNext())
                {
                    channel.ShutdownToken.ThrowIfCancellationRequested();
                    var state = stateStream.Current;
                    await mSyncContext.Execute(() => mapState.State.MergeFrom(state.Map), channel.ShutdownToken);
                    playerState.PlayerState.MergeFrom(state.Player);
                    channel.ShutdownToken.ThrowIfCancellationRequested();

                    if (!mMapLoaded)
                    {
                        mMapLoaded = true;
                        await mSyncContext.Execute(() => MapLoaded?.Invoke(mapState), channel.ShutdownToken);
                    }
                }
            }
        }
    }
}
