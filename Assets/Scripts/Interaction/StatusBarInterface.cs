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
        public Text LimitText;

        void Update()
        {
            var money = Interface.Root.Player?.Money;
            if (money != null)
                MoneyText.text = $"{money}$";

            var limit = Interface.Root.Player?.Limit;
            if (limit != null)
                LimitText.text = $"{limit}/200";
        }
    }
}
