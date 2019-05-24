using System;

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
        public Guid ObjectID { get; }
        public abstract ServerClientPackageType PackageType { get; }

        public ServerClientPackage(Guid objectID)
        {
            ObjectID = objectID;
        }
    }
}
