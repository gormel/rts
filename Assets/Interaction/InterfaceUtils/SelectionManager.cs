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

                    if (!selectableView.IsControlable)
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
        }

        public void SelectSingle(bool union, Vector3 mouse)
        {
            var viewHit = mRaycaster.Raycast<SelectableView>(mouse);
            var isObjectExist = !viewHit.IsEmpty() && (mUserInterface.Selected.Count == 0 || mUserInterface.Selected[0].IsControlable == viewHit.Object.IsControlable);
            var objs = isObjectExist ? new[] {viewHit.Object} : new SelectableView[0];
            SelectInner(objs, union);
        }

        public void Update(Vector3 mouse)
        {
            mSelectionBox.gameObject.SetActive(mSelectionInProgress);

            if (mSelectionInProgress)
            {
                mSelectionBox.position = new Vector3(Mathf.Min(mouse.x, mSelectionStartPosition.x), Mathf.Max(mouse.y, mSelectionStartPosition.y));
                mSelectionBox.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Abs(mouse.x - mSelectionStartPosition.x));
                mSelectionBox.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Abs(mouse.y - mSelectionStartPosition.y));
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
