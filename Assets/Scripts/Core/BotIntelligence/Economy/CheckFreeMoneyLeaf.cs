using System;
using Assets.Core.BehaviorTree;
using Core.BotIntelligence.Memory;

namespace Core.BotIntelligence.Economy
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