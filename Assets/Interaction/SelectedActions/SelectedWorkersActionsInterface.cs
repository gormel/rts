using System;
using Assets.Core.GameObjects;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using Assets.Views;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Interaction
{
    sealed class SelectedWorkersActionsInterface : SelectedUnitsActionsInterface
    {
        public GameObject StartScreen;
        public Button PlaceTurretButton;

        public Text CentralBuildingCostText;
        public Text BarrakCostText;
        public Text TurretCostText;
        public Text BuildersLabCostText;
        public Text WarriorsLabCostText;
        public Text MiningCampCostText;
        
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
                MiningCamp.BuildingSize
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

            var player = Interface.Root.Player;
            
            CentralBuildingCostText.text = player.CentralBuildingCost.ToString();
            BarrakCostText.text = player.BarrakCost.ToString();
            TurretCostText.text = player.TurretCost.ToString();
            BuildersLabCostText.text = player.BuildersLabCost.ToString();
            WarriorsLabCostText.text = player.WarriorsLabCost.ToString();
            MiningCampCostText.text = player.MiningCampCost.ToString();
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