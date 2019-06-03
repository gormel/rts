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
        private bool mFocused;

        public Vector3 UpMove;
        public Vector3 RightMove;

        public float Speed;

        void Update()
        {
            if (!mFocused || !Input.GetKey(KeyCode.LeftAlt))
                return;

            var velocity = Vector3.zero;

            var mouseX = Input.mousePosition.x;
            var mouseY = Input.mousePosition.y;

            if (mouseX < 0)
                velocity -= RightMove;

            if (mouseX > Screen.width)
                velocity += RightMove;

            if (mouseY < 0)
                velocity -= UpMove;

            if (mouseY > Screen.height)
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
