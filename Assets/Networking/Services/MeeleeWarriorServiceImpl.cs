using System;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Final;
using Assets.Utils;
using Grpc.Core;

namespace Assets.Networking.Services
{
    class MeeleeWarriorServiceImpl : MeeleeWarriorService.MeeleeWarriorServiceBase, IRegistrator<IMeeleeWarriorOrders, IMeeleeWarriorInfo>
    {
        private CommonListenCreationService<IMeeleeWarriorOrders, IMeeleeWarriorInfo, MeeleeWarriorState> mCreationService;

        public MeeleeWarriorServiceImpl()
        {
            mCreationService = new CommonListenCreationService<IMeeleeWarriorOrders, IMeeleeWarriorInfo, MeeleeWarriorState>(CreateState);
        }

        private MeeleeWarriorState CreateState(IMeeleeWarriorInfo info)
        {
            return new MeeleeWarriorState
            {
                Base = new WarriorUnitState
                {
                    Base = new UnitState
                    {
                        Base = new ObjectState
                        {
                            ID = new ID { Value = info.ID.ToString() },
                            Health = info.Health,
                            MaxHealth = info.MaxHealth,
                            PlayerID = new ID { Value = info.PlayerID.ToString() },
                            Position = info.Position.ToGrpc()
                        },
                        Destignation = info.Destignation.ToGrpc(),
                        Direction = info.Direction.ToGrpc(),
                        Speed = info.Speed
                    },
                    AttackRange = info.AttackRange,
                    AttackSpeed = info.AttackSpeed,
                    Damage = info.Damage,
                    IsAttacks = info.IsAttacks
                }
            };
        }

        public override Task ListenCreation(Empty request, IServerStreamWriter<MeeleeWarriorState> responseStream, ServerCallContext context)
        {
            return mCreationService.ListenCreation(responseStream, context);
        }

        public override Task ListenState(ID request, IServerStreamWriter<MeeleeWarriorState> responseStream, ServerCallContext context)
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

        public override Task<Empty> Attack(AttackRequest request, ServerCallContext context)
        {
            return mCreationService.ExecuteOrder(request.WarriorID, async orders =>
            {
                await orders.Attack(Guid.Parse(request.TargetID.Value));
                return new Empty();
            });
        }

        public void Register(IMeeleeWarriorOrders orders, IMeeleeWarriorInfo info)
        {
            mCreationService.Register(orders, info);
        }

        public void Unregister(Guid id)
        {
            mCreationService.Unregister(id);
        }
    }
}