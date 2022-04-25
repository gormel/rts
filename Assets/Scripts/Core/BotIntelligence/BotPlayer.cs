using System;
using Assets.Core.BehaviorTree;
using Assets.Core.Game;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using Core.BotIntelligence.Economy;

namespace Core.BotIntelligence
{
    class BotPlayer : Player
    {
        public const string EconomyIntelligenceTag = "EconomyIntelligence";
        
        private readonly Game mGame;
        private BotMemory mMemory;

        private BTree mEconomyIntelligence;
        
        public BotPlayer(Game game, IGameObjectFactory externalFactory, int team) 
            : base(externalFactory, team)
        {
            mGame = game;
            mMemory = new BotMemory(this);

            mEconomyIntelligence = BuildEconomyIntelligence();
        }

        private BTree BuildEconomyIntelligence()
        {
            var buildTemplateFM = new BuildingFastMemory();
            var buildTemplateGuardFM = new ExecutionGuardFastMemory();
            var buildCentralFM = new BuildingFastMemory();
            var buildCentralGuardFM = new ExecutionGuardFastMemory();
            var buildCampFM = new BuildingFastMemory();
            var buildCampGuardFM = new ExecutionGuardFastMemory();
            var miningBuilderFM = new BuildingFastMemory();
            var workerOrdFM = new WorkerOrderingFastMemory();
            var miningFillFM = new MiningFillFastMemory();
            return BTree.Create(EconomyIntelligenceTag)
                .Sequence(b => b
                    .Success(b1 => b1
                        .Sequence(b2 => b2
                            .Selector(b3 => b3
                                .Sequence(b4 => b4
                                    .Selector(b5 => b5
                                        .Invert(b6 => b6
                                            .Leaf(new ExecutionGuardLeaf(buildTemplateGuardFM, 1, ExecutionGuardMode.Wait))
                                        )
                                        .Sequence(b6 => b6
                                            .Leaf(new QueryUnbuiltLeaf(mMemory, buildTemplateFM, 2))
                                            .Leaf(new QueryFreeOrMiningWorkerLeaf(mMemory, buildTemplateFM))
                                        )
                                    )
                                    .Leaf(new AttachAsBuilderLeaf(mMemory, buildTemplateFM))
                                    .Leaf(new ExecutionGuardLeaf(buildTemplateGuardFM, 1, ExecutionGuardMode.Release))
                                )
                                .Sequence(b4 => b4
                                    .Selector(b5 => b5
                                        .Invert(b6 => b6
                                            .Leaf(new ExecutionGuardLeaf(buildCentralGuardFM, 1, ExecutionGuardMode.Wait))
                                        )
                                        .Sequence(b6 => b6
                                            .Leaf(new CheckFreeMoneyLeaf(mMemory, CentralBuildingCost))
                                            .Leaf(new QueryFreeOrMiningWorkerLeaf(mMemory, buildCentralFM))
                                            .Leaf(new FindFreePlaceLeaf(mGame, mGame.Map.Data, buildCentralFM, CentralBuilding.BuildingSize))
                                        )
                                    )
                                    .Leaf(new PlaceCentralLeaf(mMemory, buildCentralFM))
                                    .Leaf(new ExecutionGuardLeaf(buildCentralGuardFM, 1, ExecutionGuardMode.Release))
                                )
                                .Sequence(b4 => b4
                                    .Selector(b5 => b5
                                        .Invert(b6 => b6
                                            .Leaf(new ExecutionGuardLeaf(buildCampGuardFM, 1, ExecutionGuardMode.Wait))
                                        )
                                        .Sequence(b6 => b6
                                            .Leaf(new CheckFreeMoneyLeaf(mMemory, MiningCampCost))
                                            .Leaf(new QueryFreeOrMiningWorkerLeaf(mMemory, buildCampFM))
                                            .Leaf(new FindFreeMiningCampPlaceLeaf(mGame, mGame.Map.Data, buildCampFM, MiningCamp.BuildingSize))
                                        )
                                    )
                                    .Leaf(new PlaceMiningCampLeaf(mMemory, buildCampFM))
                                    .Leaf(new ExecutionGuardLeaf(buildCampGuardFM, 1, ExecutionGuardMode.Release))
                                )
                            )
                        )
                    )
                    .Success(b2 => b2
                        .Sequence(b3 => b3
                            .Leaf(new CheckFreeMoneyLeaf(mMemory, WorkerCost))
                            .Leaf(new QueryIdleCentralLeaf(mMemory, workerOrdFM))
                            .Leaf(new CheckIdleWorkerCountLeaf(mMemory, -1, 2))
                            .Leaf(new QueueWorkerLeaf(workerOrdFM))
                        )
                    )
                    .Success(b2 => b2
                        .Sequence(b3 => b3
                            .Leaf(new CheckIdleWorkerCountLeaf(mMemory, 0, int.MaxValue))
                            .Leaf(new QueryFreeWorkerLeaf(mMemory, miningBuilderFM))
                            .Leaf(new QueryFreeMimingLeaf(mMemory, miningFillFM, -1, MiningCamp.MaxWorkers))
                            .Leaf(new AttachToMiningLeaf(mMemory, miningFillFM, miningBuilderFM))
                        )
                    )
                )
                .Build();
        }

        protected override void OnObjectCreated(RtsGameObject obj)
        {
            mMemory.Assign(obj);
        }

        public void Update(TimeSpan detaTime)
        {
            mEconomyIntelligence.Update(detaTime);
        }
    }
}