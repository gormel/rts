using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Interaction
{
    class CameraInteractor : MonoBehaviour
    {
        private bool mFocused = true;

        public Vector3 UpMove;
        public Vector3 RightMove;

        public float Border = 2;

        public float Speed;
        private Vector3 mCameraVelocity = Vector3.zero;
        private bool mAltState;

        private void MoveCamera(bool up, bool right, bool down, bool left)
        {
            mCameraVelocity = Vector3.zero;
            
            if (left)
                mCameraVelocity -= RightMove;

            if (right)
                mCameraVelocity += RightMove;

            if (down)
                mCameraVelocity -= UpMove;

            if (up)
                mCameraVelocity += UpMove;

            mCameraVelocity.Normalize();
        }

        public void OnMove(InputAction.CallbackContext ctx)
        {
            var xy = ctx.ReadValue<Vector2>();
            var sx = xy.x;
            var sy = xy.y;
            if (!mFocused)
                return;

            MoveCamera(sy > 0, sx > 0, sy < 0, sx < 0);
            
        }

        public void OnAltState(InputAction.CallbackContext ctx)
        {
            mAltState = ctx.ReadValueAsButton();
        }
        
        public void OnPan(InputAction.CallbackContext ctx)
        {
            var xy = ctx.ReadValue<Vector2>();
            var mouseX = xy.x;
            var mouseY = xy.y;
            
            if (!mFocused)
                return;

#if UNITY_EDITOR
            if (!mAltState)
                return;
#endif
            MoveCamera(
                mouseY >= Screen.height - Border, 
                mouseX >= Screen.width - Border, 
                mouseY <= Border, 
                mouseX <= Border);
        }

        void Update()
        {
            transform.localPosition += mCameraVelocity * Speed * Time.deltaTime;
        }

        void OnApplicationFocus(bool hasFocus)
        {
            mFocused = hasFocus;
        }
    }
}
