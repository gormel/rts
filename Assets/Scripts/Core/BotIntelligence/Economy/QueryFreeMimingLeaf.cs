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
        private readonly int mMoreThan;
        private readonly int mLessThan;

        public QueryFreeMimingLeaf(BotMemory memory, MiningFillFastMemory fastMemory, int moreThan, int lessThan)
        {
            mMemory = memory;
            mFastMemory = fastMemory;
            mMoreThan = moreThan;
            mLessThan = lessThan;
        }
        public BTreeLeafState Update(TimeSpan deltaTime)
        {
            mFastMemory.FreeMining = mMemory.MiningCamps.FirstOrDefault(m =>
            {
                if (mMemory.MiningAttachedWorkers.TryGetValue(m.ID, out var workers))
                    return workers.Count < mLessThan && workers.Count > mMoreThan;

                return true;
            });
            return mFastMemory.FreeMining == null ? BTreeLeafState.Failed : BTreeLeafState.Successed;
        }
    }
}