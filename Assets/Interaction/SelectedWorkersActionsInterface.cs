using Assets.Core.GameObjects;
using Assets.Core.GameObjects.Final;
using Assets.Views;
using UnityEngine;

namespace Assets.Interaction
{
    sealed class SelectedWorkersActionsInterface : SelectedUnitsActionsInterface<Worker, WorkerView>
    {
        public GameObject StartScreen;

        public void StartCentralBuildingPlacement()
        {
            Interface.BeginBuildingPlacement(SelectedViews, (view, position) => 
                view.PlaceCentralBuilding(new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y))),
                CentralBuilding.BuildingSize
                );
        }

        void OnEnable()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                child.gameObject.SetActive(false);
            }

            StartScreen.SetActive(true);
        }
    }
}