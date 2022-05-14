using System;
using UnityEngine;

namespace Assets.Core.Map
{
    static class MapDataExtensions
    {
        public static float GetHeightAt(this IMapData data, Vector2 position)
        {
            var left = Mathf.FloorToInt(position.x);
            var right = Mathf.CeilToInt(position.x);
            var top = Mathf.FloorToInt(position.y);
            var bottom = Mathf.CeilToInt(position.y);

            if (left == right && top == bottom)
                return data.GetHeightAt(left, top);

            if (left == right)
            {
                var t = Mathf.Abs(position.y - bottom);
                return data.GetHeightAt(left, top) * t + data.GetHeightAt(left, bottom) * (1 - t);
            }

            if (top == bottom)
            {
                var t = Mathf.Abs(position.x - right);
                return data.GetHeightAt(left, top) * t + data.GetHeightAt(right, top) * (1 - t);
            }

            if (left < 0 || top < 0 || right >= data.Width || bottom >= data.Length)
                return 0;

            var k = (float)(top - bottom) / (left - right);
            var b = top - k * left;

            var lu = new Vector3(left, top, data.GetHeightAt(left, top));
            var middle = new Vector3(right, bottom, data.GetHeightAt(right, bottom)) - lu;
            var notMiddle = new Vector3(right, top, data.GetHeightAt(right, top)) - lu;

            if (position.y > position.x * k + b)
                notMiddle = new Vector3(left, bottom, data.GetHeightAt(left, bottom)) - lu;

            var normal = Vector3.Cross(middle, notMiddle);
            var d = -Vector3.Dot(normal, lu);
            return -(Vector2.Dot(normal, position) + d) / normal.z;
        }

        public static bool IsOutOfBounds(this IMapData data, Vector2Int pos) =>
            pos.x < 0 || pos.x >= data.Width || pos.y < 0 || pos.y >= data.Length;
        
        public static bool GetIsAreaFree(this IMapData data, Vector2 position, Vector2 size)
        {
            var intSize = new Vector2Int(Mathf.CeilToInt(size.x), Mathf.CeilToInt(size.y));
            var intPos = new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));

            if (data.IsOutOfBounds(intPos))
                return false;
            
            var toCompare = data.GetHeightAt(intPos.x, intPos.y);
            for (int x = 0; x <= intSize.x; x++)
            {
                for (int y = 0; y <= intSize.y; y++)
                {
                    var localPos = intPos + new Vector2Int(x, y);
                    if (data.IsOutOfBounds(localPos))
                        return false;

                    if (Math.Abs(toCompare - data.GetHeightAt(localPos.x, localPos.y)) > 0.001)
                        return false;

                    if (data.GetMapObjectAt(localPos.x, localPos.y) != MapObject.None && x != intSize.x && y != intSize.y)
                        return false;
                }
            }

            return true;
        }
    }
}