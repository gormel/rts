using UnityEngine;

namespace Assets.Core.GameObjects.Base
{
    interface IBuildingInfo : IGameObjectInfo
    {
        Vector2 Size { get; }
    }

    interface IBuildingOrders : IGameObjectOrders
    {
    }

    abstract class Building : RtsGameObject, IBuildingInfo, IBuildingOrders
    {
        public abstract Vector2 Size { get; }

        public sealed override float MaxHealth => Player.Upgrades.BuildingHealthUpgrade.Calculate(MaxHealthBase);
        public sealed override int Armour => Player.Upgrades.BuildingArmourUpgrade.Calculate(ArmourBase);

        protected override int ArmourBase => 1;

        public override void OnAddedToGame()
        {
            base.OnAddedToGame();

            Player.RegisterCreatedBuilding(GetType());
        }

        public override void OnRemovedFromGame()
        {
            base.OnRemovedFromGame();
            
            Player.FreeCreatedBuilding(GetType());
        }
    }
}