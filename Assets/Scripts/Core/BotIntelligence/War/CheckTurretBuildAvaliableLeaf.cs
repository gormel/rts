using System;
using Assets.Core.BehaviorTree;
using Assets.Core.Game;

namespace Core.BotIntelligence.War
{
    class CheckTurretBuildAvaliableLeaf : IBTreeLeaf
    {
        private readonly IPlayerState mPlayer;

        public CheckTurretBuildAvaliableLeaf(IPlayerState player)
        {
            mPlayer = player;
        }
        public BTreeLeafState Update(TimeSpan deltaTime)
        {
            return mPlayer.TurretBuildingAvaliable ? BTreeLeafState.Successed : BTreeLeafState.Failed;
        }
    }
}