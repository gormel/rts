using System;
using System.Linq;
using Assets.Core.BehaviorTree;
using Core.BotIntelligence.Memory;

namespace Core.BotIntelligence.War
{
    class QueryIdleBarrakLeaf : IBTreeLeaf
    {
        private readonly BotMemory mMemory;
        private readonly WarriorOrderingFastMemory mFastMemory;

        public QueryIdleBarrakLeaf(BotMemory memory, WarriorOrderingFastMemory fastMemory)
        {
            mMemory = memory;
            mFastMemory = fastMemory;
        }
        
        public BTreeLeafState Update(TimeSpan deltaTime)
        {
            mFastMemory.IdleBarrak = mMemory.Barracks.FirstOrDefault(b => b.Queued < 1);
            return mFastMemory.IdleBarrak == null ? BTreeLeafState.Failed : BTreeLeafState.Successed;
        }
    }
}