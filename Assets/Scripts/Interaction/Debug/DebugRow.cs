using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Core.Game;
using Assets.Core.GameObjects.Base;
using Assets.Interaction.InterfaceUtils;
using Assets.Utils;
using Assets.Views;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Interaction.Debug
{
    class DebugRow : MonoBehaviour
    {
        public TextMeshProUGUI NicknameText;
        public TextMeshProUGUI SelectedObjectText;

        private Game mGame;
        private Player mAssignedPlayer;
        private Raycaster mRaycaster;
        private RtsInputActions mInputActions;
        private Vector2 mMousePosition;
        private bool mShiftState;

        private bool mIsSpawning;

        private int mSelectedCreatorIndex;
        private static List<(string Name, Action<IGameObjectFactory, Vector2> Do)> mCreators;

        private void Awake()
        {
            mCreators = new()
            {
                ("Worker", (f, p) => Add(f.CreateWorker(p))),
                ("Artillery", (f, p) => Add(f.CreateArtillery(p))),
                ("Melee", (f, p) => Add(f.CreateMeeleeWarrior(p))),
                ("Ranged", (f, p) => Add(f.CreateRangedWarrior(p))),
                ("Command Center", (f, p) => AddB(f.CreateCentralBuilding(Floor(p)))),
                ("Barrak", (f, p) => AddB(f.CreateBarrak(Floor(p)))),
                ("Builders Lab", (f, p) => AddB(f.CreateBuildersLab(Floor(p)))),
                ("Warriors Lab", (f, p) => AddB(f.CreateWarriorsLab(Floor(p)))),
                ("Turret", (f, p) => AddB(f.CreateTurret(Floor(p)))),
                ("Mining Camp", (f, p) => AddB(f.CreateMiningCamp(Floor(p)))),
            };
            
            SelectedObjectText.text = mCreators[mSelectedCreatorIndex].Name;
            mRaycaster = new Raycaster(Camera.main);
            mInputActions = new RtsInputActions();
            mInputActions.OnScreenInteraction.MouseMove.performed += OnMouseMove;
            mInputActions.OnScreenInteraction.LeftClick.performed += OnLeftClick;
            mInputActions.OnScreenInteraction.ShiftMod.performed += OnShiftMod;
        }

        private void OnEnable()
        {
            mInputActions.Enable();
        }

        private void OnDisable()
        {
            mInputActions.Disable();
        }

        private static Vector2 Floor(Vector2 v) => new Vector2(Mathf.Floor(v.x), Mathf.Floor(v.y));

        private async Task<T> Add<T>(Task<T> created) where T : RtsGameObject
        {
            var go = await created;
            await mGame.PlaceObject(go);
            return go;
        }

        private async void AddB<T>(Task<T> created) where T : Building
        {
            var go = await Add(created);
            go.CompleteBuilding();
        }

        private void OnShiftMod(InputAction.CallbackContext obj)
        {
            mShiftState = obj.ReadValueAsButton();
        }

        private void OnLeftClick(InputAction.CallbackContext obj)
        {
            if (!mIsSpawning)
                return;
            
            var hit = mRaycaster.Raycast<MapView>(mMousePosition);
            if (hit.IsEmpty())
                return;

            var flatMapPos = GameUtils.GetFlatPosition(hit.HitPoint);
            mCreators[mSelectedCreatorIndex].Do(mAssignedPlayer, flatMapPos);

            if (!mShiftState)
                mIsSpawning = false;
        }

        private void OnMouseMove(InputAction.CallbackContext obj)
        {
            mMousePosition = obj.ReadValue<Vector2>();
        }

        public void AddResources(int amount)
        {
            mAssignedPlayer.Money.Store(amount);
        }

        public void AssignPlayer(Player player, Game game)
        {
            mGame = game;
            mAssignedPlayer = player;
            NicknameText.text = player.Nickname;
        }

        public void SelectNextObject()
        {
            mSelectedCreatorIndex = (mSelectedCreatorIndex + 1) % mCreators.Count;
            SelectedObjectText.text = mCreators[mSelectedCreatorIndex].Name;
        }

        public void BeginObjectSpawn()
        {
            mIsSpawning = true;
        }
    }
}