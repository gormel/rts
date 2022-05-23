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
                IsAttacks = info.IsAttacks,
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

        public override Task<Empty> Attack(AttackRequest request, ServerCallContext context)
        {
            return mCommonService.ExecuteOrder(request.WarriorID, async orders =>
            {
                await orders.Attack(Guid.Parse(request.TargetID.Value));
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

        public override Task<Empty> CancelBuilding(CancelBuildingRequest request, ServerCallContext context)
        {
            return mCommonService.ExecuteOrder(request.BuildingID, async orders =>
            {
                await orders.CancelBuilding();
                return new Empty();
            });
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