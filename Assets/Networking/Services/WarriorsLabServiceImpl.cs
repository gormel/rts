﻿using System;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using Grpc.Core;

namespace Assets.Networking.Services
{
    class WarriorsLabServiceImpl : WarriorsLabService.WarriorsLabServiceBase, IRegistrator<IWarriorsLabOrders, IWarriorsLabInfo>
    {
        private CommonListenCreationService<IWarriorsLabOrders, IWarriorsLabInfo, WarriorsLabState> mCommonService;

        public WarriorsLabServiceImpl()
        {
            mCommonService = new CommonListenCreationService<IWarriorsLabOrders, IWarriorsLabInfo, WarriorsLabState>(CreateState);
        }

        private WarriorsLabState CreateState(IWarriorsLabInfo info)
        {
            return new WarriorsLabState
            {
                Base = StateUtils.CreateLaboratoryBuildingState(info),
            };
        }

        public override Task ListenCreation(Empty request, IServerStreamWriter<WarriorsLabState> responseStream, ServerCallContext context)
        {
            return mCommonService.ListenCreation(responseStream, context);
        }

        public override Task ListenState(ID request, IServerStreamWriter<WarriorsLabState> responseStream, ServerCallContext context)
        {
            return mCommonService.ListenState(request, responseStream, context);
        }

        public override Task<Empty> CancelOredr(CancelQueuedRequest request, ServerCallContext context)
        {
            return mCommonService.ExecuteOrder(request.ObjectID, async orders =>
            {
                await orders.CancelOrderAt(request.Index);
                return new Empty();
            });
        }

        public void Register(IWarriorsLabOrders orders, IWarriorsLabInfo info)
        {
            mCommonService.Register(orders, info);
        }

        public void Unregister(Guid id)
        {
            mCommonService.Unregister(id);
        }
    }
}