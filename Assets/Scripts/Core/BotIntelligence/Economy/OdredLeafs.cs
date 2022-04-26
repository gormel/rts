using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Final;
using Core.BotIntelligence.Memory;

namespace Core.BotIntelligence.Economy
{
    class PlaceMiningCampLeaf : ExecuteOrderLeaf<(BuildingFastMemory, BotMemory), Worker>
    {
        public PlaceMiningCampLeaf(BotMemory memory, BuildingFastMemory fastMemory) 
            : base((fastMemory, memory))
        {
        }

        protected override Worker SelectObject((BuildingFastMemory, BotMemory) memory)
            => memory.Item1.FreeWorker;

        protected override async Task ExecuteOrder(Worker worker, (BuildingFastMemory, BotMemory) memory)
        {
            var templateId = await worker.PlaceMiningCampTemplate(memory.Item1.Place);
            memory.Item2.TemplateAttachedBuilders.AddOrUpdate(templateId, new HashSet<Guid>() { worker.ID }, (id, c) =>
            {
                c.Add(worker.ID);
                return c;
            });
        }
    }
    
    class PlaceCentralLeaf : ExecuteOrderLeaf<(BuildingFastMemory, BotMemory), Worker>
    {
        public PlaceCentralLeaf(BotMemory memory, BuildingFastMemory fastMemory) 
            : base((fastMemory, memory))
        {
        }

        protected override Worker SelectObject((BuildingFastMemory, BotMemory) memory)
            => memory.Item1.FreeWorker;

        protected override async Task ExecuteOrder(Worker worker, (BuildingFastMemory, BotMemory) memory)
        {
            var templateId = await worker.PlaceCentralBuildingTemplate(memory.Item1.Place);
            memory.Item2.TemplateAttachedBuilders.AddOrUpdate(templateId, new HashSet<Guid>() { worker.ID }, (id, c) =>
            {
                c.Add(worker.ID);
                return c;
            });
        }
    }
    
    class AttachAsBuilderLeaf : ExecuteOrderLeaf<(BuildingFastMemory, BotMemory), Worker>
    {
        public AttachAsBuilderLeaf(BotMemory memory, BuildingFastMemory fastMemory) 
            : base((fastMemory, memory))
        {
        }

        protected override Worker SelectObject((BuildingFastMemory, BotMemory) memory)
            => memory.Item1.FreeWorker;

        protected override Task ExecuteOrder(Worker worker, (BuildingFastMemory, BotMemory) memory)
        {
            memory.Item2.TemplateAttachedBuilders.AddOrUpdate(memory.Item1.Template.ID, new HashSet<Guid>() { worker.ID }, (id, c) =>
            {
                c.Add(worker.ID);
                return c;
            });
            return worker.AttachAsBuilder(memory.Item1.Template.ID);
        }
    }
    
    class QueueWorkerLeaf : ExecuteOrderLeaf<WorkerOrderingFastMemory, CentralBuilding>
    {
        public QueueWorkerLeaf(WorkerOrderingFastMemory fastMemory) 
            : base(fastMemory)
        {
        }

        protected override CentralBuilding SelectObject(WorkerOrderingFastMemory memory)
            => memory.IdleCentral;

        protected override Task ExecuteOrder(CentralBuilding worker, WorkerOrderingFastMemory memory)
        {
            return memory.IdleCentral.QueueWorker();
        }
    }
    
    class AttachToMiningLeaf : ExecuteOrderLeaf<(BotMemory, MiningFillFastMemory, BuildingFastMemory), Worker>
    {
        public AttachToMiningLeaf(BotMemory memory, MiningFillFastMemory m, BuildingFastMemory b) 
            : base((memory, m, b))
        {
        }

        protected override Worker SelectObject((BotMemory, MiningFillFastMemory, BuildingFastMemory) memory)
            => memory.Item3.FreeWorker;

        protected override Task ExecuteOrder(Worker worker, (BotMemory, MiningFillFastMemory, BuildingFastMemory) memory)
        {
            memory.Item1.MiningAttachedWorkers.AddOrUpdate(memory.Item2.FreeMining.ID, new HashSet<Guid>() { worker.ID }, (id, c) =>
            {
                c.Add(worker.ID);
                return c;
            });
            return worker.AttachToMiningCamp(memory.Item2.FreeMining.ID);
        }
    }

    class FreeMiningWorkerLeaf : ExecuteOrderLeaf<MiningFillFastMemory, MiningCamp>
    {
        public FreeMiningWorkerLeaf(MiningFillFastMemory memory) 
            : base(memory)
        {
        }

        protected override MiningCamp SelectObject(MiningFillFastMemory memory)
        {
            return memory.FreeMining;
        }

        protected override Task ExecuteOrder(MiningCamp obj, MiningFillFastMemory memory)
        {
            return obj.FreeWorker();
        }
    }
}