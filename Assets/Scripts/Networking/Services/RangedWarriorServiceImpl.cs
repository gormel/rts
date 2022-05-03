using System;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using Assets.Utils;
using Grpc.Core;

namespace Assets.Networking.Services 
{
    class RangedWarriorServiceImpl : RangedWarriorService.RangedWarriorServiceBase, IRegistrator<IRangedWarriorOrders, IRangedWarriorInfo>
    {
        private CommonListenCreationService<IRangedWarriorOrders, IRangedWarriorInfo, RangedWarriorState> mCreationService;

        public RangedWarriorServiceImpl()
        {
            mCreationService = new CommonListenCreationService<IRangedWarriorOrders, IRangedWarriorInfo, RangedWarriorState>(CreateState);
        }

        private RangedWarriorState CreateState(IRangedWarriorInfo info)
        {
            return new RangedWarriorState
            {
                Base = StateUtils.CreateWarriorUnitState(info)
            };
        }

        public override Task ListenCreation(Empty request, IServerStreamWriter<RangedWarriorState> responseStream, ServerCallContext context)
        {
            return mCreationService.ListenCreation(responseStream, context);
        }

        public override Task ListenState(ID request, IServerStreamWriter<RangedWarriorState> responseStream, ServerCallContext context)
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

        public override Task<Empty> GoToAndAttack(GoToAndAttackRequest request, ServerCallContext context)
        {
            return mCreationService.ExecuteOrder(request.Base.UnitID, async orders =>
            {
                await orders.GoToAndAttack(request.Base.Destignation.ToUnity());
                return new Empty();
            });
        }

        public override Task<Empty> Attack(AttackRequest request, ServerCallContext context)
        {
            return mCreationService.ExecuteOrder(request.WarriorID, async orders =>
            {
                await orders.Attack(Guid.Parse(request.TargetID.Value));
                return new Empty();
            });
        }

        public override Task<Empty> SetStrategy(SetStrategyRequest request, ServerCallContext context)
        {
            return mCreationService.ExecuteOrder(request.WarriorID, async orders =>
            {
                await orders.SetStrategy((Strategy)request.Strategy);
                return new Empty();
            });
        }

        public void Register(IRangedWarriorOrders orders, IRangedWarriorInfo info)
        {
            mCreationService.Register(orders, info);
        }

        public void Unregister(Guid id)
        {
            mCreationService.Unregister(id);
        }
    }
}