using System;
using Assets.Core.GameObjects.Base;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Core.GameObjects.Utils
{
    static class PositionUtils
    {
        public static float DistanceTo(this Rect rect, Vector2 position)
        {
            var p = rect.center;
            var s = rect.size;
            var dx = Math.Max(Math.Abs(position.x - p.x) - s.x / 2, 0);
            var dy = Math.Max(Math.Abs(position.y - p.y) - s.y / 2, 0);
            
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        public static float DistanceTo(this IGameObjectInfo from, IGameObjectInfo target)
        {
            return DistanceTo(from.Position, target);
        }

        public static float DistanceTo(Vector2 from, IGameObjectInfo target)
        {
            if (target is IBuildingInfo building)
            {
                return new Rect(building.Position, building.Size).DistanceTo(from);
            }

            return Vector2.Distance(from, PositionOf(target));
        }

        public static int Overlapses;

        public static bool Overlaps(this IGameObjectInfo target, Rect rect)
        {
            Overlapses++;
            if (target is IBuildingInfo building)
                return rect.Overlaps(new Rect(building.Position, building.Size));

            return rect.Contains(target.Position);
        }

        public static bool Overlaps(this IGameObjectInfo target, Vector2 center, float radius)
        {
            return DistanceTo(center, target) < radius;
        }

        public static Vector2 PositionOf(IGameObjectInfo target)
        {
            if (target is IBuildingInfo building)
                return building.Size / 2 + target.Position;

            return target.Position;
        }
    }
}