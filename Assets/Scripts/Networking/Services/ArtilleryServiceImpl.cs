using System;
using System.Threading.Tasks;
using Assets.Utils;
using Core.GameObjects.Final;
using Grpc.Core;

namespace Assets.Networking.Services
{
    class ArtilleryServiceImpl : ArtilleryService.ArtilleryServiceBase, IRegistrator<IArtilleryOrders, IArtilleryInfo>
    {
        private CommonListenCreationService<IArtilleryOrders, IArtilleryInfo, ArtilleryState> mCreationService;

        public ArtilleryServiceImpl()
        {
            mCreationService = new CommonListenCreationService<IArtilleryOrders, IArtilleryInfo, ArtilleryState>(CreateState);
        }

        private ArtilleryState CreateState(IArtilleryInfo info)
        {
            return new ArtilleryState()
            {
                Base = StateUtils.CreateUnitState(info),
                
                LaunchAvaliable = info.LaunchAvaliable,
                MissileSpeed = info.MissileSpeed,
                MissileRadius = info.MissileRadius,
                MissileDamage = info.MissileDamage,
                LaunchRange = info.LaunchRange,
            };
        }

        public override Task ListenCreation(Empty request, IServerStreamWriter<ArtilleryState> responseStream, ServerCallContext context)
        {
            return mCreationService.ListenCreation(responseStream, context);
        }

        public override Task ListenState(ID request, IServerStreamWriter<ArtilleryState> responseStream, ServerCallContext context)
        {
            return mCreationService.ListenState(request, responseStream, context);
        }

        public override Task<Empty> GoTo(GoToRequest request, ServerCallContext context)
        {
            return mCreationService.ExecuteOrder(request.UnitID, async orders =>
            {
                await orders.GoTo(request.Destignation.ToUnity());
                return new Empty();
            });
        }

        public override Task<Empty> Stop(StopRequest request, ServerCallContext context)
        {
            return mCreationService.ExecuteOrder(request.UnitUD, async orders =>
            {
                await orders.Stop();
                return new Empty();
            });
        }

        public override Task<Empty> Launch(LaunchReqest request, ServerCallContext context)
        {
            return mCreationService.ExecuteOrder(request.UnitID, async orders =>
            {
                await orders.Launch(request.Target.ToUnity());
                return new Empty();
            });
        }

        public void Register(IArtilleryOrders orders, IArtilleryInfo info)
        {
            mCreationService.Register(orders, info);
        }

        public void Unregister(Guid id)
        {
            mCreationService.Unregister(id);
        }
    }
}