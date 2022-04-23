using System;
using System.Collections.Generic;
using System.Threading;
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

        protected abstract AsyncServerStreamingCall<TState> GetCreationCall(TClient client, CancellationToken token);
        protected abstract AsyncServerStreamingCall<TState> GetUpdatesCall(TClient client, ID id, CancellationToken token);
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
                using (var call = GetCreationCall(client, channel.ShutdownToken))
                using (var creationsStream = call.ResponseStream)
                {
                    while (await creationsStream.MoveNext(channel.ShutdownToken))
                    {
                        channel.ShutdownToken.ThrowIfCancellationRequested();

                        var createdState = creationsStream.Current;
                        var info = new TInfo();
                        info.State.MergeFrom(createdState);

                        var orders = CreateOrders(client, info.ID);
                        
                        await mSyncContext.Execute(() => Created?.Invoke(orders, info));
                        var t = ListenStateUpdates(info, client, channel.ShutdownToken);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        private async Task ListenStateUpdates(TInfo state, TClient client, CancellationToken token)
        {
            try
            {
                using (var call = GetUpdatesCall(client, new ID { Value = state.ID.ToString() }, token))
                using (var updateStream = call.ResponseStream)
                {
                    while (await updateStream.MoveNext(token))
                    {
                        token.ThrowIfCancellationRequested();
                        state.ResetState();
                        state.State.MergeFrom(updateStream.Current);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                if (mSyncContext.isActiveAndEnabled)
                    await mSyncContext.Execute(() => Destroyed?.Invoke(state), token);

                throw;
            }
        }
    }
}