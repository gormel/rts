using System;
using System.Linq;
using Assets.Core.BehaviorTree;
using Core.BotIntelligence.Memory;

namespace Core.BotIntelligence.Technology
{
    class QueryFreeWarriorsLabLeaf : IBTreeLeaf
    {
        private readonly BotMemory mMemory;
        private readonly WarriorUpgradeFastMemory mFastMemory;

        public QueryFreeWarriorsLabLeaf(BotMemory memory, WarriorUpgradeFastMemory fastMemory)
        {
            mMemory = memory;
            mFastMemory = fastMemory;
        }
        public BTreeLeafState Update(TimeSpan deltaTime)
        {
            mFastMemory.FreeLab = mMemory.WarriorsLabs.FirstOrDefault(l => l.Queued < 1);
            return mFastMemory.FreeLab == null ? BTreeLeafState.Failed : BTreeLeafState.Successed;
        }
    }
}