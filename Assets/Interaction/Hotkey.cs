using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

namespace Assets.Interaction
{
    class Hotkey : MonoBehaviour
    {
        public OnScreenButton KeyBinding;
        public Text View;

        void Start()
        {
            if (View != null && KeyBinding != null)
                View.text = KeyBinding.control.name.ToUpper(CultureInfo.InvariantCulture);
        }
    }
}
