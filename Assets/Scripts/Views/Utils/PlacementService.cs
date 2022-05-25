using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Utils;
using Assets.Utils;
using UnityEngine;

namespace Assets.Views.Utils
{
    class PlacementService : MonoBehaviour, IPlacementService
    {
        public GameObject[] PlacementPoints;
        public HashSet<int> mLockedPoints = new HashSet<int>();
        public UnitySyncContext SyncContext { get; set; }

        private RaycastHit[] mNoAllocRaycastHits = new RaycastHit[5];

        public async Task<PlacementPoint> TryAllocatePoint()
        {
            return await SyncContext.Execute(() =>
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
                        GameUtils.GetFlatPosition(PlacementPoints[i].transform.position));
                }

                return PlacementPoint.Invalid;
            });
        }

        public async Task<PlacementPoint> TryAllocateNearestPoint(Vector2 toPoint)
        {
            return await SyncContext.Execute(() =>
            {
                if (PlacementPoints == null)
                    return PlacementPoint.Invalid;

                float nearestDist = Single.PositiveInfinity;
                int nearestPointId = -1;
                for (int i = 0; i < PlacementPoints.Length; i++)
                {
                    if (mLockedPoints.Contains(i))
                        continue;

                    if (!IsFree(i))
                        continue;

                    var distance = Vector2.Distance(GameUtils.GetFlatPosition(PlacementPoints[i].transform.position), toPoint);
                    if (distance < nearestDist)
                    {
                        nearestDist = distance;
                        nearestPointId = i;
                    }
                }

                if (nearestPointId >= 0)
                {
                    mLockedPoints.Add(nearestPointId);
                    return new PlacementPoint(nearestPointId,
                        GameUtils.GetFlatPosition(PlacementPoints[nearestPointId].transform.position));
                }

                return PlacementPoint.Invalid;
            });
        }

        public Task<bool> ReleasePoint(int pointId)
        {
            return SyncContext.Execute(() => mLockedPoints.Remove(pointId));
        }

        public bool IsFree(int pointId)
        {
            var ray = new Ray(PlacementPoints[pointId].transform.position - Vector3.up, Vector3.up);
            var size = Physics.RaycastNonAlloc(ray, mNoAllocRaycastHits, 5);
            if (size > 0)
            {
                return mNoAllocRaycastHits
                    .Take(size)
                    .All(hit => hit.transform.gameObject.GetComponent<MapView>() != null);
            }
            return true;
        }
    }
}