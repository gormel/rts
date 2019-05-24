using System.Collections.ObjectModel;
using System.Net;
using UnityEngine;

namespace Assets.Networking
{
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
