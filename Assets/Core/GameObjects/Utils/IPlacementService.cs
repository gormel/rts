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
        /// <returns>true, if point succesfully released, otherwise - false.</returns>
        Task<bool> ReleasePoint(int pointId);
    }
}