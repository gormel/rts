using System;
using System.Linq;
using Assets.Core.BehaviorTree;
using Assets.Core.GameObjects.Final;

namespace Core.BotIntelligence
{
    class QueryFreeMimingLeaf : IBTreeLeaf
    {
        private readonly BotMemory mMemory;
        private readonly MiningFillFastMemory mFastMemory;

        public QueryFreeMimingLeaf(BotMemory memory, MiningFillFastMemory fastMemory)
        {
            mMemory = memory;
            mFastMemory = fastMemory;
        }
        public BTreeLeafState Update(TimeSpan deltaTime)
        {
            mFastMemory.FreeMining = mMemory.MiningCamps.FirstOrDefault(m => m.WorkerCount < MiningCamp.MaxWorkers);
            return mFastMemory.FreeMining == null ? BTreeLeafState.Failed : BTreeLeafState.Successed;
        }
    }
}