using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Game;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;

namespace Core.BotIntelligence.Memory
{
    class BotMemory
    {
        private readonly BotPlayer mPlayer;
        public List<CentralBuilding> CentralBuildings { get; } = new();
        public List<Worker> Workers { get; } = new();
        public List<MiningCamp> MiningCamps { get; } = new();
        public List<BuildingTemplate> BuildingTemplates { get; } = new();
        public List<Barrak> Barracks { get; } = new();
        public List<MeeleeWarrior> MeeleeWarriors { get; } = new();
        public List<RangedWarrior> RangedWarriors { get; } = new();
        public List<WarriorsLab> WarriorsLabs { get; } = new();
        public ConcurrentDictionary<Guid, HashSet<Guid>> TemplateAttachedBuilders { get; } = new();
        public ConcurrentDictionary<Guid, HashSet<Guid>> MiningAttachedWorkers { get; } = new();

        public float BarrackOutcome 
        { 
            get 
            {
                var meleeOutcome = Barrak.MeleeWarriorCost / (float)Barrak.MeeleeWarriorProductionTime.TotalSeconds;
                var rangedOutcome = Barrak.RangedWarriorCost / (float)Barrak.RangedWarriorProductionTime.TotalSeconds;
                return (meleeOutcome + rangedOutcome) / 2;
            }
        }

        public float WarriorLabOutcome
        {
            get
            {
                var damageUpgradeOutcome = WarriorsLab.UnitDamageUpgradeCost / (float)WarriorsLab.DamageUpgradeTime.TotalSeconds * (mPlayer.UnitDamageUpgradeAvaliable ? 1 : 0);
                var armorUpgradeOutcome = WarriorsLab.UnitArmourUpgradeCost / (float)WarriorsLab.ArmourUpgradeTime.TotalSeconds * (mPlayer.UnitArmourUpgradeAvaliable ? 1 : 0);
                var rangeUpgradeOutcome = WarriorsLab.UnitAttackRangeUpgradeCost / (float)WarriorsLab.AttackRangeUpgradeTime.TotalSeconds * (mPlayer.UnitAttackRangeUpgradeAvaliable ? 1 : 0);

                return (damageUpgradeOutcome + armorUpgradeOutcome + rangeUpgradeOutcome) / 3;
            }
        }

        public float CentralOutcome
        {
            get
            {
                var workerOutcome = CentralBuilding.WorkerCost / (float) CentralBuilding.WorkerProductionTime.TotalSeconds;
                return workerOutcome;
            }
        }
        
        public BotMemory(BotPlayer player)
        {
            mPlayer = player;
        }
        
        public float GetIncome()
        {
            return MiningCamps.Sum(c => c.MiningSpeed);
        }

        public float GetOutcome()
        {
            return Barracks.Count * BarrackOutcome + 
                   CentralBuildings.Count * CentralOutcome +
                   WarriorsLabs.Count * WarriorLabOutcome;
        }
        
        public void Assign(RtsGameObject obj)
        {
            switch (obj)
            {
                case WarriorsLab warriorsLab:
                    WarriorsLabs.Add(warriorsLab);
                    warriorsLab.RemovedFromGame += o => WarriorsLabs.Remove(warriorsLab);
                    break;
                case MeeleeWarrior meeleeWarrior:
                    MeeleeWarriors.Add(meeleeWarrior);
                    meeleeWarrior.RemovedFromGame += o => MeeleeWarriors.Remove(meeleeWarrior);
                    break;
                case RangedWarrior rangedWarrior:
                    RangedWarriors.Add(rangedWarrior);
                    rangedWarrior.RemovedFromGame += o => RangedWarriors.Remove(rangedWarrior);
                    break;
                case Barrak barrak:
                    Barracks.Add(barrak);
                    barrak.RemovedFromGame += o => Barracks.Remove(barrak);
                    break;
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