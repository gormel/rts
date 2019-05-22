using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Networking.ServerClientPackages
{
    abstract class ObjectUpdatedPackage : ServerClientPackage
    {
    }

    class ObjectCreatedPackage : ServerClientPackage
    {
        public override ServerClientPackageType PackageType { get; } = ServerClientPackageType.ObjectAdded;

        public float X { get; }
        public float Y { get; }

        public string ObjectType { get; }

        public ObjectCreatedPackage(Guid objectID, float x, float y, string objectType)
            : base(objectID)
        {
            X = x;
            Y = y;
            ObjectType = objectType;
        }
    }

    enum ServerClientPackageType
    {
        ObjectAdded,
        ObjectUpdated,
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
