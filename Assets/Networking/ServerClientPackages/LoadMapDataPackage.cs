using System;
using Assets.Core.Map;
using Newtonsoft.Json;

namespace Assets.Networking.ServerClientPackages
{
    class LoadMapDataPackage : ServerClientPackage
    {
        [JsonIgnore]
        public override ServerClientPackageType PackageType => ServerClientPackageType.LoadMap;

        [JsonProperty]
        public float[] Heights { get; private set; }
        [JsonProperty]
        public int Width { get; private set; }
        [JsonProperty]
        public int Length { get; private set; }

        [JsonConstructor]
        private LoadMapDataPackage()
            : base(Guid.Empty)
        {
        }

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