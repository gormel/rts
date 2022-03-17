using Assets.Core.GameObjects.Base;
using Assets.Views.Base;
using UnityEngine;

namespace Assets.Views
{
    class BuildersLabView : BuildingView<IBuildersLabOrders, IBuildersLabInfo>
    {
        public override string Name => "Лаба строителей";
        public override Rect FlatBounds => new Rect(Info.Position, Info.Size);
    }
}