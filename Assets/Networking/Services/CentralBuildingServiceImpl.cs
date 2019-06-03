using System;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Final;
using Assets.Utils;
using Grpc.Core;

namespace Assets.Networking.Services
{
    class CentralBuildingServiceImpl : CentralBuildingService.CentralBuildingServiceBase, IRegistrator<ICentralBuildingOrders, ICentralBuildingInfo>
    {
        private CommonListenCreationService<ICentralBuildingOrders, ICentralBuildingInfo, CentralBuildingState> mCommonService;

        public CentralBuildingServiceImpl()
        {
            mCommonService = new CommonListenCreationService<ICentralBuildingOrders, ICentralBuildingInfo, CentralBuildingState>(CreateState);
        }

        private CentralBuildingState CreateState(ICentralBuildingInfo info)
        {
            return new CentralBuildingState
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
                    Size = info.Size.ToGrpc(),
                    Waypoint = info.Waypoint.ToGrpc()
                },
                Progress = info.Progress,
                WorkersQueued = info.WorkersQueued
            };
        }

        public override Task ListenCreation(Empty request, IServerStreamWriter<CentralBuildingState> responseStream, ServerCallContext context)
        {
            return mCommonService.ListenCreation(responseStream, context);
        }

        public override Task ListenState(ID request, IServerStreamWriter<CentralBuildingState> responseStream, ServerCallContext context)
        {
            return mCommonService.ListenState(request, responseStream, context);
        }

        public override Task<QueueWorkerResult> QueueWorker(QueueWorkerRequest request, ServerCallContext context)
        {
            return mCommonService.ExecuteOrder(request.CentralBuildingID, async orders => new QueueWorkerResult { Result = await orders.QueueWorker() });
        }

        public override Task<Empty> SetWaypoint(SetWaypointRequest request, ServerCallContext context)
        {
            return mCommonService.ExecuteOrder(request.CentralBuildingID, async orders =>
            {
                await orders.SetWaypoint(request.Waypoint.ToUnity());
                return new Empty();
            });
        }

        public void Register(ICentralBuildingOrders orders, ICentralBuildingInfo info)
        {
            mCommonService.Register(orders, info);
        }
    }
}