using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Map;
using Assets.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Interaction
{
    class Minimap : MonoBehaviour, IPointerClickHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IPointerExitHandler
    {
        public GameObject PlayerScreen;
        public RectTransform PlayerScreenIndicator;
        public Camera MinimapCamera;
        public RenderTexture MinimapTexture;
        public Root Root;
        public UserInterface Interface;

        private RectTransform mRectTransform;
        private bool mIsCameraMovement;

        void OnEnable()
        {
            mRectTransform = GetComponent<RectTransform>();
            Root.MapLoaded += RootOnMapLoaded;
        }

        private void RootOnMapLoaded(IMapData obj)
        {
            var x = (obj.Width - 1) / 2f;
            var z = (obj.Length - 1) / 2f;
            MinimapCamera.transform.localPosition = new Vector3(x, 50, z);
            MinimapCamera.orthographicSize = Mathf.Max(x, z);
        }

        void Update()
        {
            var screenPlayerCameraPosition = MinimapCamera.WorldToScreenPoint(PlayerScreen.transform.position);
            PlayerScreenIndicator.anchoredPosition = new Vector2(
                screenPlayerCameraPosition.x / MinimapTexture.width * mRectTransform.rect.width,
                screenPlayerCameraPosition.y / MinimapTexture.height * mRectTransform.rect.height
                );
        }

        private Vector3 ProjectToWorld(Vector2 screenPosition)
        {
            var local = (mRectTransform.InverseTransformPoint(screenPosition) / mRectTransform.rect.size +
                         new Vector2(0.5f, 0.5f)) *
                        new Vector2(MinimapTexture.width, MinimapTexture.height);

            return MinimapCamera.ScreenToWorldPoint(local);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var world = ProjectToWorld(eventData.position);

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                mIsCameraMovement = true;
                Root.PlaceCamera(new Vector2(world.x, world.z));
            }

            if (eventData.button == PointerEventData.InputButton.Right)
            {
                Interface.ForwardRightClick(new Vector2(world.x, world.z));
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            mIsCameraMovement = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            
            mIsCameraMovement = true;
            var world = ProjectToWorld(eventData.position);
            Root.PlaceCamera(new Vector2(world.x, world.z));
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (mIsCameraMovement)
            {
                var world = ProjectToWorld(eventData.position);
                Root.PlaceCamera(new Vector2(world.x, world.z));
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            mIsCameraMovement = false;
        }
    }
}
