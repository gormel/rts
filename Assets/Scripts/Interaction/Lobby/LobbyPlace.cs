using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Interaction.Lobby
{
    class LobbyPlace : MonoBehaviour
    {
        public string Name { get; set; }
        public int Team { get; set; }
        
        public bool IsBusy { get; set; }
        public bool IsBot { get; set; }

        public LobbyRoot Root;
        
        public GameObject BusyState;
        public GameObject FreeState;
        public GameObject BotState;
        public Text NicknameText;
        public Text TeamText;

        public GameObject[] ServerOnlyVisible;
        
        void Update()
        {
            BusyState?.SetActive(IsBusy && !IsBot);
            BotState?.SetActive(IsBusy && IsBot);
            FreeState?.SetActive(!IsBusy);
            
            NicknameText.text = Name;
            TeamText.text = IsBusy ? Team.ToString() : "";

            foreach (var o in ServerOnlyVisible) 
                o.SetActive(GameUtils.CurrentMode == GameMode.Server);
        }

        public void RemoveBot()
        {
            Root.RemoveBot(Name);
        }

        public void ChangeTeam()
        {
            Root.SetBotTeam(Name, Math.Max((Team + 1) % (GameUtils.MaxPlayers + 1), 1));
        }
    }
}
