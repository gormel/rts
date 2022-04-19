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

        public override float ViewRadius => 3;
        protected override float MaxHealthBase { get; } = MaximumHealthConst;
        public override Vector2 Size => BuildingSize;
        
        public WarriorsLab(Vector2 position) 
            : base(position)
        {
        }
    }
}