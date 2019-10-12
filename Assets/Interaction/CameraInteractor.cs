using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Interaction
{
    class CameraInteractor : MonoBehaviour
    {
        private bool mFocused = true;

        public Vector3 UpMove;
        public Vector3 RightMove;

        public float Border = 2;

        public float Speed;

        void Update()
        {
            if (!mFocused)
                return;

#if UNITY_EDITOR
            if (!Input.GetKey(KeyCode.LeftAlt))
                return;
#endif
            
            var velocity = Vector3.zero;

            var mouseX = Input.mousePosition.x;
            var mouseY = Input.mousePosition.y;

            if (mouseX <= Border || Input.GetKey(KeyCode.LeftArrow))
                velocity -= RightMove;

            if (mouseX >= Screen.width - Border || Input.GetKey(KeyCode.RightArrow))
                velocity += RightMove;

            if (mouseY <= Border || Input.GetKey(KeyCode.DownArrow))
                velocity -= UpMove;

            if (mouseY >= Screen.height - Border || Input.GetKey(KeyCode.UpArrow))
                velocity += UpMove;

            velocity.Normalize();
            transform.localPosition += velocity * Speed * Time.deltaTime;
        }

        void OnApplicationFocus(bool hasFocus)
        {
            mFocused = hasFocus;
        }
    }
}
