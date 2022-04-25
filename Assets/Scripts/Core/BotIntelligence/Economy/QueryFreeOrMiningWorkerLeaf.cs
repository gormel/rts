using System;
using System.Linq;
using System.Threading.Tasks;
using Assets.Core.BehaviorTree;
using Assets.Core.GameObjects.Base;

namespace Core.BotIntelligence.Economy
{
    class QueryFreeOrMiningWorkerLeaf : ExecuteTaskLeaf
    {
        private readonly BotMemory mMemory;
        private readonly BuildingFastMemory mFastMemory;

        public QueryFreeOrMiningWorkerLeaf(BotMemory memory, BuildingFastMemory fastMemory)
        {
            mMemory = memory;
            mFastMemory = fastMemory;
        }

        protected sealed override async Task<BTreeLeafState> GetTask()
        {
            mFastMemory.FreeWorker = mMemory.Workers.FirstOrDefault(w => w.IntelligenceTag == Unit.IdleIntelligenceTag);
            if (mFastMemory.FreeWorker == null)
            {
                var camp = mMemory.MiningCamps.FirstOrDefault(c => c.WorkerCount > 0);
                if (camp != null)
                {
                    var workerId = await camp.FreeWorker();
                    if (workerId == Guid.Empty)
                        return BTreeLeafState.Failed;
                    
                    foreach (var memoryMiningAttachedWorker in mMemory.MiningAttachedWorkers)
                        memoryMiningAttachedWorker.Value.Remove(workerId);
                    
                    return BTreeLeafState.Successed;
                }
            }

            return mFastMemory.FreeWorker == null ? BTreeLeafState.Failed : BTreeLeafState.Successed;
        }
    }
}