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
                var x = Random.Range(0, width);
                var y = Random.Range(0, length);
                var sizeX = Random.Range(5, 10) / 2;
                var sizeY = Random.Range(5, 10) / 2;
                
                for (int x1 = x - sizeX; x1 <= x + sizeX; x1++)
                {
                    for (int y1 = y - sizeY; y1 <= y + sizeY; y1++)
                    {
                        if (x1 < 0 || y1 < 0 || x1 >= width || y1 >= width)
                            continue;

                        var h = 1f;
                        if ((x1 == x - sizeX || x1 == x + sizeX) &&
                            (y1 == y - sizeY || y1 == y + sizeY))
                            h = 0.5f;

                        data[x1, y1] = h;
                    }
                }
            }

            Data = new MapData(width, length, data);
        }
    }
}