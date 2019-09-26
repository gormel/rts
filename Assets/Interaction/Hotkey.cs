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

        void Start()
        {
            mButton = GetComponent<Button>();
        }

        void Update()
        {
            if (Input.GetKeyDown(Key))
                mButton?.onClick?.Invoke();
        }
    }
}
