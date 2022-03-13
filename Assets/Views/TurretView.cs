using Assets.Core.GameObjects.Final;
using Assets.Views.Base;
using UnityEngine;

namespace Assets.Views
{
    class TurretView : BuildingView<ITurretOrders, ITurretInfo>
    {
        public override string Name => "Турель";
        public override Rect FlatBounds => new Rect(Info.Position, Info.Size);

        protected override void OnLoad()
        {
            transform.localScale = new Vector3(
                transform.localScale.x * Info.Size.x,
                transform.localScale.y * Mathf.Min(Info.Size.x, Info.Size.y),
                transform.localScale.z * Info.Size.y);
        }

        protected override void Update()
        {
            base.Update();
        }
    }
}