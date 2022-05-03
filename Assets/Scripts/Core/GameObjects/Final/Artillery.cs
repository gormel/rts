using Assets.Core.Game;
using Assets.Core.GameObjects.Base;
using Assets.Core.Map;
using UnityEngine;

namespace Core.GameObjects.Final
{
    interface IArtilleryInfo : IUnitInfo
    {
    }

    interface IArtilleryOrders : IUnitOrders
    {
    }
    
    class Artillery : Unit, IArtilleryInfo, IArtilleryOrders
    {
        public override float ViewRadius => 2;
        public override int Armour => Player.Upgrades.UnitArmourUpgrade.Calculate(ArmourBase);
        protected override float MaxHealthBase => 30;
        protected override int ArmourBase => 1;
        public override float Speed => 1.5f;
        
        public Artillery(Game game, IPathFinder pathFinder, Vector2 position) 
            : base(game, pathFinder, position)
        {
        }
    }
}