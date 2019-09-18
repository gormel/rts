using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Assets.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Assets.Interaction
{
    class StartGui : MonoBehaviour
    {
        public string GameSceneName;//нужен нормальный эдитор

        public InputField HostPortField;
        public InputField ConnectIPField;
        public InputField ConnectPortField;

        void Start()
        {
            HostPortField.text = "15656";
            ConnectIPField.text = "127.0.0.1";
            ConnectPortField.text = "15656";
        }

        public void Host()
        {
            int port;
            if (!int.TryParse(ConnectPortField.text, out port))
                return;

            GameUtils.IP = IPAddress.Any;
            GameUtils.Port = port;
            GameUtils.CurrentMode = GameMode.Server;
            SceneManager.LoadScene(GameSceneName);
        }

        public void Connect()
        {
            IPAddress ip;
            if (!IPAddress.TryParse(ConnectIPField.text, out ip))
                return;

            int port;
            if (!int.TryParse(ConnectPortField.text, out port))
                return;

            GameUtils.IP = ip;
            GameUtils.Port = port;
            GameUtils.CurrentMode = GameMode.Client;
            SceneManager.LoadScene(GameSceneName);
        }
    }
}
