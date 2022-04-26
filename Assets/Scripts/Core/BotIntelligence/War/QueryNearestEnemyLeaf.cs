using System;
using System.Linq;
using Assets.Core.BehaviorTree;
using Assets.Core.Game;
using Assets.Core.GameObjects.Base;
using Core.BotIntelligence.Memory;
using UnityEngine;

namespace Core.BotIntelligence.War
{
    class QueryNearestEnemyLeaf : IBTreeLeaf
    {
        private readonly Game mGame;
        private readonly IPlayerState mPlayer;
        private readonly AttackFastMemory mFastMemory;

        public QueryNearestEnemyLeaf(Game game, IPlayerState player, AttackFastMemory fastMemory)
        {
            mGame = game;
            mPlayer = player;
            mFastMemory = fastMemory;
        }

        public BTreeLeafState Update(TimeSpan deltaTime)
        {
            mFastMemory.Target = mGame.GetPlayers()
                .Where(p => p.Team != mPlayer.Team)
                .SelectMany(mGame.RequestPlayerObjects<RtsGameObject>)
                .OrderBy(o => Vector2.Distance(o.Position, mFastMemory.Warrior.Position))
                .FirstOrDefault();

            return mFastMemory.Target == null ? BTreeLeafState.Failed : BTreeLeafState.Successed;
        }
    }
}