using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Final;
using Assets.Views.Base;
using Assets.Views.Utils;
using UnityEngine;

namespace Assets.Views
{
    class BarrakView : ModelSelectableView<IBarrakOrders, IBarrakInfo>
    {
        public override string Name { get; } = "Барак";
        public override Rect FlatBounds => new Rect(Info.Position, Info.Size);

        public LineRenderer WaypointLine;

        protected override void OnLoad()
        {
            transform.localScale = new Vector3(
                transform.localScale.x * Info.Size.x,
                transform.localScale.y * Mathf.Min(Info.Size.x, Info.Size.y),
                transform.localScale.z * Info.Size.y);
        }

        private void Update()
        {
            WaypointLine.gameObject.SetActive(IsSelected);

            WaypointLine.SetPosition(0, Map.GetWorldPosition(Info.Position + Info.Size / 2));
            WaypointLine.SetPosition(1, Map.GetWorldPosition(Info.Waypoint));

            UpdateProperties();
        }
    }
}
