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
using UnityEngine;

namespace Assets.Networking.Services
{
    class WorkerServiceImpl : WorkerService.WorkerServiceBase, IRegistrator<IWorkerOrders, IWorkerInfo>
    {
        private CommonListenCreationService<IWorkerOrders, IWorkerInfo, WorkerState> mCommonService;

        public WorkerServiceImpl()
        {
            mCommonService = new CommonListenCreationService<IWorkerOrders, IWorkerInfo, WorkerState>(CreateState);
        }

        private WorkerState CreateState(IWorkerInfo info)
        {
            return new WorkerState
            {
                Base = StateUtils.CreateUnitState(info),
                IsBuilding = info.IsBuilding,
                IsAttachedToMiningCamp = info.IsAttachedToMiningCamp,
            };
        }

        public override Task ListenCreation(Empty request, IServerStreamWriter<WorkerState> responseStream, ServerCallContext context)
        {
            return mCommonService.ListenCreation(responseStream, context);
        }

        public override Task ListenState(ID request, IServerStreamWriter<WorkerState> responseStream, ServerCallContext context)
        {
            return mCommonService.ListenState(request, responseStream, context);
        }

        public override Task<Empty> GoTo(GoToRequest request, ServerCallContext context)
        {
            return mCommonService.ExecuteOrder(request.UnitID, async orders =>
            {
                await orders.GoTo(request.Destignation.ToUnity());
                return new Empty();
            });
        }

        public override Task<Empty> Stop(StopRequest request, ServerCallContext context)
        {
            return mCommonService.ExecuteOrder(request.UnitUD, async orders =>
            {
                await orders.Stop();
                return new Empty();
            });
        }

        public override Task<ID> PlaceBuildersLabTemplate(PlaceBuildersLabTemplateRequest request, ServerCallContext context)
        {
            return mCommonService.ExecuteOrder(request.WorkerID, async orders =>
            {
                var id = await orders.PlaceBuildersLabTemplate(new Vector2Int((int)request.Position.X, (int)request.Position.Y));
                return new ID { Value = id.ToString() };
            });
        }

        public override Task<ID> PlaceCentralBuildingTemplate(PlaceCentralBuildingTemplateRequest request, ServerCallContext context)
        {
            return mCommonService.ExecuteOrder(request.WorkerID, async orders =>
            {
                var id = await orders.PlaceCentralBuildingTemplate(new Vector2Int((int)request.Position.X, (int)request.Position.Y));
                return new ID { Value = id.ToString() };
            });
        }

        public override Task<ID> PlaceMiningCampTemplate(PlaceMiningCampTemplateRequest request, ServerCallContext context)
        {
            return mCommonService.ExecuteOrder(request.WorkerID, async orders =>
            {
                var id = await orders.PlaceMiningCampTemplate(new Vector2Int((int)request.Position.X, (int)request.Position.Y));
                return new ID { Value = id.ToString() };
            });
        }

        public override Task<Empty> AttachAsBuilder(AttachAsBuilderRequest request, ServerCallContext context)
        {
            return mCommonService.ExecuteOrder(request.WorkerID, async orders =>
            {
                await orders.AttachAsBuilder(Guid.Parse(request.BuildingTemplateID.Value));
                return new Empty();
            });
        }

        public override Task<ID> PlaceBarrakTemplate(PlaceBarrakTemplateRequest request, ServerCallContext context)
        {
            return mCommonService.ExecuteOrder(request.WorkerID, async orders =>
            {
                var id = await orders.PlaceBarrakTemplate(new Vector2Int((int)request.Position.X, (int)request.Position.Y));
                return new ID { Value = id.ToString() };
            });
        }

        public override Task<ID> PlaceTurretTemplate(PlaceTurretTemplateRequest request, ServerCallContext context)
        {
            return mCommonService.ExecuteOrder(request.WorkerID, async orders =>
            {
                var id = await orders.PlaceTurretTemplate(new Vector2Int((int)request.Position.X, (int)request.Position.Y));
                return new ID { Value = id.ToString() };
            });
        }

        public override Task<Empty> AttachToMiningCamp(AttachToMiningCampRequest request, ServerCallContext context)
        {
            return mCommonService.ExecuteOrder(request.WorkerID, async orders =>
            {
                await orders.AttachToMiningCamp(Guid.Parse(request.CampID.Value));
                return new Empty();
            });
        }

        public void Register(IWorkerOrders orders, IWorkerInfo info)
        {
            mCommonService.Register(orders, info);
        }

        public void Unregister(Guid id)
        {
            mCommonService.Unregister(id);
        }
    }
}
