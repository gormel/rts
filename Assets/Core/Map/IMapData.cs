using UnityEngine;

namespace Assets.Core.Map
{
    interface IMapData
    {
        int Length { get; }
        int Width { get; }

        float GetHeightAt(Vector2 position);
        bool GetIsAreaFree(Vector2 position, Vector2 size);
    }
}