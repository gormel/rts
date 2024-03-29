using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.BehaviorTree;
using Assets.Core.GameObjects.Base;
using Core.BotIntelligence.Memory;
using Core.GameObjects.Final;
using Random = UnityEngine.Random;

namespace Core.BotIntelligence.War
{
    class QueryIdleWarriorLeaf : IBTreeLeaf
    {
        private readonly BotMemory mMemory;
        private readonly AttackFastMemory mFastMemory;

        public QueryIdleWarriorLeaf(BotMemory memory, AttackFastMemory fastMemory)
        {
            mMemory = memory;
            mFastMemory = fastMemory;
        }

        public BTreeLeafState Update(TimeSpan deltaTime)
        {
            IEnumerable<WarriorUnit> collection = Random.value < 0.5 ? mMemory.MeeleeWarriors : mMemory.RangedWarriors;
            mFastMemory.Warrior = collection.FirstOrDefault(w => 
                w.IntelligenceTag is 
                    Unit.IdleIntelligenceTag or 
                    WarriorUnit.AggressiveIdleIntelligenceTag or 
                    WarriorUnit.DefenciveIdleIntelligenceTag);

            return mFastMemory.Warrior == null ? BTreeLeafState.Failed : BTreeLeafState.Successed;
        }
    }
    class QueryIdleArtilleryLeaf : IBTreeLeaf
    {
        private readonly BotMemory mMemory;
        private readonly SiedgeFastMemory mFastMemory;

        public QueryIdleArtilleryLeaf(BotMemory memory, SiedgeFastMemory fastMemory)
        {
            mMemory = memory;
            mFastMemory = fastMemory;
        }

        public BTreeLeafState Update(TimeSpan deltaTime)
        {
            IEnumerable<Artillery> collection = mMemory.Artilleries;
            mFastMemory.Artillery = collection.FirstOrDefault(w => 
                w.IntelligenceTag is Unit.IdleIntelligenceTag && w.LaunchAvaliable);

            return mFastMemory.Artillery == null ? BTreeLeafState.Failed : BTreeLeafState.Successed;
        }
    }
}