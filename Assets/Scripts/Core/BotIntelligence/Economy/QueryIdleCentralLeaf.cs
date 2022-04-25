using System;
using System.Linq;
using Assets.Core.BehaviorTree;
using Core.BotIntelligence.Memory;

namespace Core.BotIntelligence.Economy
{
    class QueryIdleCentralLeaf : IBTreeLeaf
    {
        private readonly BotMemory mMemory;
        private readonly WorkerOrderingFastMemory mFastMemory;

        public QueryIdleCentralLeaf(BotMemory memory, WorkerOrderingFastMemory fastMemory)
        {
            mMemory = memory;
            mFastMemory = fastMemory;
        }
        public BTreeLeafState Update(TimeSpan deltaTime)
        {
            mFastMemory.IdleCentral = mMemory.CentralBuildings.FirstOrDefault(c => c.Queued < 1);
            return mFastMemory.IdleCentral == null ? BTreeLeafState.Failed : BTreeLeafState.Successed;
        }
    }
}