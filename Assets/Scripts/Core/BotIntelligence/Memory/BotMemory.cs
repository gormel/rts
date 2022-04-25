using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Assets.Core.Game;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;

namespace Core.BotIntelligence.Memory
{
    class BotMemory
    {
        private readonly Player mPlayer;
        public List<CentralBuilding> CentralBuildings { get; } = new();
        public List<Worker> Workers { get; } = new();
        public List<MiningCamp> MiningCamps { get; } = new();
        public List<BuildingTemplate> BuildingTemplates { get; } = new();
        public ConcurrentDictionary<Guid, HashSet<Guid>> TemplateAttachedBuilders { get; } = new();
        public ConcurrentDictionary<Guid, HashSet<Guid>> MiningAttachedWorkers { get; } = new();
        public HashSet<Guid> LockedWorkers { get; } = new();

        public int Money => mPlayer.Money.Resources;
        public int Limit => mPlayer.Limit.Resources;

        public BotMemory(Player player)
        {
            mPlayer = player;
        }
        
        public void Assign(RtsGameObject obj)
        {
            switch (obj)
            {
                case CentralBuilding central:
                    CentralBuildings.Add(central);
                    central.RemovedFromGame += o => CentralBuildings.Remove(central);
                    break;
                case Worker worker:
                    Workers.Add(worker);
                    worker.RemovedFromGame += o =>
                    {
                        Workers.Remove(worker);
                        foreach (var templateAttachedBuilder in TemplateAttachedBuilders)
                            templateAttachedBuilder.Value.Remove(o.ID);
                        foreach (var miningAttachedWorker in MiningAttachedWorkers)
                            miningAttachedWorker.Value.Remove(o.ID);
                    };
                    break;
                case MiningCamp camp:
                    MiningCamps.Add(camp);
                    camp.RemovedFromGame += o =>
                    {
                        MiningCamps.Remove(camp);
                        MiningAttachedWorkers.TryRemove(o.ID, out _);
                    };
                    break;
                case BuildingTemplate template:
                    BuildingTemplates.Add(template);
                    template.RemovedFromGame += o =>
                    {
                        BuildingTemplates.Remove(template);
                        TemplateAttachedBuilders.TryRemove(template.ID, out _);
                    };
                    break;
            }
        }
    }
}