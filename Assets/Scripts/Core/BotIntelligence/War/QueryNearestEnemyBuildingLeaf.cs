using System;
using System.Linq;
using Assets.Core.BehaviorTree;
using Assets.Core.Game;
using Assets.Core.GameObjects.Base;
using Core.BotIntelligence.Memory;
using UnityEngine;

namespace Core.BotIntelligence.War
{
    class QueryNearestEnemyBuildingLeaf : IBTreeLeaf
    {
        private readonly Game mGame;
        private readonly IPlayerState mPlayer;
        private readonly TargetLockFastMemory mFastMemory;

        public QueryNearestEnemyBuildingLeaf(Game game, IPlayerState player, TargetLockFastMemory fastMemory)
        {
            mGame = game;
            mPlayer = player;
            mFastMemory = fastMemory;
        }

        public BTreeLeafState Update(TimeSpan deltaTime)
        {
            mFastMemory.Target = mGame.GetPlayers()
                .Where(p => p.Team != mPlayer.Team)
                .SelectMany(mGame.RequestPlayerObjects<Building>)
                .OrderBy(o => Vector2.Distance(o.Position, mFastMemory.SearchOrigin))
                .FirstOrDefault();

            return mFastMemory.Target == null ? BTreeLeafState.Failed : BTreeLeafState.Successed;
        }
    }
}