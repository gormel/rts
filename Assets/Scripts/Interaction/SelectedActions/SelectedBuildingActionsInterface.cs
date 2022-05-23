using System;
using System.Linq;
using Assets.Core.GameObjects.Base;
using UnityEngine;

namespace Assets.Interaction
{
    class SelectedBuildingActionsInterface : MonoBehaviour
    {
        public UserInterface Interface;

        public GameObject BuildingPanel;
        public GameObject CompletePanel;

        public virtual void Update()
        {
            var building = Interface.FetchSelectedInfo<IBuildingInfo>().FirstOrDefault();
            if (building == null)
                return;
            
            BuildingPanel.SetActive(building.BuildingProgress == BuildingProgress.Building);
            CompletePanel.SetActive(building.BuildingProgress == BuildingProgress.Complete);
        }

        public void CancelBuilding()
        {
            foreach (var order in Interface.FetchSelectedOrders<IBuildingOrders>())
                order.CancelBuilding();
        }
    }
}