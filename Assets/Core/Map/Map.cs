using TMPro;
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

            var objs = new MapObject[width, length];
            for (int i = 0; i < 10; i++)
            {
                var x = Random.Range(0, width);
                var y = Random.Range(0, length);
                GenerateForest(x, y, objs, 0);
            }

            for (int i = 0; i < 20; i++)
            {
                var x = Random.Range(0, width - 1);
                var y = Random.Range(0, length - 1);
                if (objs[x, y] != MapObject.None)
                {
                    i--;
                    continue;
                }

                objs[x, y] = MapObject.Crystal;
            }

            Data = new MapData(width, length, data, objs);
        }

        private void GenerateForest(int x, int y, MapObject[,] objs, int depth)
        {
            if (x < 0 || x >= objs.GetLength(0) - 1)
                return;

            if (y < 0 || y >= objs.GetLength(1) - 1)
                return;

            if (objs[x, y] != MapObject.None)
                return;

            objs[x, y] = MapObject.Tree;

            var dir = new Vector2Int(-1, 0);
            for (int i = 0; i < 9; i++)
            {
                if (Random.value > depth / 15f + 0.1f)
                    GenerateForest(x + dir.x, y + dir.y, objs, depth + 1);

                dir = new Vector2Int(dir.y, -dir.x); //rotate 90 deg
            }
        }
    }
}