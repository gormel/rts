﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Interaction
{
    class Minimap : MonoBehaviour, IPointerClickHandler
    {
        public GameObject PlayerScreen;
        public RectTransform PlayerScreenIndicator;
        public Camera MinimapCamera;
        public RenderTexture MinimapTexture;
        public Root Root;

        private RectTransform mRectTransform;

        void OnEnable()
        {
            mRectTransform = GetComponent<RectTransform>();
        }

        void Update()
        {
            var screenPlayerCameraPosition = MinimapCamera.WorldToScreenPoint(PlayerScreen.transform.position);
            PlayerScreenIndicator.anchoredPosition = new Vector2(
                screenPlayerCameraPosition.x / MinimapTexture.width * mRectTransform.rect.width,
                screenPlayerCameraPosition.y / MinimapTexture.height * mRectTransform.rect.height
                );
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            var local = (mRectTransform.InverseTransformPoint(eventData.position) / mRectTransform.rect.size + new Vector2(0.5f, 0.5f)) * 
                        new Vector2(MinimapTexture.width, MinimapTexture.height);

            var world = MinimapCamera.ScreenToWorldPoint(local);

            Root.PlaseCamera(new Vector2(world.x, world.z));
        }
    }
}
