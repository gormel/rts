using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Interaction.Lobby
{
    class LobbyPlace : MonoBehaviour
    {
        public bool IsBusy;
        public GameObject BusyState;
        public GameObject FreeState;

        void Update()
        {
            BusyState?.SetActive(IsBusy);
            FreeState?.SetActive(!IsBusy);
        }
    }
}
