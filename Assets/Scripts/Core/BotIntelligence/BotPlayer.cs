using System;
using Assets.Core.BehaviorTree;
using Assets.Core.Game;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using Core.BotIntelligence.Common;
using Core.BotIntelligence.Economy;
using Core.BotIntelligence.Memory;
using Core.BotIntelligence.War;

namespace Core.BotIntelligence
{
    class BotPlayer : Player
    {
        public const string EconomyIntelligenceTag = "EconomyIntelligence";
        public const string WarIntelligenceTag = "WarIntelligence";
        public const string TechIntelligenceTag = "TechIntelligence";
        
        private readonly Game mGame;
        private BotMemory mMemory;

        private BTree mEconomyIntelligence;
        private BTree mWarIntelligence;
        
        public BotPlayer(Game game, IGameObjectFactory externalFactory, int team) 
            : base(externalFactory, team)
        {
            mGame = game;
            mMemory = new BotMemory();

            mEconomyIntelligence = BuildEconomyIntelligence();
            mWarIntelligence = BuildWarIntelligence();
        }

        private BTree BuildEconomyIntelligence()
        {
            var buildTemplateFM = new BuildingFastMemory();
            var buildCentralFM = new BuildingFastMemory();
            var buildCampFM = new BuildingFastMemory();
            var miningBuilderFM = new BuildingFastMemory();
            var workerOrdFM = new WorkerOrderingFastMemory();
            var miningFillFM = new MiningFillFastMemory();
            var takeMinerFM = new MiningFillFastMemory();
            return BTree.Create(EconomyIntelligenceTag)
                .Sequence(b => b
                    .Success(b2 => b2
                        .Sequence(b3 => b3
                            .Leaf(new CheckIdleWorkerCountLeaf(mMemory, -1, 1))
                            .Leaf(new QueryFreeMimingLeaf(mMemory, takeMinerFM, 0, MiningCamp.MaxWorkers))
                            .Leaf(new FreeMiningWorkerLeaf(takeMinerFM))
                        )
                    )
                    .Success(b2 => b2
                        .Sequence(b4 => b4
                            .Leaf(new QueryUnbuiltLeaf(mMemory, buildTemplateFM, 1))
                            .Leaf(new QueryFreeWorkerLeaf(mMemory, buildTemplateFM))
                            .Leaf(new AttachAsBuilderLeaf(mMemory, buildTemplateFM))
                        )
                    )
                    .Success(b2 => b2
                        .Sequence(b4 => b4
                            .Leaf(new CheckCountLessLeaf(mMemory, m => m.MiningCamps.Count / 4 + 1, m => m.CentralBuildings.Count))
                            .Leaf(new CheckFreeMoneyLeaf(this, CentralBuildingCost))
                            .Leaf(new QueryFreeWorkerLeaf(mMemory, buildCentralFM))
                            .Leaf(new FindFreePlaceLeaf(mGame, mGame.Map.Data, buildCentralFM, CentralBuilding.BuildingSize, 1))
                            .Leaf(new PlaceCentralLeaf(mMemory, buildCentralFM))
                        )
                    )
                    .Success(b2 => b2
                        .Sequence(b4 => b4
                            .Leaf(new CheckFreeMoneyLeaf(this, MiningCampCost))
                            .Leaf(new QueryFreeWorkerLeaf(mMemory, buildCampFM))
                            .Leaf(new FindFreeMiningCampPlaceLeaf(mGame, mGame.Map.Data, buildCampFM, MiningCamp.BuildingSize))
                            .Leaf(new PlaceMiningCampLeaf(mMemory, buildCampFM))
                        )
                    )
                    .Success(b2 => b2
                        .Sequence(b3 => b3
                            .Leaf(new CheckCountLessLeaf(mMemory, _ => MaxLimit / 2, m => m.Workers.Count))
                            .Leaf(new CheckFreeLimitLeaf(this))
                            .Leaf(new CheckFreeMoneyLeaf(this, WorkerCost))
                            .Leaf(new CheckIdleWorkerCountLeaf(mMemory, -1, 3))
                            .Leaf(new QueryIdleCentralLeaf(mMemory, workerOrdFM))
                            .Leaf(new QueueWorkerLeaf(workerOrdFM))
                        )
                    )
                    .Success(b2 => b2
                        .Sequence(b3 => b3
                            .Leaf(new CheckIdleWorkerCountLeaf(mMemory, 1, int.MaxValue))
                            .Leaf(new QueryFreeWorkerLeaf(mMemory, miningBuilderFM))
                            .Leaf(new QueryFreeMimingLeaf(mMemory, miningFillFM, -1, MiningCamp.MaxWorkers))
                            .Leaf(new AttachToMiningLeaf(mMemory, miningFillFM, miningBuilderFM))
                        )
                    )
                )
                .Build();
        }

        private BTree BuildWarIntelligence()
        {
            var buildBarrakFM = new BuildingFastMemory();
            var rangedOrderFM = new WarriorOrderingFastMemory();
            var meleeOrderFM = new WarriorOrderingFastMemory();
            var attackFM = new AttackFastMemory();
            return BTree.Create(WarIntelligenceTag)
                .Sequence(b => b
                    .Success(b2 => b2
                        .Sequence(b3 => b3
                            .Leaf(new CheckCountLessLeaf(mMemory, m => m.MeeleeWarriors.Count, m => m.RangedWarriors.Count))
                            .Leaf(new CheckFreeMoneyLeaf(this, RangedWarriorCost))
                            .Leaf(new CheckFreeLimitLeaf(this))
                            .Leaf(new QueryIdleBarrakLeaf(mMemory, rangedOrderFM))
                            .Leaf(new QueueRangedWarriorLeaf(rangedOrderFM))
                        )
                    )
                    .Success(b2 => b2
                        .Sequence(b3 => b3
                            .Leaf(new CheckCountLessLeaf(mMemory, m => m.RangedWarriors.Count + 1, m => m.MeeleeWarriors.Count))
                            .Leaf(new CheckFreeMoneyLeaf(this, MeleeWarriorCost))
                            .Leaf(new CheckFreeLimitLeaf(this))
                            .Leaf(new QueryIdleBarrakLeaf(mMemory, meleeOrderFM))
                            .Leaf(new QueueMeleeWarriorLeaf(meleeOrderFM))
                        )
                    )
                    .Success(b2 => b2
                        .Sequence(b3  => b3
                            .Leaf(new QueryIdleWarriorLeaf(mMemory, attackFM))
                            .Leaf(new QueryNearestEnemyLeaf(mGame, this, attackFM))
                            .Leaf(new AttackTargetLeaf(attackFM))
                        )
                    )
                    .Success(b2 => b2
                        .Sequence(b3 => b3
                            .Leaf(new CheckCountLessLeaf(mMemory, m => m.MiningCamps.Count / 2, m => m.Barracks.Count))
                            .Leaf(new CheckFreeMoneyLeaf(this, BarrakCost))
                            .Leaf(new QueryFreeWorkerLeaf(mMemory, buildBarrakFM))
                            .Leaf(new FindFreePlaceLeaf(mGame, mGame.Map.Data, buildBarrakFM, Barrak.BuildingSize, 1))
                            .Leaf(new BuildBarrakLeaf(mMemory, buildBarrakFM))
                        )
                    )
                )
                .Build();
        }

        protected override void OnObjectCreated(RtsGameObject obj)
        {
            mMemory.Assign(obj);
        }

        public void Update(TimeSpan deltaTime)
        {
            mEconomyIntelligence.Update(deltaTime);
            mWarIntelligence.Update(deltaTime);
        }
    }
}