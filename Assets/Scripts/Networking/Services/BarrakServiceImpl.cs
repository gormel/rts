﻿using System;
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
                Base = StateUtils.CreateFactoryBuildingState(info),
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

        public override Task<QueueMeeleeResult> QueueMeelee(QueueMeeleeRequest request, ServerCallContext context)
        {
            return mCommonService.ExecuteOrder(request.Base.BuildingID, async orders =>
            {
                var result = await orders.QueueMeelee();
                return new QueueMeeleeResult { Base = new QueueUnitResult { Result = result } };
            });
        }

        public override Task<QueueArtilleryResult> QueueArtillery(QueueArtilleryRequest request, ServerCallContext context)
        {
            return mCommonService.ExecuteOrder(request.Base.BuildingID, async orders =>
            {
                var result = await orders.QueueArtillery();
                return new QueueArtilleryResult() { Base = new QueueUnitResult { Result = result } };
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
