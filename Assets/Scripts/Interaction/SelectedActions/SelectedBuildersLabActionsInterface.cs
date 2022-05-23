using System;
using System.Linq;
using Assets.Core.GameObjects.Base;
using Assets.Views;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Interaction
{
    sealed class SelectedBuildersLabActionsInterface : SelectedBuildingActionsInterface
    {
        public GameObject QueueAttackUpgradeButton;
        public GameObject QueueDefenceUpgradeButton;

        public TextMeshProUGUI QueueAttackUpgradeText;
        public TextMeshProUGUI QueueDefenceUpgradeText;

        public override void Update()
        {
            base.Update();
            var player = Interface.Root.Player;
            QueueAttackUpgradeButton.SetActive(player.TurretAttackUpgradeAvaliable);
            QueueDefenceUpgradeButton.SetActive(player.BuildingDefenceUpgradeAvaliable);
            
            QueueAttackUpgradeText.text = BuildersLab.TurretAttackUpgradeCost.ToString();
            QueueDefenceUpgradeText.text = BuildersLab.BuildingDefenceUpgradeCost.ToString();
        }

        public void QueueAttackUpgrade()
        {
            foreach (var view in Interface.Selected.OfType<BuildersLabView>().OrderBy(v => v.Info.Queued))
            {
                view.Orders.QueueAttackUpgrade();
                break;
            }
        }

        public void QueueDefenceUpgrade()
        {
            foreach (var view in Interface.Selected.OfType<BuildersLabView>().OrderBy(v => v.Info.Queued))
            {
                view.Orders.QueueDefenceUpgrade();
                break;
            }
        }
    }
}