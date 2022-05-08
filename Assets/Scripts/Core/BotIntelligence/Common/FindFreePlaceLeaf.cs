using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.BehaviorTree;
using Assets.Core.Game;
using Assets.Core.GameObjects.Final;
using Assets.Core.Map;
using Core.BotIntelligence.Memory;
using UnityEngine;

namespace Core.BotIntelligence.Common
{
    class FindFreePlaceLeaf : IBTreeLeaf
    {
        private readonly Game mGame;
        private readonly IMapData mMapdata;
        private readonly BuildingFastMemory mFastMemory;
        private readonly Vector2 mBuildingSize;
        private readonly int mBorder;
        private readonly List<Vector2Int> mFoundPlaces = new();

        public FindFreePlaceLeaf(Game game, IMapData mapdata, BuildingFastMemory fastMemory, Vector2 buildingSize, int border)
        {
            mGame = game;
            mMapdata = mapdata;
            mFastMemory = fastMemory;
            mBuildingSize = buildingSize;
            mBorder = border;
        }

        private RectInt GetSearchArea(Vector2Int center, int size)
        {
            var halfSize = new Vector2Int(size, size);
            return new RectInt(center - halfSize, halfSize * 2);
        }

        private bool TryFindPlace(RectInt searchArea, out Vector2Int foundPosition)
        {
            mFoundPlaces.Clear();
            for (int x = 0; x < searchArea.width - mBuildingSize.x; x++)
            {
                for (int y = 0; y < searchArea.height - mBuildingSize.y; y++)
                {
                    foundPosition = searchArea.position + new Vector2Int(x, y);
                    if (CheckPosition(foundPosition))
                        mFoundPlaces.Add(foundPosition);
                }
            }

            if (mFoundPlaces.Count > 0)
            {
                foundPosition = mFoundPlaces.OrderBy(p => Vector2.Distance(p, searchArea.center)).First();
                return true;
            }
            
            foundPosition = Vector2Int.zero;
            return false;
        }

        protected virtual bool CheckPosition(Vector2Int position)
            => mGame.GetIsAreaFreeNoAlloc(position - new Vector2(mBorder, mBorder), mBuildingSize + new Vector2(mBorder, mBorder) * 2);

        public BTreeLeafState Update(TimeSpan deltaTime)
        {
            if (mFastMemory.FreeWorker == null)
                return BTreeLeafState.Failed;
            
            var workerPos = mFastMemory.FreeWorker.Position;
            var areaCenter = new Vector2Int(Mathf.FloorToInt(workerPos.x), Mathf.FloorToInt(workerPos.y));
            for (int i = 1; i < 5; i++)
            {
                var area = GetSearchArea(areaCenter, 5 * i);
                if (TryFindPlace(area, out var foundPosition))
                {
                    mFastMemory.Place = foundPosition;
                    return BTreeLeafState.Successed;
                }
            }
            return BTreeLeafState.Failed;
        }
    }

    class FindFreeMiningCampPlaceLeaf : FindFreePlaceLeaf
    {
        private readonly IMapData mMapdata;

        public FindFreeMiningCampPlaceLeaf(Game game, IMapData mapdata, BuildingFastMemory fastMemory, Vector2 buildingSize) 
            : base(game, mapdata, fastMemory, buildingSize, 0)
        {
            mMapdata = mapdata;
        }

        protected override bool CheckPosition(Vector2Int position)
        {
            if (!base.CheckPosition(position))
                return false;

            return MiningCamp.CheckPlaceAllowed(mMapdata, position);
        }
    }
}