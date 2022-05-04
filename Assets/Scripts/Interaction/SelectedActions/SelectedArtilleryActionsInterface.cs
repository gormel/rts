using System;
using System.Linq;
using Core.GameObjects.Final;
using UnityEngine.UI;

namespace Assets.Interaction
{
    sealed class SelectedArtilleryActionsInterface : SelectedUnitsActionsInterface
    {
        public Button LaunchMissileButton;

        private void Update()
        {
            var firstInfo = Interface.FetchSelectedInfo<IArtilleryInfo>().FirstOrDefault();
            if (firstInfo == null)
                return;

            LaunchMissileButton.interactable = firstInfo.LaunchAvaliable;
        }

        public void BeginLaunchMissile()
        {
            Interface.BeginLaunchMissile(Interface.FetchSelectedOrders<IArtilleryOrders>());
        }
    }
}