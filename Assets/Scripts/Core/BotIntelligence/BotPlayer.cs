using System;
using Assets.Core.BehaviorTree;
using Assets.Core.Game;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using Core.BotIntelligence.Economy;
using Core.BotIntelligence.Memory;

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
            var buildCentralFM = new BuildingFastMemory();
            var buildCampFM = new BuildingFastMemory();
            var miningBuilderFM = new BuildingFastMemory();
            var workerOrdFM = new WorkerOrderingFastMemory();
            var miningFillFM = new MiningFillFastMemory();
            return BTree.Create(EconomyIntelligenceTag)
                .Sequence(b => b
                    .Success(b2 => b2
                        .Sequence(b4 => b4
                            .Leaf(new QueryUnbuiltLeaf(mMemory, buildTemplateFM, 1))
                            .Leaf(new QueryFreeWorkerLeaf(mMemory, buildTemplateFM))
                            .Leaf(new AttachAsBuilderLeaf(mMemory, buildTemplateFM))
                        )
                    )
                    .Success(b2 => b2
                        .Sequence(b4 => b4
                            .Leaf(new CheckFreeMoneyLeaf(mMemory, CentralBuildingCost))
                            .Leaf(new QueryFreeWorkerLeaf(mMemory, buildCentralFM))
                            .Leaf(new FindFreePlaceLeaf(mGame, mGame.Map.Data, buildCentralFM, CentralBuilding.BuildingSize))
                            .Leaf(new PlaceCentralLeaf(mMemory, buildCentralFM))
                        )
                    )
                    .Success(b2 => b2
                        .Sequence(b4 => b4
                            .Leaf(new CheckFreeMoneyLeaf(mMemory, MiningCampCost))
                            .Leaf(new QueryFreeWorkerLeaf(mMemory, buildCampFM))
                            .Leaf(new FindFreeMiningCampPlaceLeaf(mGame, mGame.Map.Data, buildCampFM, MiningCamp.BuildingSize))
                            .Leaf(new PlaceMiningCampLeaf(mMemory, buildCampFM))
                        )
                    )
                    .Success(b2 => b2
                        .Sequence(b3 => b3
                            .Leaf(new CheckFreeMoneyLeaf(mMemory, WorkerCost))
                            .Leaf(new CheckIdleWorkerCountLeaf(mMemory, -1, 5))
                            .Leaf(new QueryIdleCentralLeaf(mMemory, workerOrdFM))
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