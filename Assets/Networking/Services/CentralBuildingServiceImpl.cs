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
                Base = StateUtils.CreateFactoryBuildingState(info),
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
            return mCommonService.ExecuteOrder(request.Base.BuildingID, async orders => 
                new QueueWorkerResult { Base = new QueueUnitResult { Result = await orders.QueueWorker() } });
        }

        public override Task<Empty> SetWaypoint(SetWaypointRequest request, ServerCallContext context)
        {
            return mCommonService.ExecuteOrder(request.BuildingID, async orders =>
            {
                await orders.SetWaypoint(request.Waypoint.ToUnity());
                return new Empty();
            });
        }

        public override Task<Empty> CancelOredr(CancelQueuedRequest request, ServerCallContext context)
        {
            return mCommonService.ExecuteOrder(request.ObjectID, async orders =>
            {
                await orders.CancelOrderAt(request.Index);
                return new Empty();
            });
        }

        public void Register(ICentralBuildingOrders orders, ICentralBuildingInfo info)
        {
            mCommonService.Register(orders, info);
        }

        public void Unregister(Guid id)
        {
            mCommonService.Unregister(id);
        }
    }
}