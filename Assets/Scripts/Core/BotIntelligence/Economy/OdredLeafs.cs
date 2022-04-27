using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Core.BehaviorTree;
using Assets.Core.Game;
using Assets.Core.GameObjects.Final;
using Core.BotIntelligence.Memory;

namespace Core.BotIntelligence.Economy
{
    class PlaceMiningCampLeaf : BuildLeaf
    {
        public PlaceMiningCampLeaf(BotMemory memory, BuildingFastMemory fastMemory)
            : base(memory, fastMemory)
        {
        }

        protected override Task<Guid> PlaceBuildingTemplate(BuildingFastMemory fastMemory)
        {
            return fastMemory.FreeWorker.PlaceMiningCampTemplate(fastMemory.Place);
        }
    }
    
    class PlaceCentralLeaf : BuildLeaf
    {
        public PlaceCentralLeaf(BotMemory memory, BuildingFastMemory fastMemory) 
            : base(memory, fastMemory)
        {
        }
        
        protected override Task<Guid> PlaceBuildingTemplate(BuildingFastMemory fastMemory)
        {
            return fastMemory.FreeWorker.PlaceCentralBuildingTemplate(fastMemory.Place);
        }
    }
    
    class AttachAsBuilderLeaf : ExecuteTaskLeaf
    {
        private readonly BotMemory mMemory;
        private readonly BuildingFastMemory mFastMemory;

        public AttachAsBuilderLeaf(BotMemory memory, BuildingFastMemory fastMemory)
        {
            mMemory = memory;
            mFastMemory = fastMemory;
        }

        protected override async Task<BTreeLeafState> GetTask()
        {
            var worker = mFastMemory.FreeWorker;
            if (worker == null)
                return BTreeLeafState.Failed;
            
            mMemory.TemplateAttachedBuilders.AddOrUpdate(mFastMemory.Template.ID, new HashSet<Guid>() { worker.ID }, (id, c) =>
            {
                c.Add(worker.ID);
                return c;
            });
            await worker.AttachAsBuilder(mFastMemory.Template.ID);
            return BTreeLeafState.Successed;
        }
    }
    
    class QueueWorkerLeaf : ExecuteTaskLeaf
    {
        private readonly WorkerOrderingFastMemory mFastMemory;

        public QueueWorkerLeaf(WorkerOrderingFastMemory fastMemory)
        {
            mFastMemory = fastMemory;
        }

        protected override async Task<BTreeLeafState> GetTask()
        {
            return await mFastMemory.IdleCentral.QueueWorker() ? BTreeLeafState.Successed : BTreeLeafState.Failed;

        }
    }
    
    class AttachToMiningLeaf : ExecuteTaskLeaf
    {
        private readonly BotMemory mMemory;
        private readonly MiningFillFastMemory mMiningFastMemory;
        private readonly BuildingFastMemory mBuildingFastMemory;

        public AttachToMiningLeaf(BotMemory memory, MiningFillFastMemory miningFastMemory, BuildingFastMemory buildingFastMemory)
        {
            mMemory = memory;
            mMiningFastMemory = miningFastMemory;
            mBuildingFastMemory = buildingFastMemory;
        }

        protected override async Task<BTreeLeafState> GetTask()
        {
            var worker = mBuildingFastMemory.FreeWorker;
            if (worker == null)
                return BTreeLeafState.Failed;
            
            mMemory.MiningAttachedWorkers.AddOrUpdate(mMiningFastMemory.FreeMining.ID, new HashSet<Guid>() { worker.ID }, (id, c) =>
            {
                c.Add(worker.ID);
                return c;
            });
            await worker.AttachToMiningCamp(mMiningFastMemory.FreeMining.ID);
            return BTreeLeafState.Successed;
        }
    }

    class FreeMiningWorkerLeaf : ExecuteTaskLeaf
    {
        private readonly MiningFillFastMemory mMemory;

        public FreeMiningWorkerLeaf(MiningFillFastMemory memory)
        {
            mMemory = memory;
        }

        protected override async Task<BTreeLeafState> GetTask()
        {
            if (mMemory.FreeMining == null)
                return BTreeLeafState.Failed;
            
            await mMemory.FreeMining.FreeWorker();
            return BTreeLeafState.Successed;
        }
    }
}