﻿using System.Linq;
using Assets.Views;
using UnityEngine;

namespace Assets.Interaction
{
    class SelectedMiningCampActionsInterface : MonoBehaviour
    {
        public UserInterface Interface;
        public void FreeWorker()
        {
            var views = Interface.Selected.OfType<MiningCampView>();
            foreach (var campView in views)
            {
                campView.Orders.FreeWorker();
            }
        }
    }
}