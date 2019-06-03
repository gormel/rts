using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Interaction
{
    class StatusBarInterface : MonoBehaviour
    {
        public UserInterface Interface;
        public Text MoneyText;

        void Update()
        {
            var money = Interface.Root.Player?.Money;
            if (money != null)
                MoneyText.text = money.ToString();
        }

        public void Close()
        {
            SceneManager.LoadScene("Start");
        }
    }
}
