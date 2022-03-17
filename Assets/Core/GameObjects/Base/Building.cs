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
        public Vector2 Size { get; protected set; }

        public sealed override float MaxHealth => Player.Upgrades.BuildingDefenceUpgrade.Calculate(MaxHealthBase);

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