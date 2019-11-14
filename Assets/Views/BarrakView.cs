using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Final;
using Assets.Core.GameObjects.Utils;
using Assets.Views.Base;
using Assets.Views.Utils;
using UnityEngine;

namespace Assets.Views
{
    class BarrakView : FactoryBuildingView<IBarrakOrders, IBarrakInfo>
    {
        public override string Name { get; } = "Барак";
        public override Rect FlatBounds => new Rect(Info.Position, Info.Size);

        protected override void OnLoad()
        {
            transform.localScale = new Vector3(
                transform.localScale.x * Info.Size.x,
                transform.localScale.y * Mathf.Min(Info.Size.x, Info.Size.y),
                transform.localScale.z * Info.Size.y);

            RegisterProperty(new SelectableViewProperty("Current progress", () => $"{Info.Progress * 100:#0}%"));
            RegisterProperty(new SelectableViewProperty("Queued", () => $"{Info.Queued}"));
        }

        public override void OnRightClick(Vector2 position)
        {
            Orders.SetWaypoint(position);
        }
    }
}
