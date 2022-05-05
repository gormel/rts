using Assets.Core.GameObjects.Base;
using Assets.Core.Map;
using Assets.Views.Base;
using Core.GameObjects.Final;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Views
{
    class ArtilleryView : UnitView<IArtilleryOrders, IArtilleryInfo>, ITrajectoryService
    {
        public override string Name => "Артелерия";
        public float GetTrajectoryLength(Vector2 from, Vector2 to)
        {
            return 1;
        }

        public override void OnEnemyRightClick(SelectableView view)
        {
            var pos = view.InfoBase.Position;
            if (view.InfoBase is IBuildingInfo buildingInfo)
                pos = buildingInfo.Position + buildingInfo.Size / 2;
            
            Orders.Launch(pos);
        }
    }
}