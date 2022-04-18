using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Assets.Interaction.Selection
{
    class SelectionGroupInterface : MonoBehaviour, IPointerClickHandler
    {
        public Text CountTarget;
        public int Index; //[1-9]
        public UserInterface Interface;
        public Image IconTarget;
        public Sprite EmptySprite;

        public bool mShiftState;
        public bool mCtrlState;

        private SelectionGroup mSelectionGroup;
        private RtsInputActions mInputActions;

        void Awake()
        {
            if (Interface == null)
                return;
            
            if (Index < 1 || Index > 9)
                return;
            
            mInputActions = new RtsInputActions();
            mInputActions.Groups.SetGroupState.performed += OnCtrlState;
            mInputActions.Groups.AddToGroupState.performed += OnShiftState;
            mInputActions.Groups.InteractToGroup.performed += OnInteractToGroup;

            StartCoroutine(InitSelectionGroup());
        }

        private void OnEnable()
        {
            mInputActions.Enable();
        }

        private void OnDisable()
        {
            mInputActions.Disable();
        }

        IEnumerator InitSelectionGroup()
        {
            yield return new WaitUntil(() => Interface.isActiveAndEnabled);

            mSelectionGroup = Interface.SelectionManager.Groups[Index - 1];
        }

        public void OnShiftState(InputAction.CallbackContext ctx)
        {
            mShiftState = ctx.ReadValueAsButton();
        }

        public void OnCtrlState(InputAction.CallbackContext ctx)
        {
            mCtrlState = ctx.ReadValueAsButton();
        }

        public void OnInteractToGroup(InputAction.CallbackContext ctx)
        {
            if (Math.Abs(ctx.ReadValue<float>() - Index) < 0.01)
            {
                if (mCtrlState)
                    mSelectionGroup.Set();
                else if (mShiftState)
                    mSelectionGroup.Add();
                else
                    mSelectionGroup.Select();
            }
        }

        void Update()
        {
            if (mSelectionGroup == null)
                return;

            var count = mSelectionGroup.Members.Count(go => go != null);
            CountTarget.text = count > 0 ? count.ToString() : "";

            var firstInGroup = mSelectionGroup.Members.FirstOrDefault(v => v != null);
            if (firstInGroup != null)
                IconTarget.sprite = firstInGroup.Icon;
            else
                IconTarget.sprite = EmptySprite;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            mSelectionGroup.Select();
        }
    }
}
