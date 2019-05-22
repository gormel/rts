using System;
using Assets.Core.GameObjects.Final;
using Assets.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Networking
{
    class CancelBuildingTemplatePackage { }

    class BuildingTemplateStatePackage : IBuildingTemplateInfo
    {
        public Guid ID { get; set; }
        public Vector2 Position { get; }
        public float Health { get; }
        public float MaxHealth { get; }
        public Vector2 Size { get; }
        public Vector2 Waypoint { get; }
        public float Progress { get; }
        public int AttachedWorkers { get; }

        public BuildingTemplateStatePackage(IBuildingTemplateInfo info)
        {
            ID = info.ID;
            Position = info.Position;
            Health = info.Health;
            MaxHealth = info.MaxHealth;
            Size = info.Size;
            Waypoint = info.Waypoint;
            Progress = info.Progress;
            AttachedWorkers = info.AttachedWorkers;
        }
    }
    class BuildingTemplatePackageProcessor : IPackageProcessor
    {
        private readonly IBuildingTemplateInfo mInfo;

        public BuildingTemplatePackageProcessor(IBuildingTemplateOrders orders, IBuildingTemplateInfo info)
        {
            mInfo = info;
        }

        public void Process(JObject package)
        {
        }

        public void SendState(JsonTextWriter writer)
        {
            JsonUtils.Serializer.Serialize(writer, new BuildingTemplateStatePackage(mInfo));
        }
    }
}