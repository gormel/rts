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
    class Waypoint : MonoBehaviour, IPathFinderBase
    {
        public Vector2 CurrentPosition => GameUtils.GetFlatPosition(transform.localPosition);
        public bool IsArrived => true;
        public Vector2 Target => CurrentPosition;
    }
}
