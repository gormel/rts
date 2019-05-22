using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using Assets.Networking.ServerClientPackages;
using Assets.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Networking
{
    class NetworkServer
    {
        private TcpListener mTcpListener;
        private ConcurrentDictionary<TcpClient, TcpClient> mClients = new ConcurrentDictionary<TcpClient, TcpClient>();
        private ConcurrentDictionary<Guid, IPackageProcessor> mPackageProcessors = new ConcurrentDictionary<Guid, IPackageProcessor>();

        public event Action<TcpClient> OnConnected;

        public NetworkServer(int port)
        {
            mTcpListener = new TcpListener(IPAddress.Any, port);
            mTcpListener.Start();
            AcceptClients();
        }

        private async void AcceptClients()
        {
            while (true)
            {
                ProcessClient(await mTcpListener.AcceptTcpClientAsync());
            }
        }

        private async void ProcessClient(TcpClient client)
        {
            try
            {
                while (!mClients.TryAdd(client, client))
                    if (mClients.ContainsKey(client))
                        break;

                OnConnected?.Invoke(client);

                using (var streamReader = new StreamReader(client.GetStream()))
                using (var reader = new JsonTextReader(streamReader))
                {
                    while (client.Connected)
                    {
                        var token = await JObject.LoadAsync(reader);

                        var id = token["ID"].ToObject<Guid>();
                        var data = token.Property("Data").Value<JObject>();

                        IPackageProcessor processor;
                        if (mPackageProcessors.TryGetValue(id, out processor))
                            processor.Process(data);
                    }
                }
            }
            finally
            {
                TcpClient notUsed;
                while (!mClients.TryRemove(client, out notUsed))
                    if (!mClients.ContainsKey(client))
                        break;
            }
        }
        
        public async Task ObjectCreated<TOrder, TInfo>(TOrder orders, TInfo info, IPackageProcessor packageProcessor)
            where TOrder : IGameObjectOrders
            where TInfo : IGameObjectInfo
        {
            while (!mPackageProcessors.TryAdd(info.ID, packageProcessor))
                if (mPackageProcessors.ContainsKey(info.ID))
                    break;

            foreach (var client in mClients.Values)
            {
                using (var textWriter = new StreamWriter(client.GetStream()))
                using (var jsonWriter = new JsonTextWriter(textWriter))
                {
                    await JToken.FromObject(new ObjectCreatedPackage(info.ID, info.Position.x, info.Position.y, nameof(TOrder) + nameof(TInfo)), JsonUtils.Serializer).WriteToAsync(jsonWriter);
                }
            }
        }

        public void Update()
        {
            foreach (var client in mClients.Values)
            {
                using (var textWriter = new StreamWriter(client.GetStream()))
                using (var jsonWriter = new JsonTextWriter(textWriter))
                {
                    foreach (var processor in mPackageProcessors)
                    {
                        processor.Value.SendState(jsonWriter);
                    }
                }
            }
        }
    }
}