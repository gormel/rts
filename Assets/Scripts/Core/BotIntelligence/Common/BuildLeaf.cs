using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Core.BehaviorTree;
using Core.BotIntelligence.Memory;

namespace Core.BotIntelligence
{
    abstract class BuildLeaf : ExecuteTaskLeaf
    {
        private readonly BotMemory mMemory;
        private readonly BuildingFastMemory mFastMemory;

        public BuildLeaf(BotMemory memory, BuildingFastMemory fastMemory)
        {
            mMemory = memory;
            mFastMemory = fastMemory;
        }

        protected abstract Task<Guid> PlaceBuildingTemplate(BuildingFastMemory fastMemory);

        protected override async Task<BTreeLeafState> GetTask()
        {
            var worker = mFastMemory.FreeWorker;
            if (worker == null)
                return BTreeLeafState.Failed;
            
            var templateId = await PlaceBuildingTemplate(mFastMemory);
            mMemory.TemplateAttachedBuilders.AddOrUpdate(templateId, new HashSet<Guid>() { worker.ID }, (id, c) =>
            {
                c.Add(worker.ID);
                return c;
            });
            return BTreeLeafState.Successed;
        }
    }
}