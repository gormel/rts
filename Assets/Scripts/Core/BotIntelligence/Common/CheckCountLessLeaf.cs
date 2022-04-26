using System;
using Assets.Core.BehaviorTree;
using Core.BotIntelligence.Memory;

namespace Core.BotIntelligence.Common
{
    class CheckCountLessLeaf : IBTreeLeaf
    {
        private readonly BotMemory mMemory;
        private readonly Func<BotMemory, int> mGetLimit;
        private readonly Func<BotMemory, int> mGetCount;

        public CheckCountLessLeaf(BotMemory memory, Func<BotMemory, int> getLimit, Func<BotMemory, int> getCount)
        {
            mMemory = memory;
            mGetLimit = getLimit;
            mGetCount = getCount;
        }

        public BTreeLeafState Update(TimeSpan deltaTime)
        {
            return mGetCount(mMemory) < mGetLimit(mMemory) ? BTreeLeafState.Successed : BTreeLeafState.Failed;
        }
    }
}