using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Utils;
using Assets.Utils;
using UnityEngine;

namespace Assets.Views.Base
{
    abstract class PlacementServiceBuildingView<TOrderer, TInfo> : BuildingView<TOrderer, TInfo>, IPlacementService
        where TOrderer : IBuildingOrders
        where TInfo : IBuildingInfo
    {
        public GameObject[] PlacementPoints;
        public HashSet<int> mLockedPoints = new HashSet<int>();

        public async Task<PlacementPoint> TryAllocatePoint()
        {
            await WaitScaledAsync();
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
                        GameUtils.GetFlatPosition(transform.localPosition + PlacementPoints[i].transform.position -
                                                  transform.position));
                }

                return PlacementPoint.Invalid;
            });
        }

        public async Task<PlacementPoint> TryAllocateNearestPoint(Vector2 toPoint)
        {
            await WaitScaledAsync();
            return await SyncContext.Execute(() =>
            {
                if (PlacementPoints == null)
                    return PlacementPoint.Invalid;

                var toPoint3D = Map.GetWorldPosition(toPoint);
                float nearestDist = Single.PositiveInfinity;
                int nearestPointId = -1;
                for (int i = 0; i < PlacementPoints.Length; i++)
                {
                    if (mLockedPoints.Contains(i))
                        continue;

                    if (!IsFree(i))
                        continue;

                    var distance = Vector3.Distance(PlacementPoints[i].transform.position, toPoint3D);
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
                        GameUtils.GetFlatPosition(transform.localPosition + PlacementPoints[nearestPointId].transform.position -
                                                  transform.position));
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
            var ray = new Ray(PlacementPoints[pointId].transform.position, Vector3.up);
            return !Physics.Raycast(ray);
        }
    }
}