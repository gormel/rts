using System;
using Assets.Core.GameObjects.Base;
using UnityEngine;

namespace Assets.Views.Base {
    abstract class FactoryBuildingView<TOrderer, TInfo> : PlacementServiceBuildingView<TOrderer, TInfo>
        where TOrderer : IFactoryBuildingOrders
        where TInfo : IFactoryBuildingInfo
    {
        public LineRenderer WaypointLine;

        public override Rect FlatBounds => new Rect(Info.Position, Info.Size);

        protected override void Update()
        {
            base.Update();
            WaypointLine.gameObject.SetActive(IsSelected);

            WaypointLine.SetPosition(0, Map.GetWorldPosition(Info.Position + Info.Size / 2));
            WaypointLine.SetPosition(1, Map.GetWorldPosition(Info.Waypoint));
        }
    }
}