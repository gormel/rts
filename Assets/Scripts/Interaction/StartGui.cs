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
        public string LobbySceneName;//нужен нормальный эдитор

        public InputField ConnectIPField;
        public InputField NicknameField;

        void Start()
        {
            ConnectIPField.text = GameUtils.IP.ToString();
            NicknameField.text = GameUtils.Nickname;
        }

        public void Host()
        {
            GameUtils.CurrentMode = GameMode.Server;
            if (!string.IsNullOrEmpty(NicknameField.text))
                GameUtils.Nickname = NicknameField.text;

            SceneManager.LoadScene(LobbySceneName);
        }

        public void Connect()
        {
            IPAddress ip;
            if (!IPAddress.TryParse(ConnectIPField.text, out ip))
                return;

            GameUtils.IP = ip;
            GameUtils.CurrentMode = GameMode.Client;
            if (!string.IsNullOrEmpty(NicknameField.text))
                GameUtils.Nickname = NicknameField.text;

            SceneManager.LoadScene(LobbySceneName);
        }

        void OnDestroy()
        {
            GameUtils.SaveSettings();
        }

        public void Exit()
        {
            Application.Quit();
        }
    }
}
