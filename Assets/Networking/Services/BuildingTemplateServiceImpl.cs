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
    class BuildingTemplateServiceImpl : BuildingTemplateService.BuildingTemplateServiceBase, IRegistrator<IBuildingTemplateOrders, IBuildingTemplateInfo>
    {
        private CommonListenCreationService<IBuildingTemplateOrders, IBuildingTemplateInfo, BuildingTemplateState> mCommonService;

        public BuildingTemplateServiceImpl()
        {
            mCommonService = new CommonListenCreationService<IBuildingTemplateOrders, IBuildingTemplateInfo, BuildingTemplateState>(CreateState);
        }

        private BuildingTemplateState CreateState(IBuildingTemplateInfo info)
        {
            return new BuildingTemplateState
            {
                Base = new BuildingState
                {
                    Base = new ObjectState
                    {
                        ID = new ID { Value = info.ID.ToString() },
                        Health = info.Health,
                        MaxHealth = info.MaxHealth,
                        Position = info.Position.ToGrpc(),
                        PlayerID = new ID { Value = info.PlayerID.ToString() }
                    },
                    Size = info.Size.ToGrpc(),
                    Waypoint = info.Waypoint.ToGrpc()
                },
                AttachedWorkers = info.AttachedWorkers,
                Progress = info.Progress
            };
        }

        public override Task ListenCreation(Empty request, IServerStreamWriter<BuildingTemplateState> responseStream, ServerCallContext context)
        {
            return mCommonService.ListenCreation(responseStream, context);
        }

        public override Task ListenState(ID request, IServerStreamWriter<BuildingTemplateState> responseStream, ServerCallContext context)
        {
            return mCommonService.ListenState(request, responseStream, context);
        }

        public override Task<Empty> Cancel(CancelRequest request, ServerCallContext context)
        {
            return mCommonService.ExecuteOrder(request.BuildingTemplateID, async orders =>
            {
                await orders.Cancel();
                return new Empty();
            });
        }

        public void Register(IBuildingTemplateOrders orders, IBuildingTemplateInfo info)
        {
            mCommonService.Register(orders, info);
        }

        public void Unregister(Guid id)
        {
            mCommonService.Unregister(id);
        }
    }
}
