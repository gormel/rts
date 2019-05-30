using UnityEngine;

namespace Assets.Core.Map
{
    interface IMapData
    {
        int Length { get; }
        int Width { get; }

        float GetHeightAt(int x, int y);
    }
}