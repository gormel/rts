using System;
using Assets.Core.BehaviorTree;
using Core.BotIntelligence.Memory;

namespace Core.BotIntelligence
{
    class CheckFreeIncomeLeaf : IBTreeLeaf
    {
        private readonly BotMemory mMemory;
        private readonly Func<BotMemory, float> mGetLimit;

        public CheckFreeIncomeLeaf(BotMemory memory, Func<BotMemory, float> getLimit)
        {
            mMemory = memory;
            mGetLimit = getLimit;
        }
        public BTreeLeafState Update(TimeSpan deltaTime)
        {
            return mMemory.GetIncome() - mMemory.GetOutcome() > mGetLimit(mMemory)
                ? BTreeLeafState.Successed
                : BTreeLeafState.Failed;
        }
    }
}