using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Interaction.InterfaceUtils;
using Assets.Views.Base;
using UnityEngine;

namespace Assets.Interaction.Selection
{
    class SelectionManager
    {
        private readonly RectTransform mSelectionBox;
        private readonly UserInterface mUserInterface;
        private readonly Raycaster mRaycaster;
        private bool mSelectionInProgress;
        private Vector3 mSelectionStartPosition;
        private SelectableView mLastMouseOver;

        public SelectionGroup[] Groups { get; } = new SelectionGroup[9];
        public IReadOnlyCollection<SelectableView> Selected => mUserInterface.Selected;

        public SelectionManager(RectTransform selectionBox, UserInterface userInterface, Raycaster raycaster)
        {
            mSelectionBox = selectionBox;
            mUserInterface = userInterface;
            mRaycaster = raycaster;

            for (int i = 0; i < Groups.Length; i++)
            {
                Groups[i] = new SelectionGroup(this, mUserInterface.Root);
            }
        }

        public void Select(IEnumerable<SelectableView> views)
        {
            SelectInner(views, false);
        }

        public void RemoveSelection(IEnumerable<SelectableView> views)
        {
            foreach (var selectableView in views)
            {
                selectableView.IsSelected = false;
                mUserInterface.Selected.Remove(selectableView);
            }
        }

        private void SelectInner(IEnumerable<SelectableView> views, bool union)
        {
            if (!union)
            {
                foreach (var selectableView in mUserInterface.Selected)
                    selectableView.IsSelected = false;

                mUserInterface.Selected.Clear();
            }

            foreach (var selectableView in views.Where(v => v != null))
            {
                selectableView.IsSelected = true;
                if (!mUserInterface.Selected.Contains(selectableView))
                    mUserInterface.Selected.Add(selectableView);
            }
        }

        public void StartBoxSelection(Vector3 mouse)
        {
            mSelectionStartPosition = mouse;
            mSelectionInProgress = true;
        }

        public void FinishBoxSelection(bool union, Vector3 mouse)
        {
            if (!mSelectionInProgress)
                return;

            var selectionRect = new Rect(mSelectionStartPosition, mouse - mSelectionStartPosition);
            if (selectionRect.size.magnitude > 0.1f)
            {
                var toSelect = new List<SelectableView>();
                for (int i = 0; i < mUserInterface.Root.MapView.ChildContainer.transform.childCount; i++)
                {
                    var child = mUserInterface.Root.MapView.ChildContainer.transform.GetChild(i);
                    var selectableView = child.GetComponent<SelectableView>();
                    if (selectableView == null)
                        continue;

                    if (selectableView.OwnershipRelation != ObjectOwnershipRelation.My)
                        continue;

                    var projected = Camera.main.WorldToScreenPoint(child.position);
                    if (selectionRect.Contains((Vector2)projected, true))
                        toSelect.Add(selectableView);
                }

                IEnumerable<SelectableView> filtered = new SelectableView[0];
                if (toSelect.Count > 0)
                    filtered = toSelect.GroupBy(view => view.SelectionPriority).OrderByDescending(g => g.Key).First();

                SelectInner(filtered, union);
            }

            mSelectionInProgress = false;
            mSelectionBox.gameObject.SetActive(mSelectionInProgress);
        }

        public void SelectSingle(bool union, Vector3 mouse)
        {
            var viewHit = mRaycaster.Raycast<SelectableView>(mouse);
            var isObjectExist = !viewHit.IsEmpty() && (mUserInterface.Selected.Count == 0 || mUserInterface.Selected[0].OwnershipRelation == viewHit.Object.OwnershipRelation);
            var objs = isObjectExist ? new[] {viewHit.Object} : new SelectableView[0];
            SelectInner(objs, union);
        }
        
        private static Vector2 ScreenToRectPos(Vector2 screenPoint, RectTransform targetRect, Canvas canvas)
        {
            //Canvas is in Camera mode
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && canvas.worldCamera != null)
            {	        
                Vector2 anchorPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRect, screenPoint, canvas.worldCamera, out anchorPos);
                return anchorPos;
            }
            //Canvas is in Overlay mode
            else
            {   
                Vector2 anchorPos = screenPoint - new Vector2(targetRect.position.x, targetRect.position.y);
                anchorPos = new Vector2(anchorPos.x / targetRect.lossyScale.x, anchorPos.y / targetRect.lossyScale.y);
                return anchorPos;
            }
        }

        public void Update(Vector3 mouse, RectTransform guiRoot, Canvas canvas)
        {
            if (mSelectionBox.gameObject == null)
                return;
            
            mSelectionBox.gameObject.SetActive(mSelectionInProgress);

            if (mSelectionInProgress)
            {
                var min = ScreenToRectPos(Vector3.Min(mouse, mSelectionStartPosition), guiRoot, canvas);
                var max = ScreenToRectPos(Vector3.Max(mouse, mSelectionStartPosition), guiRoot, canvas);
                
                mSelectionBox.anchoredPosition = min;
                mSelectionBox.sizeDelta = max - min;
            }

            if (mLastMouseOver != null)
                mLastMouseOver.IsMouseOver = false;

            if (!mSelectionInProgress)
            {
                var hit = mRaycaster.Raycast<SelectableView>(mouse);
                if (!hit.IsEmpty())
                {
                    hit.Object.IsMouseOver = true;
                    mLastMouseOver = hit.Object;
                }
            }
        }
    }
}
