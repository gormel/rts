using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
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

        private SelectionGroup mSelectionGroup;

        void Awake()
        {
            if (Interface == null)
                return;
            
            if (Index < 1 || Index > 9)
                return;

            StartCoroutine(InitSelectionGroup());
        }

        IEnumerator InitSelectionGroup()
        {
            yield return new WaitUntil(() => Interface.isActiveAndEnabled);

            mSelectionGroup = Interface.SelectionManager.Groups[Index - 1];
        }

        void Update()
        {
            if (mSelectionGroup == null)
                return;

            if (Input.GetKeyDown(KeyCode.Alpha1 + Index - 1))
            {
                if (Input.GetKey(KeyCode.LeftControl))
                    mSelectionGroup.Set();
                else if (Input.GetKey(KeyCode.LeftShift))
                    mSelectionGroup.Add();
                else
                    mSelectionGroup.Select();
            }

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
