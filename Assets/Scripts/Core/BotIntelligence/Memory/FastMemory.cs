using System.Collections.Generic;
using Assets.Core.GameObjects.Final;
using UnityEngine;

namespace Core.BotIntelligence
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

    class MiningFillFastMemory
    {
        public MiningCamp FreeMining { get; set; }
    }
}