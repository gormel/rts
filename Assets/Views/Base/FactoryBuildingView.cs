using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Utils;
using Assets.Utils;
using UnityEngine;

namespace Assets.Views.Base {
    abstract class FactoryBuildingView<TOrderer, TInfo> : BuildingView<TOrderer, TInfo>, IPlacementService
        where TOrderer : IFactoryBuildingOrders
        where TInfo : IFactoryBuildingInfo
    {
        public GameObject[] PlacementPoints;
        public HashSet<int> mLockedPoints = new HashSet<int>();

        public LineRenderer WaypointLine;

        public override Rect FlatBounds => new Rect(Info.Position, Info.Size);

        public Task<PlacementPoint> TryAllocatePoint()
        {
            return SyncContext.Execute(() =>
            {
                if (PlacementPoints == null)
                    return PlacementPoint.Invalid;

                for (int i = 0; i < PlacementPoints.Length; i++)
                {
                    if (mLockedPoints.Contains(i))
                        continue;

                    if (!IsFree(i))
                        continue;

                    mLockedPoints.Add(i);
                    return new PlacementPoint(i,
                        GameUtils.GetFlatPosition(transform.localPosition + PlacementPoints[i].transform.position -
                                                  transform.position));
                }

                return PlacementPoint.Invalid;
            });
        }

        protected override void Update()
        {
            base.Update();
            WaypointLine.gameObject.SetActive(IsSelected);

            WaypointLine.SetPosition(0, Map.GetWorldPosition(Info.Position + Info.Size / 2));
            WaypointLine.SetPosition(1, Map.GetWorldPosition(Info.Waypoint));
        }

        public Task ReleasePoint(int pointId)
        {
            return SyncContext.Execute(() => mLockedPoints.Remove(pointId));
        }

        public bool IsFree(int pointId)
        {
            var ray = new Ray(PlacementPoints[pointId].transform.position, Vector3.up);
            return !Physics.Raycast(ray);
        }
    }
}