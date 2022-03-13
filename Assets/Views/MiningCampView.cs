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

        protected override void OnLoad()
        {
            transform.localScale = new Vector3(
                transform.localScale.x * Info.Size.x,
                transform.localScale.y * Mathf.Min(Info.Size.x, Info.Size.y),
                transform.localScale.z * Info.Size.y);

            RegisterProperty(new SelectableViewProperty("Mining speed", () => $"{Info.MiningSpeed} m/sec"));
            RegisterProperty(new SelectableViewProperty("Workers", () => $"{Info.WorkerCount}"));
        }

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
        }
    }
}
