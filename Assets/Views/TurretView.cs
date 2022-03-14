using Assets.Core.GameObjects.Final;
using Assets.Views.Base;
using UnityEngine;

namespace Assets.Views
{
    class TurretView : BuildingView<ITurretOrders, ITurretInfo>
    {
        public override string Name => "Турель";
        public override Rect FlatBounds => new Rect(Info.Position, Info.Size);

        public GameObject[] LaserStarters;
        public LineRenderer[] Lasers;

        public GameObject RotationTarget;

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

            var localDirection = Info.Direction - Info.Position - Info.Size / 2;
            RotationTarget.transform.localEulerAngles = new Vector3(0, Mathf.Rad2Deg * Mathf.Atan2(localDirection.x, localDirection.y), 0);

            if (Info.IsShooting)
            {
                for (int i = 0; i < Lasers.Length; i++)
                {
                    var laser = Lasers[i];
                    var start = LaserStarters[i];
                    laser.gameObject.SetActive(true);
                    laser.SetPosition(0, start.transform.position);
                    laser.SetPosition(1, Map.GetWorldPosition(Info.Direction));
                }
            }
            else
            {
                for (int i = 0; i < Lasers.Length; i++)
                {
                    Lasers[i].gameObject.SetActive(false);
                }
            }
        }
    }
}