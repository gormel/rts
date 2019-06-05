using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Core.Game;
using Assets.Core.GameObjects.Base;
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

            public MapObject GetMapObjectAt(int x, int y)
            {
                return (MapObject)State.Objects[y * Width + x];
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
        public event Action<Vector2> BaseCreated;

        public event Action<IWorkerOrders, IWorkerInfo> WorkerCreated;
        public event Action<IBuildingTemplateOrders, IBuildingTemplateInfo> BuildingTemplateCreated;
        public event Action<ICentralBuildingOrders, ICentralBuildingInfo> CentralBuildingCreated;
        public event Action<IMinigCampOrders, IMinigCampInfo> MiningCampCreated;


        public event Action<IGameObjectInfo> ObjectDestroyed;

        private bool mMapLoaded;
        private readonly UnitySyncContext mSyncContext;
        private Channel mChannel;

        private readonly WorkerCreationStateListener mWorkerCreationStateListener;
        private readonly BuildingTemplateCreationStateListener mBuildingTemplateCreationStateListener;
        private readonly CentralBuildingCreationListener mCentralBuildingCreationStateListener;
        private readonly MiningCampCreationListener mMiningCampCreationListener;

        public RtsClient(UnitySyncContext syncContext)
        {
            mSyncContext = syncContext;
            mWorkerCreationStateListener = new WorkerCreationStateListener(syncContext);
            mWorkerCreationStateListener.Created += (orders, info) => WorkerCreated?.Invoke(orders, info);
            mWorkerCreationStateListener.Destroyed += info => ObjectDestroyed?.Invoke(info);

            mBuildingTemplateCreationStateListener = new BuildingTemplateCreationStateListener(syncContext);
            mBuildingTemplateCreationStateListener.Created += (orders, info) => BuildingTemplateCreated?.Invoke(orders, info);
            mBuildingTemplateCreationStateListener.Destroyed += info => ObjectDestroyed?.Invoke(info);

            mCentralBuildingCreationStateListener = new CentralBuildingCreationListener(syncContext);
            mCentralBuildingCreationStateListener.Created += (orders, info) => CentralBuildingCreated?.Invoke(orders, info);
            mCentralBuildingCreationStateListener.Destroyed += info => ObjectDestroyed?.Invoke(info);

            mMiningCampCreationListener = new MiningCampCreationListener(syncContext);
            mMiningCampCreationListener.Created += (orders, info) => MiningCampCreated?.Invoke(orders, info);
            mMiningCampCreationListener.Destroyed += info => ObjectDestroyed?.Invoke(info);
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
                        await mSyncContext.Execute(() => 
                        {
                            MapLoaded?.Invoke(mapState);
                            BaseCreated?.Invoke(state.BasePos.ToUnity());
                        }, channel.ShutdownToken);
                        var t0 = mWorkerCreationStateListener.ListenCreations(mChannel);
                        var t1 = mBuildingTemplateCreationStateListener.ListenCreations(mChannel);
                        var t2 = mCentralBuildingCreationStateListener.ListenCreations(mChannel);
                        var t3 = mMiningCampCreationListener.ListenCreations(mChannel);
                    }
                }
            }
        }
    }
}
