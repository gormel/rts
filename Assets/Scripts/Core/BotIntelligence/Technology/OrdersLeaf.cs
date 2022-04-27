using System;
using System.Threading.Tasks;
using Assets.Core.BehaviorTree;
using Core.BotIntelligence.Memory;

namespace Core.BotIntelligence.Technology
{
    class BuildWarriorsLabLeaf : BuildLeaf
    {
        public BuildWarriorsLabLeaf(BotMemory memory, BuildingFastMemory fastMemory) 
            : base(memory, fastMemory)
        {
        }

        protected override Task<Guid> PlaceBuildingTemplate(BuildingFastMemory fastMemory)
        {
            return fastMemory.FreeWorker.PlaceWarriorsLabTemplate(fastMemory.Place);
        }
    }

    class QueueUnitDamageUpgradeLeaf : ExecuteTaskLeaf
    {
        private readonly WarriorUpgradeFastMemory mFastMemory;

        public QueueUnitDamageUpgradeLeaf(WarriorUpgradeFastMemory fastMemory)
        {
            mFastMemory = fastMemory;
        }
        protected override async Task<BTreeLeafState> GetTask()
        {
            if (mFastMemory.FreeLab == null)
                return BTreeLeafState.Failed;
            
            await mFastMemory.FreeLab.QueueDamageUpgrade();
            return BTreeLeafState.Successed;
        }
    }

    class QueueUnitArmorUpgradeLeaf : ExecuteTaskLeaf
    {
        private readonly WarriorUpgradeFastMemory mFastMemory;

        public QueueUnitArmorUpgradeLeaf(WarriorUpgradeFastMemory fastMemory)
        {
            mFastMemory = fastMemory;
        }
        protected override async Task<BTreeLeafState> GetTask()
        {
            if (mFastMemory.FreeLab == null)
                return BTreeLeafState.Failed;
            
            await mFastMemory.FreeLab.QueueArmourUpgrade();
            return BTreeLeafState.Successed;
        }
    }

    class QueueUnitRangeUpgradeLeaf : ExecuteTaskLeaf
    {
        private readonly WarriorUpgradeFastMemory mFastMemory;

        public QueueUnitRangeUpgradeLeaf(WarriorUpgradeFastMemory fastMemory)
        {
            mFastMemory = fastMemory;
        }
        protected override async Task<BTreeLeafState> GetTask()
        {
            if (mFastMemory.FreeLab == null)
                return BTreeLeafState.Failed;
            
            await mFastMemory.FreeLab.QueueAttackRangeUpgrade();
            return BTreeLeafState.Successed;
        }
    }
}