using Assets.Core.GameObjects.Base;
using Assets.Views.Base;
using UnityEngine;

namespace Assets.Views
{
    class BuildersLabView : BuildingView<IBuildersLabOrders, IBuildersLabInfo>
    {
        public override string Name => "Лаба строителей";
        public override Rect FlatBounds => new Rect(Info.Position, Info.Size);
        
        protected override void OnLoad()
        {
            transform.localScale = new Vector3(
                transform.localScale.x * Info.Size.x,
                transform.localScale.y * Mathf.Min(Info.Size.x, Info.Size.y),
                transform.localScale.z * Info.Size.y);
        }
    }
}