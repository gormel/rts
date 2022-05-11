using System;
using Assets.Core.GameObjects.Base;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Core.GameObjects.Utils
{
    static class PositionUtils
    {
        public static float DistanceTo(Vector2 rectCenter, Vector2 rectSize, Vector2 position)
        {
            var dx = Math.Max(Math.Abs(position.x - rectCenter.x) - rectSize.x / 2, 0);
            var dy = Math.Max(Math.Abs(position.y - rectCenter.y) - rectSize.y / 2, 0);
            
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        public static float DistanceTo(this IGameObjectInfo from, IGameObjectInfo target)
        {
            return DistanceTo(from.Position, target);
        }

        public static float DistanceTo(Vector2 from, IGameObjectInfo target)
        {
            if (target is IBuildingInfo)
            {
                var size = ((IBuildingInfo) target).Size;
                return DistanceTo(target.Position + size / 2, size, from);
            }

            return Vector2.Distance(from, PositionOf(target));
        }

        public static bool Overlaps(this IGameObjectInfo target, Rect rect)
        {
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