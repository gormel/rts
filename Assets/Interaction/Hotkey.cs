using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Assets.Interaction
{
    public class Hotkey : MonoBehaviour
    {
        public Button TargetButton;
        public Text View;
        public string ActionName;
        
        private RtsInputActions mInputActions;

        private void OnEnable()
        {
            mInputActions.Enable();
        }

        private void OnDisable()
        {
            mInputActions.Disable();
        }

        private void OnDestroy()
        {
            mInputActions.Dispose();
        }

        private void Awake()
        {
            mInputActions = new RtsInputActions();
            if (!string.IsNullOrEmpty(ActionName))
                mInputActions.Bindings.Get()[ActionName].performed += OnHotkey;
        }

        private void OnHotkey(InputAction.CallbackContext obj)
        {
            if (TargetButton != null && TargetButton.interactable)
                TargetButton.onClick.Invoke();
        }

        void Start()
        {
            if (View != null)
            {
                var keyboardCtrl = mInputActions.Bindings.Get()[ActionName].controls
                    .FirstOrDefault(ctrl => InputControlPath.Matches("<Keyboard>/*", ctrl));
                if (keyboardCtrl != null)
                {
                    foreach (var pathComponent in InputControlPath.Parse(keyboardCtrl.path))
                    {
                        if (!string.IsNullOrEmpty(pathComponent.name) && pathComponent.name != "Keyboard")
                        {
                            View.text = pathComponent.name.ToUpper();
                            break;
                        }
                    }
                }
            }
        }
    }
}
