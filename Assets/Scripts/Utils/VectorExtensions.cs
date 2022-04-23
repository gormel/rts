using UnityEngine;

namespace Assets.Utils
{
    static class VectorExtensions
    {
        public static Vector2 ToUnity(this Vector v)
        {
            return new Vector2(v.X, v.Y);
        }

        public static Vector ToGrpc(this Vector2 v)
        {
            return new Vector { X = v.x, Y = v.y };
        }
    }
}