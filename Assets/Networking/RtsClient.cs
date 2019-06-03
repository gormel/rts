using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Core.Game;
using Assets.Core.GameObjects.Final;
using Assets.Core.Map;
using Assets.Utils;
using Google.Protobuf;
using Grpc.Core;
using UnityEngine;

namespace Assets.Networking
{
    interface IStateHolder<TState> where TState : IMessage<TState>
    {
        TState State { get; }
    }

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
        
        public event Action<IPlayerState> PlayerConnected;
        public event Action<IMapData> MapLoaded;

        public event Action<IWorkerOrders, IWorkerInfo> WorkerCreated;
        public event Action<IBuildingTemplateOrders, IBuildingTemplateInfo> BuildingTemplateCreated;

        private bool mMapLoaded;
        private readonly UnitySyncContext mSyncContext;
        private Channel mChannel;

        private readonly WorkerCreationStateListener mWorkerCreationStateListener;
        private readonly BuildingTemplateCreationStateListener mBuildingTemplateCreationStateListener;

        public RtsClient(UnitySyncContext syncContext)
        {
            mSyncContext = syncContext;
            mWorkerCreationStateListener = new WorkerCreationStateListener(syncContext);
            mWorkerCreationStateListener.Created += WorkerCreationStateListenerOnCreated;

            mBuildingTemplateCreationStateListener = new BuildingTemplateCreationStateListener(syncContext);
            mBuildingTemplateCreationStateListener.Created += BuildingTemplateCreationStateListenerOnCreated;
        }

        private void BuildingTemplateCreationStateListenerOnCreated(IBuildingTemplateOrders arg1, IBuildingTemplateInfo arg2)
        {
            BuildingTemplateCreated?.Invoke(arg1, arg2);
        }

        private void WorkerCreationStateListenerOnCreated(IWorkerOrders arg1, IWorkerInfo arg2)
        {
            WorkerCreated?.Invoke(arg1, arg2);
        }

        public Task Listen()
        {
            mChannel = new Channel(GameUtils.IP.ToString(), GameUtils.Port, ChannelCredentials.Insecure);
            return Task.WhenAll(
                ListenGameState(mChannel)
            );
        }

        public Task Shutdown()
        {
            return mChannel.ShutdownAsync();
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
                    mapState.State.MergeFrom(state.Map);
                    playerState.PlayerState.MergeFrom(state.Player);
                    channel.ShutdownToken.ThrowIfCancellationRequested();

                    if (!mMapLoaded)
                    {
                        mMapLoaded = true;
                        await mSyncContext.Execute(() => MapLoaded?.Invoke(mapState), channel.ShutdownToken);
                        var t0 = mWorkerCreationStateListener.ListenCreations(mChannel);
                        var t1 = mBuildingTemplateCreationStateListener.ListenCreations(mChannel);
                    }
                }
            }
        }
    }
}
