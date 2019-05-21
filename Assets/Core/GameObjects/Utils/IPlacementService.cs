namespace Assets.Core.GameObjects.Utils
{
    interface IPlacementService
    {
        bool TryAllocatePoint(out PlacementPoint point);
        void ReleasePoint(int pointId);
    }
}