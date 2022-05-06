using Assets.Core.Map;
using Assets.Utils;
using UnityEngine;

namespace Views.Projectiles
{
    class MissileSpawnerView : MonoBehaviour
    {
        public Root Root;
        
        public GameObject MissilePrefub;

        public GameObject MissilesRoot;

        public float TrajectoryHeight;
        
        public float GetTrajectoryLength(Vector2 from, Vector2 to)
        {
            var from3 = Root.MapView.GetWorldPosition(from);
            var to3 = Root.MapView.GetWorldPosition(to);
            var center = GetCenter(from3, to3);
            
            return MathUtils.GetBezierLenght(from3, center, to3, 0, 1);
        }

        public Vector3 GetTrajectoryPosition(Vector2 from, Vector2 to, float percent)
        {
            var from3 = Root.MapView.GetWorldPosition(from);
            var to3 = Root.MapView.GetWorldPosition(to);
            var center = GetCenter(from3, to3);

            return MathUtils.GetBezierPosition(from3, center, to3, percent);
        }

        private Vector3 GetCenter(Vector3 from, Vector3 to) => (from + to) / 2 + Vector3.up * TrajectoryHeight;
        
        public void Spawn(Vector2 from, Vector2 to, float speed, float radius)
        {
            var missile = Instantiate(MissilePrefub, MissilesRoot.transform, true);
            var view = missile.GetComponent<MissileView>();
            view.Init(this, Root.MapView, from, to, speed, radius);
        }
    }
}