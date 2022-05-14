using System;
using Assets.Core.GameObjects;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using Assets.Views;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Interaction
{
    sealed class SelectedWorkersActionsInterface : SelectedUnitsActionsInterface
    {
        public GameObject StartScreen;
        public Button PlaceTurretButton;
        public Button PlaceWarriorsLabButton;

        public TextMeshProUGUI CentralBuildingCostText;
        public TextMeshProUGUI BarrakCostText;
        public TextMeshProUGUI TurretCostText;
        public TextMeshProUGUI BuildersLabCostText;
        public TextMeshProUGUI WarriorsLabCostText;
        public TextMeshProUGUI MiningCampCostText;
        
        public void StartCentralBuildingPlacement()
        {
            Interface.BeginBuildingPlacement(Interface.FetchSelectedOrders<IWorkerOrders>(), (view, position) => 
                view.PlaceCentralBuildingTemplate(new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y))),
                CentralBuilding.BuildingSize
                );
        }

        public void StartMiningCampPlacement()
        {
            Interface.BeginBuildingPlacement(Interface.FetchSelectedOrders<IWorkerOrders>(), (view, position) =>
                view.PlaceMiningCampTemplate(new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y))),
                MiningCamp.BuildingSize, pos => Interface.Root.MapView.IsMiningCampPlacementAllowed(pos)
                );
        }

        public void StartBarrakPlacement()
        {
            Interface.BeginBuildingPlacement(Interface.FetchSelectedOrders<IWorkerOrders>(), (view, position) => 
                view.PlaceBarrakTemplate(new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y))),
                Barrak.BuildingSize
                );
        }

        public void StartTurretPlacement()
        {
            Interface.BeginBuildingPlacement(Interface.FetchSelectedOrders<IWorkerOrders>(), (view, position) => 
                view.PlaceTurretTemplate(new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y))),
                Turret.BuildingSize
            );
        }

        public void StartBuildersLabPlacement()
        {
            Interface.BeginBuildingPlacement(Interface.FetchSelectedOrders<IWorkerOrders>(), (view, position) => 
                view.PlaceBuildersLabTemplate(new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y))),
                BuildersLab.BuildingSize
            );
        }

        public void StartWarriorsLabPlacement()
        {
            Interface.BeginBuildingPlacement(Interface.FetchSelectedOrders<IWorkerOrders>(), (view, position) => 
                view.PlaceWarriorsLabTemplate(new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y))),
                WarriorsLab.BuildingSize
            );
        }

        public void AttachToMiningCamp()
        {
            Interface.BeginAttachWorkerToMiningCamp(Interface.FetchSelectedOrders<IWorkerOrders>());
        }

        private void Update()
        {
            PlaceTurretButton.interactable = Interface.Root.Player.TurretBuildingAvaliable;
            PlaceWarriorsLabButton.interactable = Interface.Root.Player.WarriorsLabBuildingAvaliable;

            CentralBuildingCostText.text = Worker.CentralBuildingCost.ToString();
            BarrakCostText.text = Worker.BarrakCost.ToString();
            TurretCostText.text = Worker.TurretCost.ToString();
            BuildersLabCostText.text = Worker.BuildersLabCost.ToString();
            WarriorsLabCostText.text = Worker.WarriorsLabCost.ToString();
            MiningCampCostText.text = Worker.MiningCampCost.ToString();
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