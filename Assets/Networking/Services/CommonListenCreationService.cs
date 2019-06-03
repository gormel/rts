using System;
using System.IO;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using Assets.Utils;
using Google.Protobuf;
using Grpc.Core;
using UnityEngine;

namespace Assets.Networking.Services
{
    class CommonListenCreationService<TOrders, TInfo, TState> : IRegistrator<TOrders, TInfo> 
        where TOrders : IGameObjectOrders 
        where TInfo : IGameObjectInfo
        where TState : IMessage<TState>
    {
        private struct Registration
        {
            public TInfo Info { get; }
            public TOrders Orders { get; }

            public Registration(TInfo info, TOrders orders)
            {
                Info = info;
                Orders = orders;
            }
        }

        private readonly AsyncQueue<Registration> mRegistrations = new AsyncQueue<Registration>();
        private readonly AsyncDictionary<Guid, Registration> mRegistred = new AsyncDictionary<Guid, Registration>();
        private readonly Func<TInfo, TState> mCreateState;

        public CommonListenCreationService(Func<TInfo, TState> createState)
        {
            mCreateState = createState;
        }

        public async Task ListenCreation(IServerStreamWriter<TState> responseStream, ServerCallContext context)
        {
            foreach (var key in mRegistred.Keys)
            {
                var reg = await mRegistred.GetValueAsync(key, context.CancellationToken);
                await responseStream.WriteAsync(mCreateState(reg.Info));
            }

            while (true)
            {
                var registration = await mRegistrations.DequeueAsync(context.CancellationToken);
                mRegistred.AddOrUpdate(registration.Info.ID, registration);
                await responseStream.WriteAsync(mCreateState(registration.Info));
            }
        }

        public async Task ListenState(ID request, IServerStreamWriter<TState> responseStream, ServerCallContext context)
        {
            var workerId = Guid.Parse(request.Value);
            var listening = await mRegistred.GetValueAsync(workerId, context.CancellationToken);

            while (true)
            {
                await responseStream.WriteAsync(mCreateState(listening.Info));
                await Task.Delay(30);

                if (!mRegistred.TryGetValue(workerId, out listening))
                    throw new EndOfStreamException();
            }
        }

        public async Task<T> ExecuteOrder<T>(ID objId, Func<TOrders, Task<T>> order)
        {
            Registration reg;
            if (!mRegistred.TryGetValue(Guid.Parse(objId.Value), out reg))
                return default;

            return await order(reg.Orders);
        }

        public void Register(TOrders orders, TInfo info)
        {
            mRegistrations.Enqueue(new Registration(info, orders));
        }

        public void Unregister(Guid id)
        {
            mRegistred.Remove(id);
        }
    }
}