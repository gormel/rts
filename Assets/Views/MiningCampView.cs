using System.Threading.Tasks;
using Assets.Core.GameObjects.Final;
using Assets.Core.GameObjects.Utils;
using Assets.Views.Base;
using Assets.Views.Utils;
using UnityEngine;

namespace Assets.Views
{
    class MiningCampView : PlacementServiceBuildingView<IMinigCampOrders, IMinigCampInfo>
    {
        public override string Name => "Добытчик";
        public override Rect FlatBounds => new Rect(Info.Position, Info.Size);

        public GameObject[] WorkerIndicators;
        public LineRenderer WaypointLine;

        protected override void Update()
        {
            base.Update();

            if (WorkerIndicators != null)
            {
                for (int i = 0; i < WorkerIndicators.Length; i++)
                {
                    WorkerIndicators[i].SetActive(Info.WorkerCount > i);
                }
            }
            
            WaypointLine.gameObject.SetActive(IsSelected && OwnershipRelation == ObjectOwnershipRelation.My);

            WaypointLine.SetPosition(0, Map.GetWorldPosition(Info.Position + Info.Size / 2));
            WaypointLine.SetPosition(1, Map.GetWorldPosition(Info.Waypoint));
        }

        public override void OnRightClick(Vector2 position)
        {
            Orders.SetWaypoint(position);
        }
    }
}
