using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Core.Map
{
    interface IPathFinder
    {
        event Action Arrived;

        Vector2 CurrentPosition { get; }
        Vector2 CurrentDirection { get; }

        Task SetTarget(Vector2 position, IMapData mapData);

        Task Stop();
    }
}