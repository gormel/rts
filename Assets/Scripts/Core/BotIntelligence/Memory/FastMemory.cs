using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using UnityEngine;

namespace Core.BotIntelligence.Memory
{
    class BuildingFastMemory
    {
        public Worker FreeWorker { get; set; }
        public Vector2Int Place { get; set; }
        public BuildingTemplate Template { get; set; }
    }

    class WorkerOrderingFastMemory
    {
        public CentralBuilding IdleCentral { get; set; }
    }

    class AttackFastMemory
    {
        public WarriorUnit Warrior { get; set; }
        public RtsGameObject Target { get; set; }
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