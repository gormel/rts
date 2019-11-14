using System;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Final;
using Assets.Utils;
using Grpc.Core;

namespace Assets.Networking.Services
{
    class MiningCampServiceImpl : MiningCampService.MiningCampServiceBase, IRegistrator<IMinigCampOrders, IMinigCampInfo>
    {
        private CommonListenCreationService<IMinigCampOrders, IMinigCampInfo, MiningCampState> mCommonService;

        public MiningCampServiceImpl()
        {
            mCommonService = new CommonListenCreationService<IMinigCampOrders, IMinigCampInfo, MiningCampState>(CreateState);
        }

        private MiningCampState CreateState(IMinigCampInfo info)
        {
            return new MiningCampState
            {
                Base = StateUtils.CreateBuildingState(info),
                MiningSpeed = info.MiningSpeed
            };
        }

        public override Task ListenCreation(Empty request, IServerStreamWriter<MiningCampState> responseStream, ServerCallContext context)
        {
            return mCommonService.ListenCreation(responseStream, context);
        }

        public override Task ListenState(ID request, IServerStreamWriter<MiningCampState> responseStream, ServerCallContext context)
        {
            return mCommonService.ListenState(request, responseStream, context);
        }

        public void Register(IMinigCampOrders orders, IMinigCampInfo info)
        {
            mCommonService.Register(orders, info);
        }

        public void Unregister(Guid id)
        {
            mCommonService.Unregister(id);
        }
    }
}