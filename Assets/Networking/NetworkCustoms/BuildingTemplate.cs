using System;
using Assets.Core.GameObjects.Final;
using Assets.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Networking.NetworkCustoms
{
    class CancelBuildingTemplateData { }

    class BuildingTemplateStatePackage : IBuildingTemplateInfo
    {
        public Guid ID { get; set; }
        public Vector2 Position { get; private set; }
        public float Health { get; private set; }
        public float MaxHealth { get; private set; }
        public Vector2 Size { get; private set; }
        public Vector2 Waypoint { get; private set; }
        public float Progress { get; private set; }
        public int AttachedWorkers { get; private set; }

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

    class BuildingTemplateServerPackageProcessor : IPackageProcessor
    {
        private readonly IBuildingTemplateOrders mOrders;
        private readonly IBuildingTemplateInfo mInfo;

        public BuildingTemplateServerPackageProcessor(IBuildingTemplateOrders orders, IBuildingTemplateInfo info)
        {
            mOrders = orders;
            mInfo = info;
        }

        public void Process(JObject data)
        {
            var des = JsonUtils.Serializer.Deserialize(new JTokenReader(data));
            if (des is CancelBuildingTemplateData)
                mOrders.Cancel();
        }

        public void SendState(JsonTextWriter writer)
        {
            JsonUtils.Serializer.Serialize(writer, new BuildingTemplateStatePackage(mInfo));
        }
    }
}