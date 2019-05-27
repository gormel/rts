using System;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using Assets.Networking.ServerClientPackages;
using Assets.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Networking.NetworkCustoms
{
    enum BuildingTemplateOrder
    {
        Cancel
    }

    class BuildingTemplateStatePackage : ServerClientPackage, IBuildingTemplateInfo
    {
        [JsonProperty]
        public Guid ID { get; set; }
        [JsonProperty]
        public Vector2 Position { get; private set; }
        [JsonProperty]
        public float Health { get; private set; }
        [JsonProperty]
        public float MaxHealth { get; private set; }
        [JsonProperty]
        public Vector2 Size { get; private set; }
        [JsonProperty]
        public Vector2 Waypoint { get; private set; }
        [JsonProperty]
        public float Progress { get; private set; }
        [JsonProperty]
        public int AttachedWorkers { get; private set; }

        [JsonIgnore]
        public override ServerClientPackageType PackageType => ServerClientPackageType.ObjectUpdated;

        [JsonConstructor]
        private BuildingTemplateStatePackage()
            : base(Guid.Empty)
        {
            
        }

        public BuildingTemplateStatePackage(IBuildingTemplateInfo info)
            : base(info.ID)
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

    class BuildingTemplateClientOrdersFactory : IClientOrdersFactory
    {
        private class Orders : IBuildingTemplateOrders
        {
            private readonly Guid mID;
            public event Action<Guid, JObject> SendOredr;
            public Orders(Guid id)
            {
                mID = id;
            }

            public async Task Cancel()
            {
                SendOredr?.Invoke(mID, JObject.FromObject(BuildingTemplateOrder.Cancel, JsonUtils.Serializer));
            }
        }

        public IGameObjectOrders CreateOrders(Guid id)
        {
            var result = new Orders(id);
            result.SendOredr += (guid, o) => SendOrder?.Invoke(guid, o);
            return result;
        }

        public event Action<Guid, JObject> SendOrder;
    }

    class BuildingTemplateClientInfoFactory : IClientInfoFactory
    {
        private class Info : IBuildingTemplateInfo
        {
            public Guid ID { get; set; }
            public Vector2 Position { get; set; }
            public float Health { get; set; }
            public float MaxHealth { get; set; }
            public Vector2 Size { get; set; }
            public Vector2 Waypoint { get; set; }
            public float Progress { get; set; }
            public int AttachedWorkers { get; set; }
        }

        private class Updater : IClientInfoUpdater
        {
            private readonly Info mInfo;

            public Updater(Info info)
            {
                mInfo = info;
            }
            public void Update(ServerClientPackage package)
            {
                var typed = package as BuildingTemplateStatePackage;
                if (typed == null)
                    return;

                mInfo.Position = typed.Position;
                mInfo.AttachedWorkers = typed.AttachedWorkers;
                mInfo.Health = typed.Health;
                mInfo.MaxHealth = typed.MaxHealth;
                mInfo.Progress = typed.Progress;
                mInfo.Size = typed.Size;
                mInfo.Waypoint = typed.Waypoint;
            }
        }

        public IGameObjectInfo CreateInfo(Guid id)
        {
            var result = new Info();
            result.ID = id;
            return result;
        }

        public IClientInfoUpdater CreateUpdater(IGameObjectInfo info)
        {
            return new Updater((Info)info);
        }
    }

    class BuildingTemplateServerPackageProcessor : IServerPackageProcessor
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
            var des = (BuildingTemplateOrder)JsonUtils.Serializer.Deserialize(new JTokenReader(data));
            if (des == BuildingTemplateOrder.Cancel)
                mOrders.Cancel();
        }

        public void SendState(JsonTextWriter writer)
        {
            JsonUtils.Serializer.Serialize(writer, new BuildingTemplateStatePackage(mInfo));
        }
    }
}