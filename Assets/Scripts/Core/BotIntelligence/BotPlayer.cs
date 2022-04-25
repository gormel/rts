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
            var buildingFastMemory = new BuildingFastMemory();
            var buildingFastMemory2 = new BuildingFastMemory();
            var buildingFastMemory3 = new BuildingFastMemory();
            var miningBuilderFastMemory = new BuildingFastMemory();
            var workerOrdFastMemory = new WorkerOrderingFastMemory();
            var miningFillMemory = new MiningFillFastMemory();
            return BTree.Create(EconomyIntelligenceTag)
                .Sequence(b => b
                    .Success(b1 => b1
                        .Sequence(b2 => b2
                            .Selector(b3 => b3
                                .Sequence(b4 => b4
                                    .Leaf(new QueryUnbuiltLeaf(mMemory, buildingFastMemory, 3))
                                    .Leaf(new QueryFreeWorkerLeaf(mMemory, buildingFastMemory))
                                    .Leaf(new AttachAsBuilderLeaf(mMemory, buildingFastMemory))
                                )
                                .Sequence(b4 => b4
                                    .Leaf(new CheckFreeMoneyLeaf(mMemory, CentralBuildingCost))
                                    .Leaf(new QueryFreeOrMiningWorkerLeaf(mMemory, buildingFastMemory2))
                                    .Leaf(new FindFreePlaceLeaf(mGame, mGame.Map.Data, buildingFastMemory2, CentralBuilding.BuildingSize))
                                    .Leaf(new PlaceCentralLeaf(mMemory, buildingFastMemory2))
                                )
                                .Sequence(b4 => b4
                                    .Leaf(new CheckFreeMoneyLeaf(mMemory, MiningCampCost))
                                    .Leaf(new QueryFreeOrMiningWorkerLeaf(mMemory, buildingFastMemory3))
                                    .Leaf(new FindFreeMiningCampPlaceLeaf(mGame, mGame.Map.Data, buildingFastMemory3, MiningCamp.BuildingSize))
                                    .Leaf(new PlaceMiningCampLeaf(mMemory, buildingFastMemory3))
                                )
                            )
                        )
                    )
                    .Success(b2 => b2
                        .Sequence(b3 => b3
                            .Leaf(new CheckFreeMoneyLeaf(mMemory, WorkerCost))
                            .Leaf(new QueryIdleCentralLeaf(mMemory, workerOrdFastMemory))
                            .Leaf(new CheckIdleWorkerCountLeaf(mMemory, -1, 2))
                            .Leaf(new QueueWorkerLeaf(workerOrdFastMemory))
                        )
                    )
                    .Success(b2 => b2
                        .Sequence(b3 => b3
                            .Leaf(new CheckIdleWorkerCountLeaf(mMemory, 0, int.MaxValue))
                            .Leaf(new QueryFreeWorkerLeaf(mMemory, miningBuilderFastMemory))
                            .Leaf(new QueryFreeMimingLeaf(mMemory, miningFillMemory, -1, MiningCamp.MaxWorkers))
                            .Leaf(new AttachToMiningLeaf(mMemory, miningFillMemory, miningBuilderFastMemory))
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