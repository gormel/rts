using UnityEngine;

namespace Assets.Core.Map
{
    enum MapObject
    {
        None,
        Tree,
        Crystal
    }

    interface IMapData
    {
        int Length { get; }
        int Width { get; }

        float GetHeightAt(int x, int y);
        MapObject GetMapObjectAt(int x, int y);
    }
}