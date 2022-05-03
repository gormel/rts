using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using UnityEngine;

namespace Assets.Interaction
{
    class GameEndInterface : MonoBehaviour
    {
        public UserInterface Interface;
        public GameObject InterfaceRoot;
        public GameObject VictoryText;
        public GameObject DefeatText;

        private void Update()
        {
#if DEVELOPMENT_BUILD
            InterfaceRoot.SetActive(false);
#else
            var player = Interface.Root.Player;
            InterfaceRoot.SetActive(player.GameplayState is PlayerGameplateState.Win or PlayerGameplateState.Lose);
            VictoryText.SetActive(player.GameplayState == PlayerGameplateState.Win);
            DefeatText.SetActive(player.GameplayState == PlayerGameplateState.Lose);
#endif
        }
    }
}