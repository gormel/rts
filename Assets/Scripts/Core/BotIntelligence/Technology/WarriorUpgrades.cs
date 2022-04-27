using System;
using Assets.Core.BehaviorTree;
using Assets.Core.Game;

namespace Core.BotIntelligence.Technology
{
    class CheckWarriorDamageUpgradeAvaliableLeaf : IBTreeLeaf
    {
        private readonly Player mPlayer;

        public CheckWarriorDamageUpgradeAvaliableLeaf(Player player)
        {
            mPlayer = player;
        }
        public BTreeLeafState Update(TimeSpan deltaTime)
        {
            return mPlayer.UnitDamageUpgradeAvaliable ? BTreeLeafState.Successed : BTreeLeafState.Failed;
        }
    }
    
    class CheckWarriorArmorUpgradeAvaliableLeaf : IBTreeLeaf
    {
        private readonly Player mPlayer;

        public CheckWarriorArmorUpgradeAvaliableLeaf(Player player)
        {
            mPlayer = player;
        }
        public BTreeLeafState Update(TimeSpan deltaTime)
        {
            return mPlayer.UnitArmourUpgradeAvaliable ? BTreeLeafState.Successed : BTreeLeafState.Failed;
        }
    }
    
    class CheckWarriorRangeUpgradeAvaliableLeaf : IBTreeLeaf
    {
        private readonly Player mPlayer;

        public CheckWarriorRangeUpgradeAvaliableLeaf(Player player)
        {
            mPlayer = player;
        }
        public BTreeLeafState Update(TimeSpan deltaTime)
        {
            return mPlayer.UnitAttackRangeUpgradeAvaliable ? BTreeLeafState.Successed : BTreeLeafState.Failed;
        }
    }
}