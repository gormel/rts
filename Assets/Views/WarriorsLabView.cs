using Assets.Core.GameObjects.Base;
using Assets.Views.Base;
using UnityEngine;

namespace Assets.Views
{
    class WarriorsLabView : BuildingView<IWarriorsLabOrders, IWarriorsLabInfo>
    {
        public override string Name => "Лаба военоф";
        public override Rect FlatBounds => new Rect(Info.Position, Info.Size);
    }
}