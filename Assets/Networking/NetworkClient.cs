using System;
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
    interface IOrdersProcessor
    {
        IGameObjectOrders Oredrs { get; }
    }

    interface IInfoProcessor
    {
        IGameObjectInfo Info { get; }
    }

    class NetworkClient
    {
        private TcpClient mClient;
        public event Action<LoadMapDataPackage> LoadMapData;

        public NetworkClient(IPAddress ip, int port)
        {
            InitClient(mClient = new TcpClient(), ip, port);
        }

        private async void InitClient(TcpClient client, IPAddress ip, int port)
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
                                //create orders
                                //create info
                                //call callback
                                break;
                            case ServerClientPackageType.ObjectUpdated:
                                //update info
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

        public void ListenObjectType<TOrders, TInfo>(Action<TOrders, TInfo> onCreate, IOrdersProcessor ordersProcessor, IInfoProcessor infoProcessor)
            where TOrders : IGameObjectOrders
            where TInfo : IGameObjectInfo
        {
            //register callback
        }

        public void Update() {}
    }
}