using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using Core.GameObjects.Final;
using UnityEngine;

namespace Core.BotIntelligence.Memory
{
    class BuildingFastMemory
    {
        public Worker FreeWorker { get; set; }
        public Vector2Int Place { get; set; }
        public Building Template { get; set; }
    }

    class WorkerOrderingFastMemory
    {
        public CentralBuilding IdleCentral { get; set; }
    }

    abstract class TargetLockFastMemory
    {
        public RtsGameObject Target { get; set; }
        
        public abstract Vector2 SearchOrigin { get; }
    }

    class AttackFastMemory : TargetLockFastMemory
    {
        public WarriorUnit Warrior { get; set; }
        public override Vector2 SearchOrigin => Warrior.Position;
    }

    class SiedgeFastMemory : TargetLockFastMemory
    {
        public Artillery Artillery { get; set; }
        public override Vector2 SearchOrigin => Artillery.Position;
    }

    class WarriorOrderingFastMemory
    {
        public Barrak IdleBarrak { get; set; }
    }

    class MiningFillFastMemory
    {
        public MiningCamp FreeMining { get; set; }
    }

    class ExecutionGuardFastMemory
    {
        public int Executions { get; set; }
    }

    class WarriorUpgradeFastMemory
    {
        public WarriorsLab FreeLab { get; set; }
    }
}