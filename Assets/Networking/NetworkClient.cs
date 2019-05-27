using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Assets.Core.GameObjects.Base;
using Assets.Networking.ServerClientPackages;
using Assets.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Assets.Networking
{
    interface IClientOrdersFactory
    {
        IGameObjectOrders CreateOrders(Guid id);
        event Action<Guid, JObject> SendOrder;
    }

    interface IClientInfoFactory
    {
        IGameObjectInfo CreateInfo(Guid id);
        IClientInfoUpdater CreateUpdater(IGameObjectInfo info);
    }

    interface IClientInfoUpdater
    {
        void Update(ServerClientPackage package);
    }

    delegate void ObjectCreatedHandler<TOrders, TInfo>(TOrders orders, TInfo info, float x, float y)
        where TOrders : IGameObjectOrders
        where TInfo : IGameObjectInfo;

    class NetworkClient
    {
        private readonly IPAddress mIp;
        private readonly int mPort;

        private struct Subscription
        {
            public IClientOrdersFactory ClientOrdersFactory { get; }
            public IClientInfoFactory ClientInfoFactory { get; }
            public event ObjectCreatedHandler<IGameObjectOrders, IGameObjectInfo> Created;

            public Subscription(IClientOrdersFactory clientOrdersFactory, IClientInfoFactory clientInfoFactory) : this()
            {
                ClientOrdersFactory = clientOrdersFactory;
                ClientInfoFactory = clientInfoFactory;
            }

            public IGameObjectInfo Invoke(Guid id, float x, float y)
            {
                var info = ClientInfoFactory.CreateInfo(id);
                Created?.Invoke(ClientOrdersFactory.CreateOrders(id), info, x, y);
                return info;
            }
        }

        private TcpClient mClient;
        public event Action<LoadMapDataPackage> LoadMapData;
        private Dictionary<string, Subscription> mSubscriptions = new Dictionary<string, Subscription>();
        private Dictionary<Guid, IClientInfoUpdater> mInfoUpdaters = new Dictionary<Guid, IClientInfoUpdater>();

        public NetworkClient(IPAddress ip, int port)
        {
            mIp = ip;
            mPort = port;
        }

        public void Connect()
        {
            InitClient(mClient = new TcpClient(), mIp, mPort);
        }

        private async void InitClient(TcpClient client, IPAddress ip, int port)
        {
            try
            {
                await client.ConnectAsync(ip, port);

                using (var streamReader = new StreamReader(client.GetStream()))
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    while (client.Connected)
                    {
                        var objToken = await JsonUtils.LoadJObjectAsync(jsonReader);
                        using (var tokenReader = new JTokenReader(objToken))
                        {
                            var package = JsonUtils.Serializer.Deserialize(tokenReader) as ServerClientPackage;
                            switch (package.PackageType)
                            {
                                case ServerClientPackageType.ObjectAdded:
                                    var objAddedPack = package as ObjectCreatedPackage;
                                    Subscription sub;
                                    if (mSubscriptions.TryGetValue(objAddedPack.ObjectType, out sub))
                                    {
                                        var info = sub.Invoke(package.ObjectID, objAddedPack.X, objAddedPack.Y);
                                        var updater = sub.ClientInfoFactory.CreateUpdater(info);
                                        mInfoUpdaters[package.ObjectID] = updater;
                                    }
                                    break;
                                case ServerClientPackageType.ObjectUpdated:
                                    IClientInfoUpdater updater1;
                                    if (mInfoUpdaters.TryGetValue(package.ObjectID, out updater1))
                                        updater1.Update(package);
                                    break;
                                case ServerClientPackageType.LoadMap:
                                    LoadMapData?.Invoke(package as LoadMapDataPackage);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                    }
                }
            }
            catch (SocketException e)
            {
            }
            catch (JsonReaderException e)
            {
                e.ToString();
            }
        }

        public void ListenObjectType<TOrders, TInfo>(ObjectCreatedHandler<TOrders, TInfo> onCreate, IClientOrdersFactory clientOrdersFactory, IClientInfoFactory clientInfoFactory)
            where TOrders : IGameObjectOrders
            where TInfo : IGameObjectInfo
        {
            
            var key = typeof(TOrders).Name + typeof(TInfo).Name;
            if (mSubscriptions.ContainsKey(key))
                throw new ArgumentException("Already subscribed.");

            var sub = new Subscription(clientOrdersFactory, clientInfoFactory);
            sub.Created += (orders, info, x, y) => onCreate((TOrders) orders, (TInfo) info, x, y);
            mSubscriptions[key] = sub;

            clientOrdersFactory.SendOrder += OrdersFactoryOnSendOrder;
        }

        private void OrdersFactoryOnSendOrder(Guid id, JObject obj)
        {
            var pack = new JObject();
            pack["ID"] = JToken.FromObject(id, JsonUtils.Serializer);
            pack["Data"] = obj;

            using (var streamWriter = new StreamWriter(mClient.GetStream()))
            using (var jsonWriter = new JsonTextWriter(streamWriter))
            {
                pack.WriteTo(jsonWriter);
            }
        }

        public void Update() {}
    }
}