using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Interaction
{
    class MoneyValueInterface : MonoBehaviour
    {
        public UserInterface Interface;
        public Text Text;

        void Update()
        {
            var money = Interface.Root.Player?.Money;
            if (money != null)
                Text.text = money.Resources.ToString();
        }
    }
}
