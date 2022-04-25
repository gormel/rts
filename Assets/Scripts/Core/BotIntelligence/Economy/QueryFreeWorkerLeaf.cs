using System;
using System.Linq;
using Assets.Core.BehaviorTree;
using Assets.Core.GameObjects.Base;
using Core.BotIntelligence.Memory;

namespace Core.BotIntelligence.Economy
{
    class QueryFreeWorkerLeaf : IBTreeLeaf
    {
        private readonly BotMemory mMemory;
        private readonly BuildingFastMemory mFastMemory;

        public QueryFreeWorkerLeaf(BotMemory memory, BuildingFastMemory fastMemory)
        {
            mMemory = memory;
            mFastMemory = fastMemory;
        }
        public BTreeLeafState Update(TimeSpan deltaTime)
        {
            mFastMemory.FreeWorker = mMemory.Workers.FirstOrDefault(w => w.IntelligenceTag == Unit.IdleIntelligenceTag);
            return mFastMemory.FreeWorker == null ? BTreeLeafState.Failed : BTreeLeafState.Successed;
        }
    }
}