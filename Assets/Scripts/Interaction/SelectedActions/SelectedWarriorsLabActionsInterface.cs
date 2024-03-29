using System.Linq;
using Assets.Core.GameObjects.Base;
using Assets.Views;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Interaction
{
    sealed class SelectedWarriorsLabActionsInterface : SelectedBuildingActionsInterface
    {
        public GameObject DamageUpgradeButton;
        public GameObject ArmourUpgradeButton;
        public GameObject AttackRangeUpgradeButton;

        public TextMeshProUGUI DamageUpgradeCostText;
        public TextMeshProUGUI ArmourUpgradeCostText;
        public TextMeshProUGUI AttackRangeUpgradeCostText;

        public override void Update()
        {
            base.Update();
            var player = Interface.Root.Player;
            DamageUpgradeButton.gameObject.SetActive(player.UnitDamageUpgradeAvaliable);
            ArmourUpgradeButton.gameObject.SetActive(player.UnitArmourUpgradeAvaliable);
            AttackRangeUpgradeButton.gameObject.SetActive(player.UnitAttackRangeUpgradeAvaliable);

            DamageUpgradeCostText.text = WarriorsLab.UnitDamageUpgradeCost.ToString();
            ArmourUpgradeCostText.text = WarriorsLab.UnitArmourUpgradeCost.ToString();
            AttackRangeUpgradeCostText.text = WarriorsLab.UnitAttackRangeUpgradeCost.ToString();
        }

        public void QueueDamageUpgrade()
        {
            foreach (var view in Interface.Selected.OfType<WarriorsLabView>().OrderBy(v => v.Info.Queued))
            {
                view.Orders.QueueDamageUpgrade();
                break;
            }
        }

        public void QueueArmourUpgrade()
        {
            foreach (var view in Interface.Selected.OfType<WarriorsLabView>().OrderBy(v => v.Info.Queued))
            {
                view.Orders.QueueArmourUpgrade();
                break;
            }
        }

        public void QueueAttackRangeUpgrade()
        {
            foreach (var view in Interface.Selected.OfType<WarriorsLabView>().OrderBy(v => v.Info.Queued))
            {
                view.Orders.QueueAttackRangeUpgrade();
                break;
            }
        }
    }
}