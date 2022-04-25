using System;
using System.Linq;
using Assets.Core.BehaviorTree;
using Core.BotIntelligence.Memory;

namespace Core.BotIntelligence.Economy
{
    class QueryUnbuiltLeaf : IBTreeLeaf
    {
        private readonly BotMemory mMemory;
        private readonly BuildingFastMemory mFastMemory;
        private readonly int mBuilderLimit;

        public QueryUnbuiltLeaf(BotMemory memory, BuildingFastMemory fastMemory, int builderLimit)
        {
            mMemory = memory;
            mFastMemory = fastMemory;
            mBuilderLimit = builderLimit;
        }
        
        public BTreeLeafState Update(TimeSpan deltaTime)
        {
            mFastMemory.Template = mMemory.BuildingTemplates.FirstOrDefault(t => !mMemory.TemplateAttachedBuilders.TryGetValue(t.ID, out var builders) || builders.Count < mBuilderLimit);
            return mFastMemory.Template == null ? BTreeLeafState.Failed : BTreeLeafState.Successed;
        }
    }
}