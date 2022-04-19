using UnityEngine;

namespace Assets.Core.GameObjects.Base
{
    interface IWarriorsLabInfo : ILaboratoryBuildingInfo
    {
    }

    interface IWarriorsLabOrders : ILaboratoryBuildingOrders
    {
    }
    class WarriorsLab : LaboratoryBuilding, IWarriorsLabInfo, IWarriorsLabOrders
    {
        public static Vector2 BuildingSize = new Vector2(2, 2);
        public const int MaximumHealthConst = 250;

        protected override float MaxHealthBase { get; } = MaximumHealthConst;
        
        public WarriorsLab(Vector2 position) 
            : base(position)
        {
        }

        public override void OnAddedToGame()
        {
            Size = BuildingSize;
            ViewRadius = 3;
            
            base.OnAddedToGame();
        }
    }
}