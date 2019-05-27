using System;
using Newtonsoft.Json;

namespace Assets.Networking.ServerClientPackages
{
    class ObjectCreatedPackage : ServerClientPackage
    {
        [JsonIgnore]
        public override ServerClientPackageType PackageType { get; } = ServerClientPackageType.ObjectAdded;

        [JsonProperty]
        public float X { get; private set; }
        [JsonProperty]
        public float Y { get; private set; }

        [JsonProperty]
        public string ObjectType { get; private set; }

        [JsonConstructor]
        private ObjectCreatedPackage()
            : base(Guid.Empty)
        {
            
        }

        public ObjectCreatedPackage(Guid objectID, float x, float y, string objectType)
            : base(objectID)
        {
            X = x;
            Y = y;
            ObjectType = objectType;
        }
    }
}