using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Assets.Views;
using UnityEngine;

namespace Assets.Interaction
{
    class SelectedViewActionsInterface : MonoBehaviour
    {
        public GameObject WorkerActions;
        public GameObject CentralBuildingActions;
        public GameObject BuildingTemplateActions;
        public GameObject BarrakActions;

        public UserInterface Interface;

        void Update()
        {
            var workerActionsActive = false;
            var centralBuildingActive = false;
            var buildingTemplateActions = false;
            var barrakActionsActive = false;
            try
            {
                if (Interface.Selected.Count < 1)
                    return;
                
                var firstSelected = Interface.Selected[0];
                if (firstSelected is WorkerView)
                    workerActionsActive = true;

                if (firstSelected is CentralBuildingView)
                    centralBuildingActive = true;

                if (firstSelected is BuildingTemplateView)
                    buildingTemplateActions = true;

                if (firstSelected is BarrakView)
                    barrakActionsActive = true;
            }
            finally
            {
                WorkerActions.SetActive(workerActionsActive);
                CentralBuildingActions.SetActive(centralBuildingActive);
                BuildingTemplateActions.SetActive(buildingTemplateActions);
                BarrakActions.SetActive(barrakActionsActive);
            }
        }
    }
}
