using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Core.Map
{
    interface IPathFinder
    {
        event Action Arrived;
        bool IsArrived { get; }

        Vector2 CurrentPosition { get; }
        Vector2 CurrentDirection { get; }

        Vector2 Target { get; }

        Task SetTarget(Vector2 position, IMapData mapData);
        Task Stop();
    }
}