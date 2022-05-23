using System;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using Grpc.Core;

namespace Assets.Networking.Services
{
    class BuildersLabServiceImpl : BuildersLabService.BuildersLabServiceBase, IRegistrator<IBuildersLabOrders, IBuildersLabInfo>
    {
        private CommonListenCreationService<IBuildersLabOrders, IBuildersLabInfo, BuildersLabState> mCommonService;

        public BuildersLabServiceImpl()
        {
            mCommonService = new CommonListenCreationService<IBuildersLabOrders, IBuildersLabInfo, BuildersLabState>(CreateState);
        }

        private BuildersLabState CreateState(IBuildersLabInfo info)
        {
            return new BuildersLabState
            {
                Base = StateUtils.CreateLaboratoryBuildingState(info),
            };
        }

        public override Task ListenCreation(Empty request, IServerStreamWriter<BuildersLabState> responseStream, ServerCallContext context)
        {
            return mCommonService.ListenCreation(responseStream, context);
        }

        public override Task ListenState(ID request, IServerStreamWriter<BuildersLabState> responseStream, ServerCallContext context)
        {
            return mCommonService.ListenState(request, responseStream, context);
        }

        public override Task<Empty> QueueAttackUpgrade(QueueAttackUpgradeRequest request, ServerCallContext context)
        {
            return mCommonService.ExecuteOrder(request.BuildingID, async orders =>
            {
                await orders.QueueAttackUpgrade();
                return new Empty();
            });
        }

        public override Task<Empty> QueueDefenceUpgrade(QueueDefenceUpgradeRequest request, ServerCallContext context)
        {
            return mCommonService.ExecuteOrder(request.BuildingID, async orders =>
            {
                await orders.QueueDefenceUpgrade();
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

        public override Task<Empty> CancelBuilding(CancelBuildingRequest request, ServerCallContext context)
        {
            return mCommonService.ExecuteOrder(request.BuildingID, async orders =>
            {
                await orders.CancelBuilding();
                return new Empty();
            });
        }

        public void Register(IBuildersLabOrders orders, IBuildersLabInfo info)
        {
            mCommonService.Register(orders, info);
        }

        public void Unregister(Guid id)
        {
            mCommonService.Unregister(id);
        }
    }
}