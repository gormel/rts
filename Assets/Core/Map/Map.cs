using System;
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

        private const int MountainCount = 20;
        private const int MountainSizeMin = 5;
        private const int MountainSizeMax = 10;

        private const int ForestCount = 10;
        private const float ForestPossibilityFallback = 15f;

        private const int CrystalCount = 20;
        private const int MaxCrystalPlacementTryes = 30;

        public Map(int width, int length)
        {
            var data = new float[width, length];

            for (int i = 0; i < MountainCount; i++)
            {
                var x = Random.Range(0, width);
                var y = Random.Range(0, length);
                var sizeX = Random.Range(MountainSizeMin, MountainSizeMax) / 2;
                var sizeY = Random.Range(MountainSizeMin, MountainSizeMax) / 2;
                
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
            for (int i = 0; i < ForestCount; i++)
            {
                var x = Random.Range(0, width);
                var y = Random.Range(0, length);
                GenerateForest(x, y, objs, 0);
            }

            var crystalPartSide = Mathf.Sqrt(CrystalCount);
            var crystalPartWidth = (length / crystalPartSide);
            var crystalPartHeight = (width / crystalPartSide);
            for (float w = 0; w + crystalPartWidth < length; w += crystalPartWidth)
            {
                for (float l = 0; l + crystalPartHeight < width; l += crystalPartHeight)
                {
                    for (int i = 0; i < MaxCrystalPlacementTryes; i++)
                    {
                        var x = (int)Random.Range(w, w + crystalPartWidth);
                        var y = (int)Random.Range(l, l + crystalPartHeight);

                        if (objs[x, y] == MapObject.None)
                        {
                            objs[x, y] = MapObject.Crystal;
                            break;
                        }
                    }
                }
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
                if (Random.value > depth / ForestPossibilityFallback + 0.1f)
                    GenerateForest(x + dir.x, y + dir.y, objs, depth + 1);

                dir = new Vector2Int(dir.y, -dir.x); //rotate 90 deg
            }
        }
    }
}