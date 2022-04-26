using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Core.BehaviorTree;
using Core.BotIntelligence.Memory;

namespace Core.BotIntelligence.War
{
    class BuildBarrakLeaf : ExecuteTaskLeaf
    {
        private readonly BotMemory mMemory;
        private readonly BuildingFastMemory mFastMemory;

        public BuildBarrakLeaf(BotMemory memory, BuildingFastMemory fastMemory)
        {
            mMemory = memory;
            mFastMemory = fastMemory;
        }
        protected override async Task<BTreeLeafState> GetTask()
        {
            if (mFastMemory.FreeWorker == null)
                return BTreeLeafState.Failed;

            var workerId = mFastMemory.FreeWorker.ID;
            var templateId = await mFastMemory.FreeWorker.PlaceBarrakTemplate(mFastMemory.Place);
            mMemory.TemplateAttachedBuilders.AddOrUpdate(templateId, new HashSet<Guid>() { workerId }, (id, c) =>
            {
                c.Add(workerId);
                return c;
            });
            return BTreeLeafState.Successed;
        }
    }

    class QueueMeleeWarriorLeaf : ExecuteTaskLeaf
    {
        private readonly WarriorOrderingFastMemory mFastMemory;

        public QueueMeleeWarriorLeaf(WarriorOrderingFastMemory fastMemory)
        {
            mFastMemory = fastMemory;
        }

        protected override async Task<BTreeLeafState> GetTask()
        {
            return await mFastMemory.IdleBarrak.QueueMeelee() ? BTreeLeafState.Successed : BTreeLeafState.Failed;
        }
    }

    class QueueRangedWarriorLeaf : ExecuteTaskLeaf
    {
        private readonly WarriorOrderingFastMemory mFastMemory;

        public QueueRangedWarriorLeaf(WarriorOrderingFastMemory fastMemory)
        {
            mFastMemory = fastMemory;
        }

        protected override async Task<BTreeLeafState> GetTask()
        {
            return await mFastMemory.IdleBarrak.QueueRanged() ? BTreeLeafState.Successed : BTreeLeafState.Failed;
        }
    }

    class AttackTargetLeaf : ExecuteTaskLeaf
    {
        private readonly AttackFastMemory mFastMemory;

        public AttackTargetLeaf(AttackFastMemory fastMemory)
        {
            mFastMemory = fastMemory;
        }

        protected override async Task<BTreeLeafState> GetTask()
        {
            if (mFastMemory.Warrior == null)
                return BTreeLeafState.Failed;
            
            if (mFastMemory.Target == null)
                return BTreeLeafState.Failed;
            
            await mFastMemory.Warrior.Attack(mFastMemory.Target.ID);
            return BTreeLeafState.Successed;
        }
    }
}