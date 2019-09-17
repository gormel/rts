using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Map;
using UnityEngine;

namespace Assets.Views.Utils
{
    class Waypoint : MonoBehaviour, IPathFinder
    {
        public event Action Arrived;
        public bool IsArrived { get; } = true;
        public Vector2 CurrentPosition { get; }
        public Vector2 CurrentDirection { get; }
        public Task SetTarget(Vector2 position, IMapData mapData)
        {
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            return Task.CompletedTask;
        }
    }
}
