﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.GameObjects;
using Assets.Core.GameObjects.Final;
using Assets.Core.GameObjects.Utils;
using Assets.Utils;
using Assets.Views.Base;
using Assets.Views.Utils;
using UnityEngine;

namespace Assets.Views
{
    sealed class CentralBuildingView : FactoryBuildingView<ICentralBuildingOrders, ICentralBuildingInfo>
    {
        public override string Name => "Главное здание";

        protected override void OnLoad()
        {
            RegisterProperty(new SelectableViewProperty("Current progress", () => $"{Info.Progress * 100:#0}%"));
            RegisterProperty(new SelectableViewProperty("Queued workers", () => $"{Info.Queued}"));
        }

        public override void OnRightClick(Vector2 position)
        {
            Orders.SetWaypoint(position);
        }
    }
}
