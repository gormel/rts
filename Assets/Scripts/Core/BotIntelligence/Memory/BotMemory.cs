using System.CodeDom;
using System.Collections.Generic;
using Assets.Core.Game;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;

namespace Core.BotIntelligence
{
    class BotMemory
    {
        private readonly Player mPlayer;
        public List<CentralBuilding> CentralBuildings { get; } = new();
        public List<Worker> Workers { get; } = new();
        public List<MiningCamp> MiningCamps { get; } = new();
        public List<BuildingTemplate> BuildingTemplates { get; } = new();

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
                    worker.RemovedFromGame += o => Workers.Remove(worker);
                    break;
                case MiningCamp camp:
                    MiningCamps.Add(camp);
                    camp.RemovedFromGame += o => MiningCamps.Remove(camp);
                    break;
                case BuildingTemplate template:
                    BuildingTemplates.Add(template);
                    template.RemovedFromGame += o => BuildingTemplates.Remove(template);
                    break;
            }
        }
    }
}