using Assets.Core.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Interaction.Debug
{
    class DebugRow : MonoBehaviour
    {
        public TextMeshProUGUI NicknameText;

        private Player mAssignedPlayer;
        
        public void AddResources(int amount)
        {
            mAssignedPlayer.Money.Store(amount);
        }

        public void AssignPlayer(Player player)
        {
            mAssignedPlayer = player;
            NicknameText.text = player.Nickname;
        }
    }
}