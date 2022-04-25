using System;
using System.Linq;
using Assets.Core.BehaviorTree;
using Assets.Core.GameObjects.Base;
using Core.BotIntelligence.Memory;

namespace Core.BotIntelligence.Economy
{
    class CheckIdleWorkerCountLeaf : IBTreeLeaf
    {
        private readonly BotMemory mMemory;
        private readonly int mMoreThen;
        private readonly int mLessThen;

        public CheckIdleWorkerCountLeaf(BotMemory memory, int moreThen, int lessThen)
        {
            mMemory = memory;
            mMoreThen = moreThen;
            mLessThen = lessThen;
        }
        public BTreeLeafState Update(TimeSpan deltaTime)
        {
            var idleCount = mMemory.Workers.Count(w => w.IntelligenceTag == Unit.IdleIntelligenceTag);
            return idleCount < mLessThen && idleCount > mMoreThen ? BTreeLeafState.Successed : BTreeLeafState.Failed;
        }
    }
}