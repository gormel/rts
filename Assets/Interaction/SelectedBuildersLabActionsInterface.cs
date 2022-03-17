﻿using System;
using System.Linq;
using Assets.Core.GameObjects.Base;
using Assets.Views;
using UnityEngine;

namespace Assets.Interaction
{
    sealed class SelectedBuildersLabActionsInterface : MonoBehaviour
    {
        public UserInterface Interface;

        public GameObject QueueAttackUpgradeButton;
        public GameObject QueueDefenceUpgradeButton;

        private void Update()
        {
            var player = Interface.Root.Player;
            QueueAttackUpgradeButton.SetActive(player.TurretAttackUpgradeAvaliable);
            QueueDefenceUpgradeButton.SetActive(player.BuildingDefenceUpgradeAvaliable);
        }

        public void QueueAttackUpgrade()
        {
            foreach (var view in Interface.Selected.OfType<BuildersLabView>())
            {
                view.Orders.QueueAttackUpgrade();
                break;
            }
        }

        public void QueueDefenceUpgrade()
        {
            foreach (var view in Interface.Selected.OfType<BuildersLabView>())
            {
                view.Orders.QueueDefenceUpgrade();
                break;
            }
        }

        public void CancelUpgrade()
        {
            foreach (var view in Interface.Selected.OfType<BuildersLabView>())
            {
                view.Orders.CancelResearch();
            }
        }
    }
}