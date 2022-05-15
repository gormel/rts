using System;
using System.Linq;
using Core.GameObjects.Final;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Interaction
{
    sealed class SelectedArtilleryActionsInterface : SelectedUnitsActionsInterface
    {
        public CooldownIndicator LaunchMissileCooldownIndicator;
        
        private void Update()
        {
            var info = Interface.FetchSelectedInfo<IArtilleryInfo>().FirstOrDefault();
            if (info != null)
            {
                LaunchMissileCooldownIndicator.SetProgress((float) (info.LaunchCooldown / Artillery.LaunchCooldownTime.TotalSeconds));
            }
        }

        public void BeginLaunchMissile()
        {
            Interface.BeginLaunchMissile(Interface.FetchSelectedOrders<IArtilleryOrders>());
        }
    }
}