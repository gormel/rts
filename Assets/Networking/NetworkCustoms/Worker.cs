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
    class WorkerGoToOrder
    {
        [JsonProperty]
        public float X { get; private set; }
        [JsonProperty]
        public float Y { get; private set; }

        [JsonConstructor]
        private WorkerGoToOrder()
        {
        }

        public WorkerGoToOrder(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    class WorkerInfoPackage : ServerClientPackage, IWorkerInfo
    {
        [JsonIgnore]
        public override ServerClientPackageType PackageType => ServerClientPackageType.ObjectUpdated;

        [JsonProperty]
        public Guid ID { get; set; }
        [JsonProperty]
        public Vector2 Position { get; set; }
        [JsonProperty]
        public float Health { get; set; }
        [JsonProperty]
        public float MaxHealth { get; set; }
        [JsonProperty]
        public float Speed { get; set; }
        [JsonProperty]
        public Vector2 Direction { get; set; }
        [JsonProperty]
        public Vector2 Destignation { get; set; }

        [JsonConstructor]
        private WorkerInfoPackage()
            : base(Guid.Empty)
        {
        }

        public WorkerInfoPackage(IWorkerInfo info)
            : base(info.ID)
        {
            ID = info.ID;
            Position = info.Position;
            Health = info.Health;
            MaxHealth = info.MaxHealth;
            Speed = info.Speed;
            Direction = info.Direction;
            Destignation = info.Destignation;
        }
    }

    class WorkerClientInfoFactory : IClientInfoFactory
    {
        private class Info : IWorkerInfo
        {
            public Info(Guid id)
            {
                ID = id;
            }

            public Guid ID { get; set; }
            public Vector2 Position { get; set; }
            public float Health { get; set; }
            public float MaxHealth { get; set; }
            public float Speed { get; set; }
            public Vector2 Direction { get; set; }
            public Vector2 Destignation { get; set; }
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
                var typed = package as WorkerInfoPackage;
                if (typed == null)
                    return;

                mInfo.Position = typed.Position;
                mInfo.Direction = typed.Direction;
                mInfo.Health = typed.Health;
                mInfo.MaxHealth = typed.MaxHealth;
                mInfo.Speed = typed.Speed;
                mInfo.Destignation = typed.Destignation;
            }
        }

        public IGameObjectInfo CreateInfo(Guid id)
        {
            return new Info(id);
        }

        public IClientInfoUpdater CreateUpdater(IGameObjectInfo info)
        {
            return new Updater((Info)info);
        }
    }

    class WorkerClientOrderFactory : IClientOrdersFactory
    {
        private class Orders : IWorkerOrders
        {
            private readonly Guid mID;
            public event Action<Guid, JObject> SendOrder;
            public Orders(Guid id)
            {
                mID = id;
            }
            public async Task GoTo(Vector2 position)
            {
                SendOrder?.Invoke(mID, JObject.FromObject(new WorkerGoToOrder(position.x, position.y), JsonUtils.Serializer));
            }

            public Task<Guid> PlaceCentralBuildingTemplate(Vector2Int position)
            {
                throw new NotImplementedException();
            }

            public Task AttachAsBuilder(Guid templateId)
            {
                throw new NotImplementedException();
            }
        }

        public IGameObjectOrders CreateOrders(Guid id)
        {
            var result = new Orders(id);
            result.SendOrder += (guid, o) => SendOrder?.Invoke(guid, o);
            return result;
        }

        public event Action<Guid, JObject> SendOrder;
    }

    class WorkerServerPackageProcessor : IServerPackageProcessor
    {
        private IWorkerOrders mOrders;
        private IWorkerInfo mInfo;

        public WorkerServerPackageProcessor(IWorkerOrders orders, IWorkerInfo info)
        {
            mOrders = orders;
            mInfo = info;
        }

        public void Process(JObject data)
        {
            var objData = JsonUtils.Serializer.Deserialize(new JTokenReader(data));
            if (objData is WorkerGoToOrder)
            {
                var order = (WorkerGoToOrder) objData;
                mOrders.GoTo(new Vector2(order.X, order.Y));
            }
        }

        public void SendState(JsonTextWriter writer)
        {
            JObject.FromObject(new WorkerInfoPackage(mInfo), JsonUtils.Serializer).WriteTo(writer);
        }
    }
}
