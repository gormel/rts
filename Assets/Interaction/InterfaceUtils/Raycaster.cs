using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Interaction.InterfaceUtils
{
    struct RaycastResult<T> where T : class
    {
        public T Object { get; private set; }
        public Vector3 HitPoint { get; private set; }

        public static readonly RaycastResult<T> Empty = new RaycastResult<T>(null, new Vector3());

        public RaycastResult(T o, Vector3 hitPoint)
        {
            Object = o;
            HitPoint = hitPoint;
        }

        public bool IsEmpty()
        {
            return Object == null;
        }
    }

    class Raycaster
    {
        private readonly Camera mCamera;

        public Raycaster(Camera camera)
        {
            mCamera = camera;
        }

        public RaycastResult<T> RaycastBase<T>(Vector2 mouse, Func<Ray, RaycastHit[]> raycaster) where T : class
        {
            var ray = mCamera.ScreenPointToRay(mouse);
            var hits = raycaster(ray);
            if (hits == null)
                return RaycastResult<T>.Empty;

            foreach (var hit in hits)
            {
                var view = hit.transform.GetComponent<T>();
                if (view != null)
                    return new RaycastResult<T>(view, hit.point);
            }

            return RaycastResult<T>.Empty;
        }

        public RaycastResult<T> Raycast<T>(Vector2 mouse) where T : class
        {
            return RaycastBase<T>(mouse, Physics.RaycastAll);
        }
    }
}
