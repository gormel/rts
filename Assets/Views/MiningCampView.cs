using Assets.Core.GameObjects.Final;
using Assets.Views.Base;
using Assets.Views.Utils;
using UnityEngine;

namespace Assets.Views
{
    class MiningCampView : ModelSelectableView<IMinigCampOrders, IMinigCampInfo>
    {
        public override string Name => "Добытчик";
        public override Rect FlatBounds => new Rect(Info.Position, Info.Size);

        protected override void OnLoad()
        {
            transform.localScale = new Vector3(
                transform.localScale.x * Info.Size.x,
                transform.localScale.y * Mathf.Min(Info.Size.x, Info.Size.y),
                transform.localScale.z * Info.Size.y);

            RegisterProperty(new SelectableViewProperty("Mining speed", () => $"{Info.MiningSpeed} m/sec"));
        }

        private void Update()
        {
            UpdateProperties();
        }
    }
}
