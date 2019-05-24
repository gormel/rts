using System;

namespace Assets.Networking.ServerClientPackages
{
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
}