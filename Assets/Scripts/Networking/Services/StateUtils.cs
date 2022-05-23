using Assets.Core.GameObjects.Base;
using Assets.Utils;

namespace Assets.Networking.Services
{
    static class StateUtils
    {
        public static ObjectState CreateObjectState(IGameObjectInfo info)
        {
            return new ObjectState
            {
                ID = new ID { Value = info.ID.ToString() },
                PlayerID = new ID { Value = info.PlayerID.ToString() },
                RecivedDamage = info.RecivedDamage,
                MaxHealth = info.MaxHealth,
                Position = info.Position.ToGrpc(),
                ViewRadius = info.ViewRadius,
                Armour = info.Armour,
            };
        }

        public static LaboratoryBuildingState CreateLaboratoryBuildingState(ILaboratoryBuildingInfo info)
        {
            return new LaboratoryBuildingState
            {
                Base = CreateBuildingState(info),
                Progress = info.Progress,
                Queued = info.Queued,
            };
        }

        public static BuildingState CreateBuildingState(IBuildingInfo info)
        {
            return new BuildingState
            {
                Base = CreateObjectState(info),
                Size = info.Size.ToGrpc(),
                Progress = info.BuildingProgress,
            };
        }

        public static FactoryBuildingState CreateFactoryBuildingState(IFactoryBuildingInfo info)
        {
            return new FactoryBuildingState
            {
                Base = CreateBuildingState(info),
                Progress = info.Progress,
                Queued = info.Queued,
                Waypoint = info.Waypoint.ToGrpc()
            };
        }

        public static UnitState CreateUnitState(IUnitInfo info)
        {
            return new UnitState
            {
                Base = CreateObjectState(info),
                Destignation = info.Destignation.ToGrpc(),
                Direction = info.Direction.ToGrpc(),
                Speed = info.Speed
            };
        }

        public static WarriorUnitState CreateWarriorUnitState(IWarriorInfo info)
        {
            return new WarriorUnitState
            {
                Base = CreateUnitState(info),
                AttackRange = info.AttackRange,
                AttackSpeed = info.AttackSpeed,
                Damage = info.Damage,
                IsAttacks = info.IsAttacks,
                Strategy = (int)info.Strategy,
                MovementState = info.MovementState,
            };
        }
    }
}
