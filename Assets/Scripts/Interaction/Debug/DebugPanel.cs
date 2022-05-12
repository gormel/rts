﻿using System.Collections.Generic;
using Assets.Core.Game;
using UnityEngine;

namespace Interaction.Debug
{
    class DebugPanel : MonoBehaviour
    {
        public GameObject RowPrefub;
        public GameObject RowContainer;

        public void ApplyPlayers(IEnumerable<Player> players)
        {
            foreach (var player in players)
            {
                var rowInst = Instantiate(RowPrefub, RowContainer.transform, false);
                var row = rowInst.GetComponent<DebugRow>();
                row.AssignPlayer(player);
            }
        }
    }
}