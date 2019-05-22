using System;
using System.Collections.ObjectModel;
using System.Net;
using Assets.Core.GameObjects.Base;
using UnityEngine;

namespace Assets.Networking
{
    class NetworkClient
    {
        public NetworkClient(IPAddress ip, int port)
        {
            
        }

        public void ListenCreations<TOrders, TInfo>(Action<TOrders, TInfo> onCreate)
            where TOrders : IGameObjectOrders
            where TInfo : IGameObjectInfo
        {
            
        }

        public void Update()
        {
            
        }
    }

    class NetworkManager : MonoBehaviour
    {
        public NetworkClient Client { get; private set; }
        public NetworkServer Server { get; private set; }

        public void Listen(int port)
        {
            Server = new NetworkServer(port);
        }

        public void Connect(IPAddress ip, int port)
        {
            Client = new NetworkClient(ip, port);
        }

        void Update()
        {
            Client?.Update();
            Server?.Update();
        }
    }
}
