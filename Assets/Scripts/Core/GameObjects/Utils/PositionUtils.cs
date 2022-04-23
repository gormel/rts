using System;
using Assets.Core.GameObjects.Base;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Core.GameObjects.Utils
{
    static class PositionUtils
    {

        public static float DistanceTo(this RtsGameObject from, RtsGameObject target)
        {
            if (target is Building)
            {
                var p = PositionOf(target);
                var s = ((Building) target).Size;
                var dx = Math.Max(Math.Abs(from.Position.x - p.x) - s.x / 2, 0);
                var dy = Math.Max(Math.Abs(from.Position.y - p.y) - s.y / 2, 0);

                return Mathf.Sqrt(dx * dx + dy * dy);
            }

            return Vector2.Distance(from.Position, PositionOf(target));
        }

        public static Vector2 PositionOf(RtsGameObject target)
        {
            if (target is Building)
                return ((Building) target).Size / 2 + target.Position;

            return target.Position;
        }
    }
}