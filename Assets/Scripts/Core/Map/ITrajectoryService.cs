using UnityEngine;

namespace Assets.Core.Map
{
    interface ITrajectoryService
    {
        float GetTrajectoryLength(Vector2 from, Vector2 to);
    }
}