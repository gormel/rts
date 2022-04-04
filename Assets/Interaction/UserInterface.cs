﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.GameObjects;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using Assets.Interaction.InterfaceUtils;
using Assets.Interaction.Selection;
using Assets.Utils;
using Assets.Views;
using Assets.Views.Base;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Assets.Interaction
{
    class UserInterface : MonoBehaviour
    {
        private enum SelectionState
        {
            Idle,
            Choose,
            Boxing
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

        private class AttackInterfaceAction : InterfaceAction
        {
            private IEnumerable<IWarriorOrders> mViews;
            private readonly Raycaster mRaycaster;

            public AttackInterfaceAction(IEnumerable<IWarriorOrders> views, Raycaster raycaster)
            {
                mViews = views;
                mRaycaster = raycaster;
            }

            public override void Resolve(Vector2 position)
            {
                var hit = mRaycaster.Raycast<SelectableView>(Input.mousePosition);
                if (hit.IsEmpty())
                {
                    foreach (var warriorOrders in mViews)
                        warriorOrders.GoToAndAttack(position);
                    return;
                }

                foreach (var warriorOrders in mViews)
                    warriorOrders.Attack(hit.Object.InfoBase.ID);
            }
        }

        private class TurretAttackInterfaceAction : InterfaceAction
        {
            private IEnumerable<ITurretOrders> mViews;
            private readonly Raycaster mRaycaster;

            public TurretAttackInterfaceAction(IEnumerable<ITurretOrders> views, Raycaster raycaster)
            {
                mViews = views;
                mRaycaster = raycaster;
            }

            public override void Resolve(Vector2 position)
            {
                var hit = mRaycaster.Raycast<SelectableView>(Input.mousePosition);
                if (!hit.IsEmpty())
                {
                    foreach (var warriorOrders in mViews)
                        warriorOrders.Attack(hit.Object.InfoBase.ID);
                }
            }
        }

        private class GoToInterfaceAction : InterfaceAction
        {
            private readonly IEnumerable<IUnitOrders> mViews;

            public GoToInterfaceAction(IEnumerable<IUnitOrders> views)
            {
                mViews = views;
            }
            public override void Resolve(Vector2 position)
            {
                foreach (var view in mViews)
                    view.GoTo(position);
            }
        }

        private class AttachWorkerToMiningCampAction : InterfaceAction
        {
            private readonly IEnumerable<IWorkerOrders> mViews;
            private readonly Raycaster mRaycaster;

            public AttachWorkerToMiningCampAction(IEnumerable<IWorkerOrders> views, Raycaster raycaster)
            {
                mViews = views;
                mRaycaster = raycaster;
            }
            public override void Resolve(Vector2 position)
            {
                var hit = mRaycaster.Raycast<MiningCampView>(Input.mousePosition);
                if (hit.IsEmpty())
                    return;
                
                foreach (var view in mViews)
                    view.AttachToMiningCamp(hit.Object.InfoBase.ID);
            }
        }

        private class BuildingPlacementInterfaceAction : InterfaceAction
        {
            private readonly UserInterface mUserInterface;
            private readonly IEnumerable<IWorkerOrders> mWorkers;
            private readonly Func<IWorkerOrders, Vector2, Task<Guid>> mCreateTemplate;
            private readonly Vector2 mSize;
            private GameObject cursorObj;
            private BuildCursorColorer colorer;

            public BuildingPlacementInterfaceAction(
                UserInterface userInterface, 
                IEnumerable<IWorkerOrders> workers, 
                Func<IWorkerOrders, Vector2, Task<Guid>> createTemplate, 
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
                    Task<Guid> createTemplateId = null;
                    foreach (var view in mWorkers.Take(1))
                        createTemplateId = mCreateTemplate(view, position);

                    if (createTemplateId == null)
                        return;

                    var templateId = await createTemplateId;
                    await Task.WhenAll(mWorkers.Skip(1).Select(o => o.AttachAsBuilder(templateId)));
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
                colorer.Valid = mUserInterface.Root.MapView.IsAreaFree(position, mSize);
                cursorObj.transform.localPosition = mUserInterface.Root.MapView.GetWorldPosition(position);
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
        public SelectionManager SelectionManager { get; private set; }
        private Raycaster mRaycaster;
        private Vector2 mChooseStartMousePosition;
        private SelectionState mSelectionState;
        
        public IEnumerable<T> FetchSelectedOrders<T>() where T : class, IGameObjectOrders
        {
            return Selected.Select(v => v.OrdersBase as T).Where(o => o != null);
        }
        
        public IEnumerable<T> FetchSelectedInfo<T>() where T : class, IGameObjectInfo
        {
            return Selected.Select(v => v.InfoBase as T).Where(o => o != null);
        }

        void Awake()
        {
            mRaycaster = new Raycaster(Camera.main);
            SelectionManager = new SelectionManager(SelectionBox, this, mRaycaster);
        }

        public void BeginGoTo(IEnumerable<IUnitOrders> views)
        {
            if (mCurrentAction != null)
                mCurrentAction.Cancel();

            mCurrentAction = new GoToInterfaceAction(views);
        }

        public void BeginAttachWorkerToMiningCamp(IEnumerable<IWorkerOrders> views)
        {
            if (mCurrentAction != null)
                mCurrentAction.Cancel();

            mCurrentAction = new AttachWorkerToMiningCampAction(views, mRaycaster);
        }

        public void BeginAttack(IEnumerable<IWarriorOrders> views)
        {
            if (mCurrentAction != null)
                mCurrentAction.Cancel();

            mCurrentAction = new AttackInterfaceAction(views, mRaycaster);
        }

        public void BeginTurretAttack(IEnumerable<ITurretOrders> views)
        {
            if (mCurrentAction != null)
                mCurrentAction.Cancel();

            mCurrentAction = new TurretAttackInterfaceAction(views, mRaycaster);
        }

        public void BeginBuildingPlacement(IEnumerable<IWorkerOrders> workers, Func<IWorkerOrders, Vector2, Task<Guid>> createTemplate, Vector2 size)
        {
            if (mCurrentAction != null)
                mCurrentAction.Cancel();

            mCurrentAction = new BuildingPlacementInterfaceAction(this, workers, createTemplate, size);
        }

        public void ForwardRightClick(Vector2 mapPoint)
        {
            foreach (var view in Selected)
            {
                if (!view.IsControlable)
                    continue;

                view.OnRightClick(mapPoint);
            }
        }

        private int updateId = 0;
        void Update()
        {
            updateId++;
            Selected.RemoveAll(view => view == null || view.gameObject == null || !view.gameObject.activeSelf);

            if (Input.GetMouseButtonDown((int) MouseButton.LeftMouse))
            {
                Debug.Log($"MouseDown {updateId}");
            }
            
            if (Input.GetMouseButtonUp((int) MouseButton.LeftMouse))
            {
                Debug.Log($"MouseUp {updateId}");
            }

            if (mSelectionState == SelectionState.Boxing && !Input.GetMouseButton((int)MouseButton.LeftMouse))
            {
                SelectionManager.FinishBoxSelection(Input.GetKey(KeyCode.LeftShift), Input.mousePosition);
                mSelectionState = SelectionState.Idle;
            }

            SelectionManager.Update(Input.mousePosition);
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (mCurrentAction != null && !EventSystem.current.IsPointerOverGameObject())
            {
                var mapHit = mRaycaster.Raycast<MapView>(Input.mousePosition);
                if (mapHit.IsEmpty())
                    return;

                var mapPoint = GameUtils.GetFlatPosition(mapHit.HitPoint);
                mCurrentAction.Move(mapPoint);

                if (Input.GetMouseButton((int) MouseButton.LeftMouse))
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

                if (Input.GetMouseButton((int) MouseButton.RightMouse))
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

            if (mSelectionState == SelectionState.Idle && Input.GetMouseButton((int) MouseButton.LeftMouse))
            {
                mSelectionState = SelectionState.Choose;
                mChooseStartMousePosition = Input.mousePosition;
                return;
            }

            if (mSelectionState == SelectionState.Choose)
            {
                if (!Input.GetMouseButton((int) MouseButton.LeftMouse))
                {
                    SelectionManager.SelectSingle(Input.GetKey(KeyCode.LeftShift), Input.mousePosition);
                    mSelectionState = SelectionState.Idle;
                }
                else if (Vector2.Distance(Input.mousePosition, mChooseStartMousePosition) > 3)
                {
                    SelectionManager.StartBoxSelection(Input.mousePosition);
                    mSelectionState = SelectionState.Boxing;
                    Debug.Log($"Boxing start {updateId}");
                }
            }

            if (Input.GetMouseButtonDown((int) MouseButton.RightMouse))
            {
                var viewHit = mRaycaster.Raycast<SelectableView>(Input.mousePosition);
                if (!viewHit.IsEmpty())
                {
                    foreach (var view in Selected)
                    {
                        if (!view.IsControlable)
                            continue;

                        if (viewHit.Object.IsControlable)
                            view.OnRightClick(viewHit.Object);
                        else
                            view.OnEnemyRightClick(viewHit.Object);
                    }

                    return;
                }

                var mapHit = mRaycaster.Raycast<MapView>(Input.mousePosition);
                if (!mapHit.IsEmpty())
                {
                    var mapPoint = GameUtils.GetFlatPosition(mapHit.HitPoint);
                    ForwardRightClick(mapPoint);

                    return;
                }
            }
        }
    }
}
