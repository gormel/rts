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

        private abstract class InterfaceAction
        {
            public virtual void Resolve(Vector2 position)
            {
            }

            public virtual void Move(Vector2 position)
            {
            }

            public virtual void Cancel()
            {
            }
        }

        private class GoToInterfaceAction<TOrders, TInfo> : InterfaceAction
            where TOrders : IUnitOrders
            where TInfo : IUnitInfo
        {
            private readonly IEnumerable<UnitView<TOrders, TInfo>> mViews;

            public GoToInterfaceAction(IEnumerable<UnitView<TOrders, TInfo>> views)
            {
                mViews = views;
            }
            public override void Resolve(Vector2 position)
            {
                foreach (var view in mViews)
                    view.GoTo(position);
            }
        }

        private class BuildingPlacementInterfaceAction : InterfaceAction
        {
            private readonly UserInterface mUserInterface;
            private readonly IEnumerable<WorkerView> mWorkers;
            private readonly Func<WorkerView, Vector2, Task<BuildingTemplate>> mCreateTemplate;
            private readonly Vector2 mSize;
            private GameObject cursorObj;
            private BuildCursorColorer colorer;

            public BuildingPlacementInterfaceAction(
                UserInterface userInterface, 
                IEnumerable<WorkerView> workers, 
                Func<WorkerView, Vector2, Task<BuildingTemplate>> createTemplate, 
                Vector2 size
                )
            {
                mUserInterface = userInterface;
                mWorkers = workers;
                mCreateTemplate = createTemplate;
                mSize = size;
                cursorObj = Instantiate(userInterface.BuildCursorPrefab);
                colorer = cursorObj.GetComponent<BuildCursorColorer>();
                cursorObj.transform.localScale = new Vector3(
                    cursorObj.transform.localScale.x * size.x,
                    cursorObj.transform.localScale.y,
                    cursorObj.transform.localScale.z * size.y);
            }

            public override async void Resolve(Vector2 position)
            {
                try
                {
                    Task<BuildingTemplate> createTemplate = null;
                    foreach (var view in mWorkers.Take(1))
                        createTemplate = mCreateTemplate(view, position);

                    if (createTemplate == null)
                        return;

                    var template = await createTemplate;

                    foreach (var view in mWorkers.Skip(1))
                        view.AttachAsBuilder(template.ID);
                }
                finally
                {
                    cursorObj.transform.SetParent(null);
                    Destroy(cursorObj);
                }
            }

            public override void Move(Vector2 position)
            {
                position = new Vector2(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
                colorer.Valid = mUserInterface.Root.Game.GetIsAreaFree(position, mSize);
                cursorObj.transform.localPosition = GameUtils.GetPosition(position, mUserInterface.Root.Game.Map);
            }

            public override void Cancel()
            {
                cursorObj.transform.SetParent(null);
                Destroy(cursorObj);
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

        public void BeginGoTo<TOrders, TInfo>(IEnumerable<UnitView<TOrders, TInfo>> views) 
            where TOrders : IUnitOrders
            where TInfo : IUnitInfo
        {
            if (mCurrentAction != null)
                mCurrentAction.Cancel();

            mCurrentAction = new GoToInterfaceAction<TOrders, TInfo>(views);
        }

        public void BeginBuildingPlacement(IEnumerable<WorkerView> workers, Func<WorkerView, Vector2, Task<BuildingTemplate>> createTemplate, Vector2 size)
        {
            if (mCurrentAction != null)
                mCurrentAction.Cancel();

            mCurrentAction = new BuildingPlacementInterfaceAction(this, workers, createTemplate, size);
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
                    try
                    {
                        mCurrentAction.Resolve(mapPoint);
                    }
                    finally
                    {
                        mCurrentAction = null;
                    }
                    return;
                }

                if (Input.GetMouseButtonDown((int) MouseButton.RightMouse))
                {
                    try
                    {
                        mCurrentAction.Cancel();
                    }
                    finally
                    {
                        mCurrentAction = null;
                    }
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
