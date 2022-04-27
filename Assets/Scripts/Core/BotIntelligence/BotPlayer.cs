using System;
using Assets.Core.BehaviorTree;
using Assets.Core.Game;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using Core.BotIntelligence.Common;
using Core.BotIntelligence.Economy;
using Core.BotIntelligence.Memory;
using Core.BotIntelligence.Technology;
using Core.BotIntelligence.War;

namespace Core.BotIntelligence
{
    class BotPlayer : Player
    {
        public const string EconomyIntelligenceTag = "EconomyIntelligence";
        public const string WarIntelligenceTag = "WarIntelligence";
        public const string TechIntelligenceTag = "TechIntelligence";
        
        private readonly Game mGame;
        private readonly BotMemory mMemory;

        private readonly BTree mEconomyIntelligence;
        private readonly BTree mWarIntelligence;
        private readonly BTree mTechIntelligence;
        
        public BotPlayer(Game game, IGameObjectFactory externalFactory, int team) 
            : base(externalFactory, team)
        {
            mGame = game;
            mMemory = new BotMemory();

            mEconomyIntelligence = BuildEconomyIntelligence();
            mWarIntelligence = BuildWarIntelligence();
            mTechIntelligence = BuildTechIntelligence();
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
                            .Leaf(new CheckCountLessLeaf(mMemory, m => Math.Min(m.MiningCamps.Count / 2, m.MiningCamps.Count / 4 + 1), m => m.Barracks.Count))
                            .Selector(b4 => b4
                                .Leaf(new CheckCountLessLeaf(mMemory, _ => 2, m => m.Barracks.Count))
                                .Leaf(new CheckCountLessLeaf(mMemory, m => m.WarriorsLabs.Count, _ => 1))
                            )
                            .Leaf(new CheckFreeMoneyLeaf(this, BarrakCost))
                            .Leaf(new QueryFreeWorkerLeaf(mMemory, buildBarrakFM))
                            .Leaf(new FindFreePlaceLeaf(mGame, mGame.Map.Data, buildBarrakFM, Barrak.BuildingSize, 1))
                            .Leaf(new BuildBarrakLeaf(mMemory, buildBarrakFM))
                        )
                    )
                )
                .Build();
        }

        private BTree BuildTechIntelligence()
        {
            var buildWarriorsLabFM = new BuildingFastMemory();
            var warriorsUpgradeFM = new WarriorUpgradeFastMemory();
            return BTree.Create(TechIntelligenceTag)
                .Sequence(b => b
                    .Success(b2 => b2
                        .Sequence(b3 => b3
                            .Leaf(new CheckCountLessLeaf(mMemory, m => m.Barracks.Count, _ => 1))
                            .Leaf(new CheckCountLessLeaf(mMemory, _ => 1, m => m.WarriorsLabs.Count))
                            .Leaf(new CheckFreeMoneyLeaf(this, WarriorsLabCost))
                            .Leaf(new QueryFreeWorkerLeaf(mMemory, buildWarriorsLabFM))
                            .Leaf(new FindFreePlaceLeaf(mGame, mGame.Map.Data, buildWarriorsLabFM, WarriorsLab.BuildingSize, 1))
                            .Leaf(new BuildWarriorsLabLeaf(mMemory, buildWarriorsLabFM))
                        )
                    )
                    .Success(b2 => b2
                        .Selector(b3 => b3
                            .Sequence(b4 => b4
                                .Leaf(new CheckWarriorDamageUpgradeAvaliableLeaf(this))
                                .Leaf(new CheckFreeMoneyLeaf(this, UnitDamageUpgradeCost))
                                .Leaf(new QueryFreeWarriorsLabLeaf(mMemory, warriorsUpgradeFM))
                                .Leaf(new QueueUnitDamageUpgradeLeaf(warriorsUpgradeFM))
                            )
                            .Sequence(b4 => b4
                                .Leaf(new CheckWarriorRangeUpgradeAvaliableLeaf(this))
                                .Leaf(new CheckFreeMoneyLeaf(this, UnitAttackRangeUpgradeCost))
                                .Leaf(new QueryFreeWarriorsLabLeaf(mMemory, warriorsUpgradeFM))
                                .Leaf(new QueueUnitRangeUpgradeLeaf(warriorsUpgradeFM))
                            )
                            .Sequence(b4 => b4
                                .Leaf(new CheckWarriorArmorUpgradeAvaliableLeaf(this))
                                .Leaf(new CheckFreeMoneyLeaf(this, UnitArmourUpgradeCost))
                                .Leaf(new QueryFreeWarriorsLabLeaf(mMemory, warriorsUpgradeFM))
                                .Leaf(new QueueUnitArmorUpgradeLeaf(warriorsUpgradeFM))
                            )
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
            mTechIntelligence.Update(deltaTime);
        }
    }
}