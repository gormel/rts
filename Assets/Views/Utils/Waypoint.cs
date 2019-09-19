using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Map;
using Assets.Utils;
using UnityEngine;

namespace Assets.Views.Utils
{
    class Waypoint : MonoBehaviour, IPathFinder
    {
        public event Action Arrived;
        public bool IsArrived { get; } = true;
        public Vector2 CurrentPosition => GameUtils.GetFlatPosition(transform.localPosition);
        public Vector2 CurrentDirection { get; } = Vector2.zero;
        public Vector2 Target => GameUtils.GetFlatPosition(transform.localPosition);

        public Task LookAt(Vector2 position, IMapData mapData)
        {
            return Task.CompletedTask;
        }

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
