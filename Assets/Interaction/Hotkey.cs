using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Interaction
{
    class Hotkey : MonoBehaviour
    {
        private Button mButton;
        public KeyCode Key;
        public Text View;

        void Start()
        {
            mButton = GetComponent<Button>();
            if (View != null)
                View.text = Key.ToString();
        }

        void Update()
        {
            if (Input.GetKeyDown(Key))
                mButton?.onClick?.Invoke();
        }
    }
}
