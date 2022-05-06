using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Utils
{
    static class MathUtils
    {
        public static Vector2 ProjectToLine(Vector2 line1, Vector2 line2, Vector2 point)
        {
            var dir = line2 - line1;
            dir.Normalize();
            return line1 + dir * Vector2.Dot(point - line1, dir);
        }

        public static Vector2 ProjectToCut(Vector2 cut1, Vector2 cut2, Vector2 point)
        {
            var dir = cut1 - cut2;
            dir.Normalize();
            var projectionLenght = Vector2.Dot(point - cut1, dir);

            if (projectionLenght < 0)
                return cut1;

            if (projectionLenght * projectionLenght > (cut2 - cut1).sqrMagnitude)
                return cut2;

            return cut1 + dir * projectionLenght;
        }

        public static Vector3 GetBezierPosition(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            return Vector3.Lerp(Vector3.Lerp(a, b, t), Vector3.Lerp(b, c, t), t);
        }

        public static Vector3 GetBezierDirection(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            return (Vector3.Lerp(b, c, t) - Vector3.Lerp(a, b, t)).normalized;
        }

        public static float GetBezierLenght(Vector3 a, Vector3 b, Vector3 c, float t0, float t1)
        {
            var p0 = GetBezierPosition(a, b, c, t0);
            var p1 = GetBezierPosition(a, b, c, t1);
            var dist = Vector3.Distance(p0, p1);
            if (dist < 0.1)
                return dist;

            return GetBezierLenght(a, b, c, t0, (t0 + t1) / 2) + GetBezierLenght(a, b, c, (t0 + t1) / 2, t1);
        }

        public static float Pow(float a, int p)
        {
            var r = 1f;
            for (int i = 0; i < Math.Abs(p); i++)
            {
                r *= a;
            }

            if (p < 0)
                r = 1 / r;

            return r;
        }
    }
}
