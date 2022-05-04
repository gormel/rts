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

        public override async void OnEnemyRightClick(SelectableView view)
        {
            var missileInfo = await Orders.Launch(view.InfoBase.Position);
            
        }
    }
}