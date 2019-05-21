using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.GameObjects;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using Assets.Utils;
using Assets.Views;
using Assets.Views.Base;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.UIElements;

namespace Assets.Interaction
{
    class UserInterface : MonoBehaviour
    {
        private class RaycastResult<T> where T : class
        {
            public T Object;
            public Vector3 HitPoint;

            public RaycastResult(T o, Vector3 hitPoint)
            {
                Object = o;
                HitPoint = hitPoint;
            }
        }

        private class InterfaceAction
        {
            private readonly Action<Vector2> mOnClick;
            private readonly Action<Vector2> mOnMove;
            private readonly Action mOnCancel;

            public InterfaceAction(Action<Vector2> onClick, Action<Vector2> onMove, Action onCancel)
            {
                mOnClick = onClick;
                mOnMove = onMove;
                mOnCancel = onCancel;
            }

            public void Resolve(Vector2 position)
            {
                mOnClick?.Invoke(position);
            }

            public void Move(Vector2 position)
            {
                mOnMove?.Invoke(position);
            }

            public void Cancel()
            {
                mOnCancel?.Invoke();
            }
        }

        public RectTransform SelectionBox;

        public Root Root;
        public List<SelectableView> Selected { get; } = new List<SelectableView>();
        private InterfaceAction mCurrentAction;
        public GameObject BuildCursorPrefab;
        private bool mSelectionInProgress;
        private Vector3 mSelectionStartPosition;

        private RaycastResult<T> RaycastBase<T>(Vector2 mouse, Func<Ray, RaycastHit[]> raycaster) where T : class
        {
            var ray = Camera.main.ScreenPointToRay(mouse);
            var hits = raycaster(ray);
            if (hits == null)
                return null;

            foreach (var hit in hits)
            {
                var view = hit.transform.GetComponent<T>();
                if (view != null)
                    return new RaycastResult<T>(view, hit.point);
            }

            return null;
        }

        private RaycastResult<T> Raycast<T>(Vector2 mouse) where T : class
        {
            return RaycastBase<T>(mouse, Physics.RaycastAll);
        }

        public void BeginGoTo<TModel>(IEnumerable<UnitView<TModel>> views) where TModel : Unit
        {
            mCurrentAction = new InterfaceAction(pos =>
            {
                foreach (var view in views)
                    view.GoTo(pos);
            }, null, null);
        }

        public void BeginBuildingPlacement(IEnumerable<WorkerView> workers, Func<WorkerView, Vector2, BuildingTemplate> createTemplate, Vector2 size)
        {
            var cursorObj = Instantiate(BuildCursorPrefab);
            var colorer = cursorObj.GetComponent<BuildCursorColorer>();
            cursorObj.transform.localScale = new Vector3(
                cursorObj.transform.localScale.x * size.x, 
                cursorObj.transform.localScale.y, 
                cursorObj.transform.localScale.z * size.y);

            mCurrentAction = new InterfaceAction(pos =>
            {
                BuildingTemplate template = null;
                foreach (var view in workers.Take(1))
                    template = createTemplate(view, pos);

                if (template == null)
                    return;

                foreach (var view in workers.Skip(1))
                    view.AttachAsBuilder(template);

                cursorObj.transform.SetParent(null);
                Destroy(cursorObj);
            }, pos =>
            {
                pos = new Vector2(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y));

                colorer.Valid = Root.Game.GetIsAreaFree(pos, size);

                cursorObj.transform.localPosition = GameUtils.GetPosition(pos, Root.Game.Map);
            }, () =>
            {
                cursorObj.transform.SetParent(null);
                Destroy(cursorObj);
            });
        }

        void Update()
        {
            Selected.RemoveAll(view => view == null || view.gameObject == null);

            if (Input.GetMouseButtonUp((int) MouseButton.LeftMouse) && mSelectionInProgress)
            {
                var selectionRect = new Rect(mSelectionStartPosition, Input.mousePosition - mSelectionStartPosition);
                if (selectionRect.size.magnitude > 0.1f)
                {
                    if (!Input.GetKey(KeyCode.LeftShift))
                    {
                        foreach (var view in Selected)
                            view.IsSelected = false;

                        Selected.Clear();
                    }

                    var toSelect = new List<SelectableView>();
                    for (int i = 0; i < Root.MapView.ChildContainer.transform.childCount; i++)
                    {
                        var child = Root.MapView.ChildContainer.transform.GetChild(i);
                        var selectableView = child.GetComponent<SelectableView>();
                        if (selectableView == null)
                            continue;

                        var projected = Camera.main.WorldToScreenPoint(child.position);
                        if (selectionRect.Contains((Vector2) projected, true))
                            toSelect.Add(selectableView);
                    }

                    foreach (var selectableView in toSelect)
                    {
                        selectableView.IsSelected = true;
                        if (!Selected.Contains(selectableView))
                            Selected.Add(selectableView);
                    }
                }

                mSelectionInProgress = false;
            }

            SelectionBox.gameObject.SetActive(mSelectionInProgress);
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (mCurrentAction != null)
            {
                var mapHit = Raycast<MapView>(Input.mousePosition);
                if (mapHit == null)
                    return;

                var mapPoint = GameUtils.GetFlatPosition(mapHit.HitPoint);
                mCurrentAction.Move(mapPoint);

                if (Input.GetMouseButtonDown((int) MouseButton.LeftMouse))
                {
                    mCurrentAction.Resolve(mapPoint);
                    mCurrentAction = null;
                    return;
                }

                if (Input.GetMouseButtonDown((int) MouseButton.RightMouse))
                {
                    mCurrentAction.Cancel();
                    mCurrentAction = null;
                    return;
                }

                return;
            }

            if (Input.GetMouseButtonDown((int) MouseButton.LeftMouse))
            {
                mSelectionStartPosition = Input.mousePosition;
                mSelectionInProgress = true;
            }

            if (mSelectionInProgress)
            {
                SelectionBox.position = new Vector3(Mathf.Min(Input.mousePosition.x, mSelectionStartPosition.x), Mathf.Max(Input.mousePosition.y, mSelectionStartPosition.y));
                SelectionBox.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Abs(Input.mousePosition.x - mSelectionStartPosition.x));
                SelectionBox.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   Mathf.Abs(Input.mousePosition.y - mSelectionStartPosition.y));
            }

            if (Input.GetMouseButtonDown((int) MouseButton.LeftMouse))
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    foreach (var view in Selected)
                        view.IsSelected = false;

                    Selected.Clear();
                }

                var viewHit = Raycast<SelectableView>(Input.mousePosition);
                if (viewHit != null)
                {
                    viewHit.Object.IsSelected = true;
                    if (!Selected.Contains(viewHit.Object))
                        Selected.Add(viewHit.Object);
                }
            }

            if (Input.GetMouseButtonDown((int) MouseButton.RightMouse))
            {
                var viewHit = Raycast<SelectableView>(Input.mousePosition);
                if (viewHit != null)
                {
                    foreach (var view in Selected)
                        view.OnRightClick(viewHit.Object);

                    return;
                }

                var mapHit = Raycast<MapView>(Input.mousePosition);
                if (mapHit != null)
                {
                    var mapPoint = GameUtils.GetFlatPosition(mapHit.HitPoint);
                    foreach (var view in Selected)
                        view.OnRightClick(mapPoint);

                    return;
                }
            }
        }
    }
}
