using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.GameObjects.Final;
using Assets.Utils;
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

        private const int BasePlacementTryes = 10;

        private ConcurrentBag<Vector2> mFreeBases = new ConcurrentBag<Vector2>();

        public Map(int width, int length)
        {
            var data = new float[width, length];

            for (int i = 0; i < MountainCount; i++)
            {
                var x = Random.Range(0, width);
                var y = Random.Range(0, length);
                var sizeX = Random.Range(MountainSizeMin, MountainSizeMax) / 2;
                var sizeY = Random.Range(MountainSizeMin, MountainSizeMax) / 2;
                
                CreateMountain(x, sizeX, y, sizeY, data);
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

            MapData generated;
            Data = generated = new MapData(width, length, data, objs);

            var relativeCrystalPos = CentralBuilding.BuildingSize + Vector2.one * 2;
            var size = relativeCrystalPos + Vector2.one * 3;
            List<Vector2> possibleBasePositions = new List<Vector2>();
            CollectPossibleBasePositions(Data, size, possibleBasePositions);
            
            List<Vector2> basePositions;
            while (!CreateBasePositions(possibleBasePositions, GameUtils.MaxPlayers, Math.Min(width, length) / 2.5f, out basePositions)) ;
            
            foreach (var basePosition in basePositions)
                mFreeBases.Add(basePosition);

            foreach (var basePosition in basePositions)
            {
                var crystalPos = basePosition + relativeCrystalPos;
                generated.mObjects[(int) crystalPos.x, (int) crystalPos.y] = MapObject.Crystal;
            }
        }

        private void CollectPossibleBasePositions(IMapData mapData, Vector2 squareSize, List<Vector2> possiblePositions)
        {
            for (int x = 0; x < mapData.Width - squareSize.x; x++)
            {
                for (int y = 0; y < mapData.Length - squareSize.y; y++)
                {
                    float hSum = 0f;
                    bool posOK = true;
                    for (int xx = 0; xx < squareSize.x && posOK; xx++)
                    {
                        for (int yy = 0; yy < squareSize.y; yy++)
                        {
                            if (mapData.GetMapObjectAt(x + xx, y + yy) != MapObject.None)
                            {
                                posOK = false;
                                break;
                            }

                            hSum += mapData.GetHeightAt(x + xx, y + yy);
                        }
                    }
                    
                    if (!posOK)
                        continue;
                    
                    if (Math.Abs(hSum / squareSize.x / squareSize.y - mapData.GetHeightAt(x, y)) > 0.01)
                        continue;
                    
                    possiblePositions.Add(new Vector2(x, y));
                }
            }
        }

        private bool CreateBasePositions(List<Vector2> possiblePositions, int depth, float baseDistance, out List<Vector2> allocatedPositions)
        {
            allocatedPositions = new List<Vector2>();

            if (depth <= 0)
            {
                return true;
            }

            if (possiblePositions.Count < 1)
                return false;

            var pos = possiblePositions[Random.Range(0, possiblePositions.Count)];

            var subPossiblePositions = possiblePositions.Where(p => Vector2.Distance(p, pos) >= baseDistance).ToList();
            List<Vector2> subAllocated = null;

            var createOK = false;
            for (int i = 0; i < BasePlacementTryes; i++)
            {
                if (CreateBasePositions(subPossiblePositions, depth - 1, baseDistance, out subAllocated))
                {
                    createOK = true;
                    break;
                }
            }

            if (!createOK)
                return false;
            
            subAllocated.Add(pos);
            allocatedPositions.AddRange(subAllocated);
            return true;
        }

        public bool TryAllocateBase(out Vector2 basePosition) 
            => mFreeBases.TryTake(out basePosition);

        void CreateMountain(int x, int sizeX, int y, int sizeY, float[,] heights)
        {
            for (int x1 = x - sizeX; x1 <= x + sizeX; x1++)
            {
                for (int y1 = y - sizeY; y1 <= y + sizeY; y1++)
                {
                    if (x1 < 0 || y1 < 0 || x1 >= heights.GetLength(0) || y1 >= heights.GetLength(1))
                        continue;

                    var h = 1f;
                    if ((x1 == x - sizeX || x1 == x + sizeX) &&
                        (y1 == y - sizeY || y1 == y + sizeY))
                        h = 0.5f;

                    heights[x1, y1] = h;
                }
            }
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