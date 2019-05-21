using UnityEngine;

namespace Assets.Core.Map
{
    interface IPathFinder
    {
        bool Active { get; }
        Vector2 CurrentPosition { get; }
        Vector2 CurrentDirection { get; }

        void SetTarget(Vector2 position, Map map);

        void Stop();
    }
}