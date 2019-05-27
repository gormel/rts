using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Core.Map
{
    internal class Map
    {
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