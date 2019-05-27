using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Interaction
{
    class StartGui : MonoBehaviour
    {
        public string GameSceneName;//нужен нормальный эдитор
        public void Host()
        {
            GameUtils.CurrentMode = GameMode.Server;
            SceneManager.LoadScene(GameSceneName);
        }

        public void Connect()
        {
            GameUtils.CurrentMode = GameMode.Client;
            SceneManager.LoadScene(GameSceneName);
        }
    }
}
