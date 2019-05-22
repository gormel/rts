using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Core.Map
{
    internal class Map
    {
        private class MapData : IMapData
        {
            public int Width { get; }
            public int Length { get; }

            public float[,] mHeights;

            public MapData(int width, int length, float[,] data)
            {
                Width = width;
                Length = length;
                mHeights = data;
            }

            public float GetHeightAt(int x, int y)
            {
                return mHeights[x, y];
            }

            public float GetHeightAt(Vector2 position)
            {
                var left = Mathf.FloorToInt(position.x);
                var right = Mathf.CeilToInt(position.x);
                var top = Mathf.FloorToInt(position.y);
                var bottom = Mathf.CeilToInt(position.y);

                if (left == right && top == bottom)
                    return mHeights[left, top];

                if (left == right)
                {
                    var t = Mathf.Abs(position.y - bottom);
                    return mHeights[left, top] * t + mHeights[left, bottom] * (1 - t);
                }

                if (top == bottom)
                {
                    var t = Mathf.Abs(position.x - right);
                    return mHeights[left, top] * t + mHeights[right, top] * (1 - t);
                }

                if (left <= 0 || top <= 0 || right >= Width || bottom >= Length)
                    return 0;

                var k = (float)(top - bottom) / (left - right);
                var b = top - k * left;

                var lu = new Vector3(left, top, GetHeightAt(left, top));
                var middle = new Vector3(right, bottom, GetHeightAt(right, bottom)) - lu;
                var notMiddle = new Vector3(right, top, GetHeightAt(right, top)) - lu;

                if (position.y > position.x * k + b)
                    notMiddle = new Vector3(left, bottom, GetHeightAt(left, bottom)) - lu;

                var normal = Vector3.Cross(middle, notMiddle);
                var d = -Vector3.Dot(normal, lu);
                return -(Vector2.Dot(normal, position) + d) / normal.z;
            }

            public bool GetIsAreaFree(Vector2 position, Vector2 size)
            {
                var intSize = new Vector2Int(Mathf.CeilToInt(size.x), Mathf.CeilToInt(size.y));
                var intPos = new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));

                var toCompare = mHeights[intPos.x, intPos.y];
                for (int x = 0; x <= intSize.x; x++)
                {
                    for (int y = 0; y <= intSize.y; y++)
                    {
                        var localPos = intPos + new Vector2Int(x, y);
                        if (localPos.x < 0 || localPos.x >= Width || localPos.y < 0 || localPos.y >= Length)
                            return false;

                        if (Math.Abs(toCompare - mHeights[localPos.x, localPos.y]) > 0.001)
                            return false;
                    }
                }

                return true;
            }
        }

        public int Width => Data.Width;
        public int Length => Data.Length;

        public IMapData Data { get; }

        public Map(int width, int length)
        {
            var data = new float[width, length];

            for (int i = 0; i < 20; i++)
            {
                var x = Mathf.FloorToInt(Random.Range(1, width - 1));
                var y = Mathf.FloorToInt(Random.Range(1, length - 1));

                data[x, y] = 1;
                for (int x1 = x - 1; x1 <= x + 1; x1++)
                {
                    for (int y1 = y - 1; y1 <= y + 1; y1++)
                    {
                        if (x1 == x && y1 == y)
                            continue;

                        data[x1, y1] = 0.7f;
                    }
                }
            }

            Data = new MapData(width, length, data);
        }
    }
}