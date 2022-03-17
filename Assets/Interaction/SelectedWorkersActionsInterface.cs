using Assets.Core.GameObjects;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using Assets.Views;
using UnityEngine;

namespace Assets.Interaction
{
    sealed class SelectedWorkersActionsInterface : SelectedUnitsActionsInterface
    {
        public GameObject StartScreen;

        public void StartCentralBuildingPlacement()
        {
            Interface.BeginBuildingPlacement(FetchSelectedOrders<IWorkerOrders>(), (view, position) => 
                view.PlaceCentralBuildingTemplate(new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y))),
                CentralBuilding.BuildingSize
                );
        }

        public void StartMiningCampPlacement()
        {
            Interface.BeginBuildingPlacement(FetchSelectedOrders<IWorkerOrders>(), (view, position) =>
                view.PlaceMiningCampTemplate(new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y))),
                MiningCamp.BuildingSize
                );
        }

        public void StartBarrakPlacement()
        {
            Interface.BeginBuildingPlacement(FetchSelectedOrders<IWorkerOrders>(), (view, position) => 
                view.PlaceBarrakTemplate(new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y))),
                Barrak.BuildingSize
                );
        }

        public void StartTurretPlacement()
        {
            Interface.BeginBuildingPlacement(FetchSelectedOrders<IWorkerOrders>(), (view, position) => 
                view.PlaceTurretTemplate(new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y))),
                Turret.BuildingSize
            );
        }

        public void StartBuildersLabPlacement()
        {
            Interface.BeginBuildingPlacement(FetchSelectedOrders<IWorkerOrders>(), (view, position) => 
                view.PlaceBuildersLabTemplate(new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y))),
                BuildersLab.BuildingSize
            );
        }

        public void AttachToMiningCamp()
        {
            Interface.BeginAttachWorkerToMiningCamp(FetchSelectedOrders<IWorkerOrders>());
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