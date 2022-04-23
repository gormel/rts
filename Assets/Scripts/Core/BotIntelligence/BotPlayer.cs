using System;
using Assets.Core.BehaviorTree;
using Assets.Core.Game;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;

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
            var workerOrdFastMemory = new WorkerOrderingFastMemory();
            var miningFillMemory = new MiningFillFastMemory();
            return BTree.Create(EconomyIntelligenceTag)
                .Sequence(b => b
                    .Success(b1 => b1
                        .Sequence(b2 => b2
                            .Leaf(new QueryFreeWorkerLeaf(mMemory, buildingFastMemory))
                            .Selector(b3 => b3
                                .Sequence(b4 => b4
                                    .Leaf(new QueryUnbuiltLeaf(mMemory, buildingFastMemory, 3))
                                    .Leaf(new ExecuteOrderLeaf<BuildingFastMemory, Worker>(buildingFastMemory, m => m.FreeWorker, (w, m) => w.AttachAsBuilder(m.Template.ID))))
                                .Sequence(b4 => b4
                                    .Leaf(new CheckFreeMoneyLeaf(mMemory, CentralBuildingCost))
                                    .Leaf(new FindFreePlaceLeaf(mGame, mGame.Map.Data, buildingFastMemory, CentralBuilding.BuildingSize))
                                    .Leaf(new ExecuteOrderLeaf<BuildingFastMemory, Worker>(buildingFastMemory, m => m.FreeWorker, (w, m) => w.PlaceCentralBuildingTemplate(m.Place)))
                                )
                                .Sequence(b4 => b4
                                    .Leaf(new CheckFreeMoneyLeaf(mMemory, MiningCampCost))
                                    .Leaf(new FindFreeMiningCampPlaceLeaf(mGame, mGame.Map.Data, buildingFastMemory, MiningCamp.BuildingSize))
                                    .Leaf(new ExecuteOrderLeaf<BuildingFastMemory, Worker>(buildingFastMemory, m => m.FreeWorker, (w, m) => w.PlaceMiningCampTemplate(m.Place)))
                                )
                            )
                        )
                    )
                    .Success(b2 => b2
                        .Sequence(b3 => b3
                            .Leaf(new CheckFreeMoneyLeaf(mMemory, WorkerCost))
                            .Leaf(new QueryIdleCentralLeaf(mMemory, workerOrdFastMemory))
                            .Leaf(new CheckIdleWorkerCountLeaf(mMemory, -1, 2))
                            .Leaf(new ExecuteOrderLeaf<WorkerOrderingFastMemory, CentralBuilding>(workerOrdFastMemory, m => m.IdleCentral, (c, m) => c.QueueWorker()))
                        )
                    )
                    .Success(b2 => b2
                        .Sequence(b3 => b3
                            .Leaf(new CheckIdleWorkerCountLeaf(mMemory, 1, int.MaxValue))
                            .Leaf(new QueryFreeWorkerLeaf(mMemory, buildingFastMemory))
                            .Leaf(new QueryFreeMimingLeaf(mMemory, miningFillMemory))
                            .Leaf(new ExecuteOrderLeaf<(MiningFillFastMemory m, BuildingFastMemory b), Worker>((miningFillMemory, buildingFastMemory), m => m.b.FreeWorker, (w, m) => w.AttachToMiningCamp(m.m.FreeMining.ID)))
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