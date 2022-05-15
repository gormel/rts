using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.GameObjects;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using Assets.Core.GameObjects.Utils;
using Assets.Interaction.InterfaceUtils;
using Assets.Interaction.Selection;
using Assets.Utils;
using Assets.Views;
using Assets.Views.Base;
using Core.GameObjects.Final;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace Assets.Interaction
{
    class UserInterface : MonoBehaviour
    {
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
            private IEnumerable<IAttackerOrders> mViews;
            private readonly Raycaster mRaycaster;
            private readonly UserInterface mParent;

            public AttackInterfaceAction(IEnumerable<IAttackerOrders> views, Raycaster raycaster, UserInterface parent)
            {
                mViews = views;
                mRaycaster = raycaster;
                mParent = parent;
            }

            public override void Resolve(Vector2 position)
            {
                var hit = mRaycaster.Raycast<SelectableView>(mParent.mMousePosition);
                if (hit.IsEmpty())
                {
                    foreach (var warriorOrders in mViews.OfType<IWarriorOrders>())
                        warriorOrders.GoToAndAttack(position);
                    return;
                }

                foreach (var warriorOrders in mViews)
                    warriorOrders.Attack(hit.Object.InfoBase.ID);
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

        private class LaunchMissileInterfaceAction : InterfaceAction
        {
            private readonly IEnumerable<IArtilleryOrders> mViews;

            public LaunchMissileInterfaceAction(IEnumerable<IArtilleryOrders> views)
            {
                mViews = views;
            }
            public override void Resolve(Vector2 position)
            {
                foreach (var view in mViews)
                    view.Launch(position);
            }
        }

        private class AttachWorkerToMiningCampAction : InterfaceAction
        {
            private readonly IEnumerable<IWorkerOrders> mViews;
            private readonly Raycaster mRaycaster;
            private readonly UserInterface mParent;

            public AttachWorkerToMiningCampAction(IEnumerable<IWorkerOrders> views, Raycaster raycaster, UserInterface parent)
            {
                mViews = views;
                mRaycaster = raycaster;
                mParent = parent;
            }
            public override void Resolve(Vector2 position)
            {
                var hit = mRaycaster.Raycast<MiningCampView>(mParent.mMousePosition);
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
            private readonly Func<Vector2, bool> mAdditionalValidation;
            private GameObject cursorObj;
            private BuildCursorColorer colorer;

            public BuildingPlacementInterfaceAction(
                UserInterface userInterface, 
                IEnumerable<IWorkerOrders> workers, 
                Func<IWorkerOrders, Vector2, Task<Guid>> createTemplate, 
                Vector2 size,
                Func<Vector2, bool> additionalValidation = null
                )
            {
                mUserInterface = userInterface;
                mWorkers = workers;
                mCreateTemplate = createTemplate;
                mSize = size;
                mAdditionalValidation = additionalValidation;
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
                var additionalValid = mAdditionalValidation == null || mAdditionalValidation(position);
                colorer.Valid = mUserInterface.Root.MapView.IsAreaFree(position, mSize) && additionalValid;
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
        public GameObject BuildCursorPrefab;

        public Canvas GuiCanvas;
        public RectTransform GuiRoot;

        public Texture2D NormalMouse;
        public Vector2 NormalMouseTexOffset;
        public Texture2D MoveMouse;
        public Vector2 MoveMouseTexOffset;
        public Texture2D AttackMouse;
        public Vector2 AttackMouseTexOffset;
        public Texture2D PlaceToMouse;
        public Vector2 PlaceMouseTexOffset;
        public List<SelectableView> Selected { get; } = new();
        private InterfaceAction mCurrentAction;
        public SelectionManager SelectionManager { get; private set; }
        private Raycaster mRaycaster;
        private Vector2 mChooseStartMousePosition;
        private Vector2 mMousePosition;
        private bool mShiftState;
        private RtsInputActions mInputActions;
        
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
            mInputActions = new RtsInputActions();
            
            mInputActions.OnScreenInteraction.MouseMove.performed += OnMouseMove;
            mInputActions.OnScreenInteraction.BeginDrag.performed += OnBeginDrag;
            mInputActions.OnScreenInteraction.Drop.performed += OnDrop;
            mInputActions.OnScreenInteraction.LeftClick.performed += OnLeftClick;
            mInputActions.OnScreenInteraction.RightClick.performed += OnRightClick;
            mInputActions.OnScreenInteraction.ShiftMod.performed += OnShiftPress;
            mInputActions.OnScreenInteraction.RollGroup.performed += OnRollGroup;
            
            Cursor.SetCursor(NormalMouse, NormalMouseTexOffset, CursorMode.Auto);
        }

        private void OnRollGroup(InputAction.CallbackContext obj)
        {
            if (Selected.Count < 1)
                return;

            var type = Selected[0].InfoBase.GetType();
            var rollCount = Selected.TakeWhile(v => v.InfoBase.GetType() == type).Count();
            if (rollCount == Selected.Count)
                return;

            for (int i = 0; i < rollCount; i++)
            {
                var zero = Selected[0];
                Selected.RemoveAt(0);
                Selected.Add(zero);
            }
        }

        void OnEnable()
        {
            mInputActions.Enable();
        }

        void OnDisable()
        {
            mInputActions.Disable();
        }

        public void BeginGoTo(IEnumerable<IUnitOrders> views)
        {
            if (mCurrentAction != null)
                mCurrentAction.Cancel();

            Cursor.SetCursor(MoveMouse, MoveMouseTexOffset, CursorMode.Auto);
            mCurrentAction = new GoToInterfaceAction(views);
        }

        public void BeginLaunchMissile(IEnumerable<IArtilleryOrders> views)
        {
            if (mCurrentAction != null)
                mCurrentAction.Cancel();

            Cursor.SetCursor(AttackMouse, AttackMouseTexOffset, CursorMode.Auto);
            mCurrentAction = new LaunchMissileInterfaceAction(views);
        }

        public void BeginAttachWorkerToMiningCamp(IEnumerable<IWorkerOrders> views)
        {
            if (mCurrentAction != null)
                mCurrentAction.Cancel();

            Cursor.SetCursor(PlaceToMouse, PlaceMouseTexOffset, CursorMode.Auto);
            mCurrentAction = new AttachWorkerToMiningCampAction(views, mRaycaster, this);
        }

        public void BeginAttack(IEnumerable<IAttackerOrders> views)
        {
            if (mCurrentAction != null)
                mCurrentAction.Cancel();

            Cursor.SetCursor(AttackMouse, AttackMouseTexOffset, CursorMode.Auto);
            mCurrentAction = new AttackInterfaceAction(views, mRaycaster, this);
        }

        public void BeginBuildingPlacement(
            IEnumerable<IWorkerOrders> workers, 
            Func<IWorkerOrders, Vector2, Task<Guid>> createTemplate, 
            Vector2 size, 
            Func<Vector2, bool> additionalValidation = null)
        {
            if (mCurrentAction != null)
                mCurrentAction.Cancel();

            mCurrentAction = new BuildingPlacementInterfaceAction(this, workers, createTemplate, size, additionalValidation);
        }

        public void ForwardRightClick(Vector2 mapPoint)
        {
            foreach (var view in Selected)
            {
                if (view.OwnershipRelation != ObjectOwnershipRelation.My)
                    continue;

                view.OnRightClick(mapPoint);
            }
        }

        public void OnShiftPress(InputAction.CallbackContext ctx)
        {
            mShiftState = ctx.ReadValueAsButton();
        }
        
        public void OnLeftClick(InputAction.CallbackContext ctx)
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (ctx.phase != InputActionPhase.Performed)
                return;

            if (mCurrentAction != null)
            {
                var mapHit = mRaycaster.Raycast<MapView>(mMousePosition);
                if (mapHit.IsEmpty())
                    return;

                var mapPoint = GameUtils.GetFlatPosition(mapHit.HitPoint);
                ResolveAction(mapPoint);
                return;
            }
            
            SelectionManager.SelectSingle(mShiftState, mMousePosition);
        }

        public bool ResolveAction(Vector2 mapPoint)
        {
            if (mCurrentAction == null)
                return false;
            
            mCurrentAction.Resolve(mapPoint);
            mCurrentAction = null;
            Cursor.SetCursor(NormalMouse, NormalMouseTexOffset, CursorMode.Auto);
            return true;
        }

        public void OnRightClick(InputAction.CallbackContext ctx)
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            
            if (mCurrentAction != null)
            {
                mCurrentAction.Cancel();
                mCurrentAction = null;
                Cursor.SetCursor(NormalMouse, NormalMouseTexOffset, CursorMode.Auto);
                return;
            }
            
            var viewHit = mRaycaster.Raycast<SelectableView>(mMousePosition);
            if (!viewHit.IsEmpty())
            {
                foreach (var view in Selected)
                {
                    if (view.OwnershipRelation != ObjectOwnershipRelation.My)
                        continue;

                    if (viewHit.Object.OwnershipRelation != ObjectOwnershipRelation.Enemy)
                        view.OnRightClick(viewHit.Object);
                    else
                        view.OnEnemyRightClick(viewHit.Object);
                }

                return;
            }

            var mapHit = mRaycaster.Raycast<MapView>(mMousePosition);
            if (!mapHit.IsEmpty())
            {
                var mapPoint = GameUtils.GetFlatPosition(mapHit.HitPoint);
                ForwardRightClick(mapPoint);
            }
        }

        public void OnBeginDrag(InputAction.CallbackContext ctx)
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (ctx.phase != InputActionPhase.Performed)
                return;

            if (mCurrentAction != null)
                return;
            
            SelectionManager.StartBoxSelection(mMousePosition);
        }

        public void OnDrop(InputAction.CallbackContext ctx)
        {
            SelectionManager.FinishBoxSelection(mShiftState, mMousePosition);
        }

        public void OnMouseMove(InputAction.CallbackContext ctx)
        {
            mMousePosition = ctx.ReadValue<Vector2>();
            SelectionManager.Update(mMousePosition, GuiRoot, GuiCanvas);

            if (mCurrentAction != null)
            {
                var mapHit = mRaycaster.Raycast<MapView>(mMousePosition);
                if (mapHit.IsEmpty())
                    return;

                var mapPoint = GameUtils.GetFlatPosition(mapHit.HitPoint);
                mCurrentAction.Move(mapPoint);
            }
        }

        void Update()
        {
            Selected.RemoveAll(view => view == null || view.gameObject == null || !view.gameObject.activeSelf);
        }
    }
}
