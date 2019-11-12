using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Core.Game;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using Assets.Core.Map;
using Assets.Networking.ClientListeners;
using Assets.Utils;
using Google.Protobuf;
using Grpc.Core;
using UnityEngine;

namespace Assets.Networking
{
    interface IStateHolder<TState> where TState : IMessage<TState>
    {
        TState State { get; }
        void ResetState();
    }

    class RtsClient
    {
        private class ClientMapData : IMapData
        {
            public MapState State { get; set; } = new MapState();

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
            public PlayerState PlayerState { get; set; } = new PlayerState();

            public Guid ID => Guid.Parse(PlayerState.ID.Value);
            public int Money => PlayerState.Money;
        }
        
        public event Action<IPlayerState> PlayerConnected;
        public event Action<IMapData> MapLoaded;
        public event Action<Vector2> BaseCreated;
        public event Action DisconnectedFromServer;

        public event Action<IRangedWarriorOrders, IRangedWarriorInfo> RangedWarriorCreated;
        public event Action<IMeeleeWarriorOrders, IMeeleeWarriorInfo> MeeleeWarriorCreated;
        public event Action<IWorkerOrders, IWorkerInfo> WorkerCreated;
        public event Action<IBuildingTemplateOrders, IBuildingTemplateInfo> BuildingTemplateCreated;
        public event Action<ICentralBuildingOrders, ICentralBuildingInfo> CentralBuildingCreated;
        public event Action<IMinigCampOrders, IMinigCampInfo> MiningCampCreated;
        public event Action<IBarrakOrders, IBarrakInfo> BarrakCreated;

        public event Action<IGameObjectInfo> ObjectDestroyed;

        public event Action<string, int> ChatMessageRecived;

        private bool mMapLoaded;
        private readonly UnitySyncContext mSyncContext;
        private Channel mChannel;

        private readonly MeeleeWarriorCreationStateListener mMeeleeWarriorCreationStateListener;
        private readonly RangedWarriorCreationStateListener mRangedWarriorCreationStateListener;
        private readonly WorkerCreationStateListener mWorkerCreationStateListener;
        private readonly BuildingTemplateCreationStateListener mBuildingTemplateCreationStateListener;
        private readonly CentralBuildingCreationListener mCentralBuildingCreationStateListener;
        private readonly MiningCampCreationListener mMiningCampCreationListener;
        private readonly BarrakCreationListener mBarrakCreationListener;
        private GameService.GameServiceClient mGameService;

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

            mBarrakCreationListener = new BarrakCreationListener(syncContext);
            mBarrakCreationListener.Created += (orders, info) => BarrakCreated?.Invoke(orders, info);
            mBarrakCreationListener.Destroyed += info => ObjectDestroyed?.Invoke(info);

            mRangedWarriorCreationStateListener = new RangedWarriorCreationStateListener(syncContext);
            mRangedWarriorCreationStateListener.Created += (orders, info) => RangedWarriorCreated?.Invoke(orders, info);
            mRangedWarriorCreationStateListener.Destroyed += info => ObjectDestroyed?.Invoke(info);
            
            mMeeleeWarriorCreationStateListener = new MeeleeWarriorCreationStateListener(syncContext);
            mMeeleeWarriorCreationStateListener.Created += (orders, info) => MeeleeWarriorCreated?.Invoke(orders, info);
            mMeeleeWarriorCreationStateListener.Destroyed += info => ObjectDestroyed?.Invoke(info);
        }

        public Task Listen()
        {
            mChannel = new Channel(GameUtils.IP.ToString(), GameUtils.GamePort, ChannelCredentials.Insecure);
            return Task.WhenAll(
                ListenGameState(mChannel)
            );
        }

        public Task Shutdown()
        {
            return mChannel.ShutdownAsync();
        }

        public void SendChatMessage(string nickname, int stickerID)
        {
            mGameService?.SendChatMessageAsync(new ChatMessage { Nickname = nickname, StickerID = stickerID });
        }

        private async Task ListenGameState(Channel channel)
        {
            var mapState = new ClientMapData();
            var playerState = new ClientPlayerState();
            await mSyncContext.Execute(() => PlayerConnected?.Invoke(playerState), channel.ShutdownToken);

            try
            {
                while (true)
                {
                    try
                    {
                        mGameService = new GameService.GameServiceClient(channel);
                        using (var call = mGameService.ConnectAndListenState(new Empty()))
                        using (var stateStream = call.ResponseStream)
                        {
                            while (await stateStream.MoveNext(channel.ShutdownToken))
                            {
                                channel.ShutdownToken.ThrowIfCancellationRequested();
                                var state = stateStream.Current;

                                if (state.Map != null)
                                    mapState.State = new MapState(state.Map);

                                if (state.Player != null)
                                    playerState.PlayerState = new PlayerState(state.Player);

                                channel.ShutdownToken.ThrowIfCancellationRequested();

                                if (!mMapLoaded)
                                {
                                    mMapLoaded = true;
                                    await mSyncContext.Execute(() =>
                                    {
                                        MapLoaded?.Invoke(mapState);
                                        BaseCreated?.Invoke(state.BasePos.ToUnity());
                                    }, channel.ShutdownToken);

                                    var tChat = ListenChat(mGameService, channel);

                                    var t0 = mWorkerCreationStateListener.ListenCreations(mChannel);
                                    var t1 = mBuildingTemplateCreationStateListener.ListenCreations(mChannel);
                                    var t2 = mCentralBuildingCreationStateListener.ListenCreations(mChannel);
                                    var t3 = mMiningCampCreationListener.ListenCreations(mChannel);
                                    var t4 = mBarrakCreationListener.ListenCreations(mChannel);
                                    var t5 = mRangedWarriorCreationStateListener.ListenCreations(mChannel);
                                    var t6 = mMeeleeWarriorCreationStateListener.ListenCreations(mChannel);
                                }
                            }
                        }

                        break;
                    }
                    catch (RpcException e)
                    {
                        if (e.Status.StatusCode != StatusCode.Unavailable)
                            throw;

                        await Task.Delay(TimeSpan.FromSeconds(0.5));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                DisconnectedFromServer?.Invoke();
                throw;
            }
        }

        private async Task ListenChat(GameService.GameServiceClient client, Channel channel)
        {
            try
            {
                using (var call = client.ListenChat(new Empty()))
                using (var chatStream = call.ResponseStream)
                {
                    while (await chatStream.MoveNext(channel.ShutdownToken))
                    {
                        ChatMessageRecived?.Invoke(chatStream.Current.Nickname, chatStream.Current.StickerID);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }
    }
}
