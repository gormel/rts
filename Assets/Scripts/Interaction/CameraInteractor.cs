using System;
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

        public GameObject InnerCamera;
        public float MinDistance;
        public float MaxDistance;
        public float ZoomSpeed;
        
        private Vector3 mCameraVelocity = Vector3.zero;
        private bool mAltState;
        private RtsInputActions mInputActions;

        private bool mMouseLeft;
        private bool mMouseUp;
        private bool mMouseRight;
        private bool mMouseDown;
        
        private bool mKeyboardLeft;
        private bool mKeyboardUp;
        private bool mKeyboardRight;
        private bool mKeyboardDown;

        private float mCameraDistance;

        void Awake()
        {
            mInputActions = new RtsInputActions();
            mInputActions.Camera.AltState.performed += OnAltState;
            mInputActions.Camera.Pan.performed += OnPan;
            mInputActions.Camera.Move.performed += OnMove;
            mInputActions.Camera.Zoom.performed += OnZoom;
        }

        private void OnZoom(InputAction.CallbackContext obj)
        {
            mCameraDistance = Mathf.Clamp(mCameraDistance + Math.Sign(obj.ReadValue<Vector2>().y) * ZoomSpeed, MinDistance, MaxDistance);
        }

        void OnEnable()
        {
            mInputActions.Enable();
            mCameraDistance = MaxDistance;
        }

        void OnDisable()
        {
            mInputActions.Disable();
        }

        private Vector3 CalcCameraVelocity(bool up, bool right, bool down, bool left)
        {
            var velocity = Vector3.zero;
            
            if (left)
                velocity -= RightMove;

            if (right)
                velocity += RightMove;

            if (down)
                velocity -= UpMove;

            if (up)
                velocity += UpMove;

            velocity.Normalize();
            return velocity;
        }

        public void OnMove(InputAction.CallbackContext ctx)
        {
            var xy = ctx.ReadValue<Vector2>();
            var sx = xy.x;
            var sy = xy.y;
            
            if (!mFocused)
                return;

            mKeyboardLeft = sx < 0;
            mKeyboardUp = sy > 0;
            mKeyboardRight = sx > 0;
            mKeyboardDown = sy < 0;
            
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
            
            mMouseLeft = false;
            mMouseUp = false;
            mMouseRight = false;
            mMouseDown = false;
            
            if (!mFocused)
                return;
            
#if DEVELOPMENT_BUILD
            if (!mAltState)
                return;
#endif
            mMouseLeft = mouseX <= Border;
            mMouseUp = mouseY >= Screen.height - Border;
            mMouseRight = mouseX >= Screen.width - Border;
            mMouseDown = mouseY <= Border;
        }

        void Update()
        {
            mCameraVelocity = CalcCameraVelocity(
                mMouseUp || mKeyboardUp, 
                mMouseRight || mKeyboardRight, 
                mMouseDown || mKeyboardDown, 
                mMouseLeft || mKeyboardLeft);

            var innerPos = InnerCamera.transform.localPosition;
            innerPos.z = -mCameraDistance;
            InnerCamera.transform.localPosition = innerPos;

            transform.localPosition += mCameraVelocity * (global::Settings.Settings.CameraSpeed * Time.deltaTime);
        }

        void OnApplicationFocus(bool hasFocus)
        {
            mFocused = hasFocus;
        }
    }
}
