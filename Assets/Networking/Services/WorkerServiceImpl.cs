using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Final;
using Assets.Utils;
using Grpc.Core;

namespace Assets.Networking.Services
{
    class WorkerServiceImpl : WorkerService.WorkerServiceBase, IRegistrator<IWorkerOrders, IWorkerInfo>
    {
        private struct Registration
        {
            public IWorkerInfo Info { get; }
            public IWorkerOrders Orders { get; }

            public Registration(IWorkerInfo info, IWorkerOrders orders)
            {
                Info = info;
                Orders = orders;
            }
        }

        private readonly UnitySyncContext mSyncContext;
        private readonly AsyncQueue<Registration> mRegistrations = new AsyncQueue<Registration>(); 
        private readonly AsyncDictionary<Guid, Registration> mRegistred = new AsyncDictionary<Guid, Registration>();

        public WorkerServiceImpl(UnitySyncContext syncContext)
        {
            mSyncContext = syncContext;
        }

        private Task<WorkerState> CreateState(IWorkerInfo info, CancellationToken token = default(CancellationToken))
        {
            return mSyncContext.Execute(() =>
                new WorkerState
                {
                    Base = new UnitState
                    {
                        Base = new ObjectState
                        {
                            ID = new ID { Value = info.ID.ToString() },
                            Health = info.Health,
                            MaxHealth = info.MaxHealth,
                            Position = info.Position.ToGrpc()
                        },
                        Destignation = info.Destignation.ToGrpc(),
                        Direction = info.Direction.ToGrpc(),
                        Speed = info.Speed
                    }
                }, token);
        }

        public override async Task ListenCreation(Empty request, IServerStreamWriter<WorkerState> responseStream, ServerCallContext context)
        {
            foreach (var key in mRegistred.Keys)
            {
                var reg = await mRegistred.GetValueAsync(key, context.CancellationToken);
                await responseStream.WriteAsync(await CreateState(reg.Info, context.CancellationToken));
            }

            while (true)
            {
                var registration = await mRegistrations.DequeueAsync(context.CancellationToken);
                mRegistred.AddOrUpdate(registration.Info.ID, registration);
                await responseStream.WriteAsync(await CreateState(registration.Info, context.CancellationToken));
            }
        }

        public override async Task ListenState(ID request, IServerStreamWriter<WorkerState> responseStream, ServerCallContext context)
        {
            var workerId = Guid.Parse(request.Value);
            var listening = (await mRegistred.GetValueAsync(workerId, context.CancellationToken)).Info;

            while (true)
            {
                await responseStream.WriteAsync(await CreateState(listening, context.CancellationToken));
                await Task.Delay(16);
            }
        }

        public void Register(IWorkerOrders orders, IWorkerInfo info)
        {
            mRegistrations.Enqueue(new Registration(info, orders));
        }
    }
}
