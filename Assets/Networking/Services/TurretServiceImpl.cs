using System;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Final;
using Assets.Utils;
using Grpc.Core;

namespace Assets.Networking.Services
{
    class TurretServiceImpl : TurretService.TurretServiceBase, IRegistrator<ITurretOrders, ITurretInfo>
    {
        private CommonListenCreationService<ITurretOrders, ITurretInfo, TurretState> mCommonService;

        public TurretServiceImpl()
        {
            mCommonService = new CommonListenCreationService<ITurretOrders, ITurretInfo, TurretState>(CreateState);
        }

        private TurretState CreateState(ITurretInfo info)
        {
            return new TurretState
            {
                Base = StateUtils.CreateBuildingState(info),
                Damage = info.Damage,
                AttackRange = info.AttackRange,
                AttackSpeed = info.AttackSpeed,
                Direction = info.Direction.ToGrpc(),
                IsAttacks = info.IsShooting,
            };
        }

        public override Task ListenCreation(Empty request, IServerStreamWriter<TurretState> responseStream, ServerCallContext context)
        {
            return mCommonService.ListenCreation(responseStream, context);
        }

        public override Task ListenState(ID request, IServerStreamWriter<TurretState> responseStream, ServerCallContext context)
        {
            return mCommonService.ListenState(request, responseStream, context);
        }

        public void Register(ITurretOrders orders, ITurretInfo info)
        {
            mCommonService.Register(orders, info);
        }

        public void Unregister(Guid id)
        {
            mCommonService.Unregister(id);
        }
    }
}