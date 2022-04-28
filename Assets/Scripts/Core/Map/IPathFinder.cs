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
        bool Initialized { get; }
        Vector2 CurrentDirection { get; }
        
        bool InProgress { get; }

        Task Initialize(Vector2 position, Vector2 destignation, IMapData mapData);

        Task SetLookAt(Vector2 position, IMapData mapData);
        Task SetTarget(Vector2 position, IMapData mapData);
        Task Stop();
        Task Teleport(Vector2 position, IMapData mapData);
    }
}