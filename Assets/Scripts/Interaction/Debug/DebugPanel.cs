using System.Collections.Generic;
using Assets.Core.Game;
using UnityEngine;

namespace Interaction.Debug
{
    class DebugPanel : MonoBehaviour
    {
        public GameObject RowPrefub;
        public GameObject RowContainer;
        public GameObject FogOfWar;

        public void ApplyPlayers(IEnumerable<Player> players, Game game)
        {
            foreach (var player in players)
            {
                var rowInst = Instantiate(RowPrefub, RowContainer.transform, false);
                var row = rowInst.GetComponent<DebugRow>();
                row.AssignPlayer(player, game);
            }
        }

        public void MapHack(bool show)
        {
            FogOfWar.SetActive(!show);
        }
    }
}