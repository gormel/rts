using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Interaction.Lobby
{
    class LobbyPlace : MonoBehaviour
    {
        public string Name { get; set; }
        public int Team { get; set; }
        
        public bool IsBusy;
        public GameObject BusyState;
        public GameObject FreeState;
        public Text NicknameText;
        public Text TeamText;
        
        void Update()
        {
            BusyState?.SetActive(IsBusy);
            FreeState?.SetActive(!IsBusy);
            
            NicknameText.text = Name;
            TeamText.text = IsBusy ? Team.ToString() : "";
        }
    }
}
