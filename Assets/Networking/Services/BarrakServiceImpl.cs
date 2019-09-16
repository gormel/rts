using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Final;
using Assets.Utils;
using Grpc.Core;

namespace Assets.Networking.Services
{
    class BarrakServiceImpl : BarrakService.BarrakServiceBase, IRegistrator<IBarrakOrders, IBarrakInfo>
    {
        private CommonListenCreationService<IBarrakOrders, IBarrakInfo, BarrakState> mCommonService;

        public BarrakServiceImpl()
        {
            mCommonService = new CommonListenCreationService<IBarrakOrders, IBarrakInfo, BarrakState>(CreateState);
        }

        private BarrakState CreateState(IBarrakInfo info)
        {
            return new BarrakState
            {
                Base = new FactoryBuildingState
                {
                    Base = new BuildingState
                    {
                        Base = new ObjectState
                        {
                            ID = new ID { Value = info.ID.ToString() },
                            PlayerID = new ID { Value = info.PlayerID.ToString() },
                            Health = info.Health,
                            MaxHealth = info.MaxHealth,
                            Position = info.Position.ToGrpc()
                        },
                        Size = info.Size.ToGrpc()
                    },
                    Waypoint = info.Waypoint.ToGrpc(),
                    Progress = info.Progress,
                    Queued = info.Queued
                }
            };
        }

        public override Task ListenCreation(Empty request, IServerStreamWriter<BarrakState> responseStream, ServerCallContext context)
        {
            return mCommonService.ListenCreation(responseStream, context);
        }

        public override Task ListenState(ID request, IServerStreamWriter<BarrakState> responseStream, ServerCallContext context)
        {
            return mCommonService.ListenState(request, responseStream, context);
        }

        public override Task<Empty> SetWaypoint(SetWaypointRequest request, ServerCallContext context)
        {
            return mCommonService.ExecuteOrder(request.BuildingID, async orders =>
            {
                await orders.SetWaypoint(request.Waypoint.ToUnity());
                return new Empty();
            });
        }

        public override Task<QueueRangedResult> QueueRanged(QueueRangedRequest request, ServerCallContext context)
        {
            return mCommonService.ExecuteOrder(request.Base.BuildingID, async orders =>
            {
                var result = await orders.QueueRanged();
                return new QueueRangedResult { Base = new QueueUnitResult { Result = result } };
            });
        }

        public void Register(IBarrakOrders orders, IBarrakInfo info)
        {
            mCommonService.Register(orders, info);
        }

        public void Unregister(Guid id)
        {
            mCommonService.Unregister(id);
        }
    }
}
