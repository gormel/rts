using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.GameObjects;
using Assets.Core.GameObjects.Final;
using Assets.Core.GameObjects.Utils;
using Assets.Utils;
using Assets.Views.Base;
using Assets.Views.Utils;
using UnityEngine;

namespace Assets.Views
{
    sealed class CentralBuildingView : ModelSelectableView<ICentralBuildingOrders, ICentralBuildingInfo>, IPlacementService
    {
        public LineRenderer WaypointLine;

        public override string Name => "Главное здание";

        public GameObject[] PlacementPoints;
        private HashSet<int> mLockedPoints = new HashSet<int>();

        protected override void OnLoad()
        {
            transform.localScale = new Vector3(
                transform.localScale.x * Info.Size.x, 
                transform.localScale.y * Mathf.Min(Info.Size.x, Info.Size.y), 
                transform.localScale.z * Info.Size.y);

            RegisterProperty(new SelectableViewProperty("Current progress", () => $"{Info.Progress * 100:#0}%"));
            RegisterProperty(new SelectableViewProperty("Queued workers", () => $"{Info.WorkersQueued}"));
        }

        public void QueueWorker()
        {
            Orders.QueueWorker();
        }

        void Update()
        {
            WaypointLine.gameObject.SetActive(IsSelected);

            WaypointLine.SetPosition(0, Map.GetWorldPosition(Info.Position + Info.Size / 2));
            WaypointLine.SetPosition(1, Map.GetWorldPosition(Info.Waypoint));

            UpdateProperties();
        }

        public override void OnRightClick(Vector2 position)
        {
            Orders.SetWaypoint(position);
        }

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
