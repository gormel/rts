﻿using System;
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
                MiningSpeed = info.MiningSpeed,
                WorkerCount = info.WorkerCount,
                Waypoint = info.Waypoint.ToGrpc(),
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

        public override Task<ID> FreeWorker(FreeWorkerRequest request, ServerCallContext context)
        {
            return mCommonService.ExecuteOrder(request.CampID, async orders =>
            {
                var id = await orders.FreeWorker();
                return new ID() { Value = id.ToString() };
            });
        }

        public override Task<Empty> CollectWorkers(CollectWorkersRequest request, ServerCallContext context)
        {
            return mCommonService.ExecuteOrder(request.CampID, async orders =>
            {
                await orders.CollectWorkers();
                return new Empty();
            });
        }

        public override Task<Empty> SetWaypoint(SetWaypointRequest request, ServerCallContext context)
        {
            return mCommonService.ExecuteOrder(request.BuildingID, async orders =>
            {
                await orders.SetWaypoint(request.Waypoint.ToUnity());
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