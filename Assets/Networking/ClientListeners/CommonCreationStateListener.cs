using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using Google.Protobuf;
using Grpc.Core;
using UnityEngine;

namespace Assets.Networking.ClientListeners
{
    abstract class CommonCreationStateListener<TOrdersBase, TInfoBase, TOrders, TInfo, TState, TClient>
        where TOrdersBase : IGameObjectOrders
        where TInfoBase : IGameObjectInfo
        where TOrders : TOrdersBase
        where TInfo : TInfoBase, IStateHolder<TState>, new()
        where TState : IMessage<TState>, new()
    {
        private readonly UnitySyncContext mSyncContext;
        public event Action<TOrdersBase, TInfoBase> Created;
        public event Action<TInfoBase> Destroyed;

        protected abstract IAsyncStreamReader<TState> GetCreationStream(TClient client);
        protected abstract IAsyncStreamReader<TState> GetUpdatesStream(TClient client, ID id);
        protected abstract TClient CreateClient(Channel channel);
        protected abstract TOrders CreateOrders(TClient client, Guid id);

        public CommonCreationStateListener(UnitySyncContext syncContext)
        {
            mSyncContext = syncContext;
        }

        public async Task ListenCreations(Channel channel)
        {
            var client = CreateClient(channel);

            try
            {
                using (var creationsStream = GetCreationStream(client))
                {
                    while (await creationsStream.MoveNext())
                    {
                        var createdState = creationsStream.Current;
                        var info = new TInfo();
                        info.State.MergeFrom(createdState);

                        var orders = CreateOrders(client, info.ID);

                        await mSyncContext.Execute(() => Created?.Invoke(orders, info));
                        var t = ListenStateUpdates(info, client);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private async Task ListenStateUpdates(TInfo state, TClient client)
        {
            try
            {
                using (var updateStream = GetUpdatesStream(client, new ID { Value = state.ID.ToString() }))
                {
                    while (await updateStream.MoveNext())
                    {
                        state.State.MergeFrom(updateStream.Current);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                await mSyncContext.Execute(() => Destroyed?.Invoke(state));
            }
        }
    }
}