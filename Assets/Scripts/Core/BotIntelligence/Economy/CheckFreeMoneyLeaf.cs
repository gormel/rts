using System;
using Assets.Core.BehaviorTree;

namespace Core.BotIntelligence
{
    class CheckFreeMoneyLeaf : IBTreeLeaf
    {
        private readonly BotMemory mMemory;
        private readonly int mLimit;

        public CheckFreeMoneyLeaf(BotMemory memory, int limit)
        {
            mMemory = memory;
            mLimit = limit;
        }
        public BTreeLeafState Update(TimeSpan deltaTime)
        {
            return mMemory.Money >= mLimit ? BTreeLeafState.Successed : BTreeLeafState.Failed;
        }
    }
}