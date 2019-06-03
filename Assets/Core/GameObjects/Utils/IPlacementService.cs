using System.Threading.Tasks;

namespace Assets.Core.GameObjects.Utils
{
    interface IPlacementService
    {
        /// <summary>
        /// Trys allocate new placement point, returns <code>PlacementPoint.Invalid</code> if fails.
        /// </summary>
        /// <returns>Allocated point.</returns>
        Task<PlacementPoint> TryAllocatePoint();

        /// <summary>
        /// Releases allocated point.
        /// </summary>
        /// <param name="pointId">Allocated point id.</param>
        Task ReleasePoint(int pointId);
    }
}