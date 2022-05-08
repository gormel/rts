using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Utils;
using Assets.Core.Map;
using Assets.Views.Base;
using Core.GameObjects.Final;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Views
{
    class ArtilleryView : UnitView<IArtilleryOrders, IArtilleryInfo>
    {
        public override string Name => "Артелерия";

        public override void OnEnemyRightClick(SelectableView view)
        {
            Orders.Launch(PositionUtils.PositionOf(view.InfoBase));
        }
    }
}