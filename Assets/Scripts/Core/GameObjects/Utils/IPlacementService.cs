using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Core.GameObjects.Utils
{
    interface IPlacementService
    {
        /// <summary>
        /// Trys allocate new placement point, returns <code>PlacementPoint.Invalid</code> if fails.
        /// </summary>
        /// <returns>Allocated point.</returns>
        Task<PlacementPoint> TryAllocatePoint();

        Task<PlacementPoint> TryAllocateNearestPoint(Vector2 toPoint);

        /// <summary>
        /// Releases allocated point.
        /// </summary>
        /// <param name="pointId">Allocated point id.</param>
        /// <returns>true, if point succesfully released, otherwise - false.</returns>
        Task<bool> ReleasePoint(int pointId);
    }
}