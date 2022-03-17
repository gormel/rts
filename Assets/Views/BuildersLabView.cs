using Assets.Core.GameObjects.Base;
using Assets.Views.Base;
using Assets.Views.Utils;
using UnityEngine;

namespace Assets.Views
{
    class BuildersLabView : BuildingView<IBuildersLabOrders, IBuildersLabInfo>
    {
        public override string Name => "Лаба строителей";
        public override Rect FlatBounds => new Rect(Info.Position, Info.Size);

        protected override void OnLoad()
        {
            RegisterProperty(new SelectableViewProperty("Current progress", () => $"{Info.Progress * 100:#0}%"));
            RegisterProperty(new SelectableViewProperty("Queued", () => $"{Info.Queued}"));
        }
    }
}