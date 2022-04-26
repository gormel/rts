using System;
using Assets.Core.BehaviorTree;
using Assets.Core.Game;
using Core.BotIntelligence.Memory;

namespace Core.BotIntelligence.Economy
{
    class CheckFreeMoneyLeaf : IBTreeLeaf
    {
        private readonly IPlayerState mPlayer;
        private readonly int mLimit;

        public CheckFreeMoneyLeaf(IPlayerState player, int limit)
        {
            mPlayer = player;
            mLimit = limit;
        }
        public BTreeLeafState Update(TimeSpan deltaTime)
        {
            return mPlayer.Money >= mLimit ? BTreeLeafState.Successed : BTreeLeafState.Failed;
        }
    }
    
    class CheckFreeLimitLeaf : IBTreeLeaf
    {
        private readonly IPlayerState mPlayer;

        public CheckFreeLimitLeaf(IPlayerState player)
        {
            mPlayer = player;
        }
        public BTreeLeafState Update(TimeSpan deltaTime)
        {
            return mPlayer.Limit >= 1 ? BTreeLeafState.Successed : BTreeLeafState.Failed;
        }
    }
}