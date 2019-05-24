using System;
using Assets.Core.Map;

namespace Assets.Networking.ServerClientPackages
{
    class LoadMapDataPackage : ServerClientPackage
    {
        public override ServerClientPackageType PackageType => ServerClientPackageType.LoadMap;

        public float[] Heights { get; }
        public int Width { get; }
        public int Length { get; }

        public LoadMapDataPackage(IMapData mapData)
            : base(Guid.Empty)
        {
            Width = mapData.Width;
            Length = mapData.Length;

            Heights = new float[Width * Length];
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Length; j++)
                {
                    Heights[i * Length + j] = mapData.GetHeightAt(i, j);
                }
            }
        }
    }
}