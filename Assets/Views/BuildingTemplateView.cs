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
    sealed class BuildingTemplateView : ModelSelectableView<IBuildingTemplateOrders, IBuildingTemplateInfo>, IPlacementService
    {
        public override string Name => "Строительство";

        public GameObject[] BuilderPoints;
        private HashSet<int> mBusyPoints = new HashSet<int>();

        public override Rect FlatBounds => new Rect(Info.Position, Info.Size);

        protected override void OnLoad()
        {
            transform.localScale = new Vector3(transform.localScale.x * Info.Size.x, transform.localScale.y, transform.localScale.z * Info.Size.y);

            RegisterProperty(new SelectableViewProperty("Progress", () => $"{Info.Progress * 100:#0}%"));
        }

        void Update()
        {
            UpdateProperties();
        }

        public void Cancel()
        {
            Orders.Cancel();
        }

        public Task<PlacementPoint> TryAllocatePoint()
        {
            return SyncContext.Execute(() =>
            {
                if (BuilderPoints == null)
                    return PlacementPoint.Invalid;

                for (int i = 0; i < BuilderPoints.Length; i++)
                {
                    if (!mBusyPoints.Contains(i))
                    {
                        var ray = new Ray(BuilderPoints[i].transform.position, Vector3.up);
                        if (Physics.Raycast(ray))
                            continue;

                        mBusyPoints.Add(i);
                        return new PlacementPoint(i, GameUtils.GetFlatPosition(transform.localPosition + BuilderPoints[i].transform.position - transform.position));
                    }
                }

                return PlacementPoint.Invalid;
            });
        }

        public Task ReleasePoint(int pointId)
        {
            return SyncContext.Execute(() => mBusyPoints.Remove(pointId));
        }
    }
}
