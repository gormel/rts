using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Core.Map
{
    interface IPathFinderBase
    {
        Vector2 CurrentPosition { get; }
        bool IsArrived { get; }
        Vector2 Target { get; }
    }
    interface IPathFinder : IPathFinderBase
    {
        event Action Arrived;

        Vector2 CurrentDirection { get; }


        Task SetLookAt(Vector2 position, IMapData mapData);
        Task SetTarget(Vector2 position, IMapData mapData);
        Task Stop();
    }
}