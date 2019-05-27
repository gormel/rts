using System;
using Newtonsoft.Json;

namespace Assets.Networking.ServerClientPackages
{
    enum ServerClientPackageType
    {
        ObjectAdded,
        ObjectUpdated,
        LoadMap
    }

    abstract class ServerClientPackage
    {
        [JsonProperty]
        public Guid ObjectID { get; private set; }

        [JsonIgnore]
        public abstract ServerClientPackageType PackageType { get; }

        public ServerClientPackage(Guid objectID)
        {
            ObjectID = objectID;
        }
    }
}
