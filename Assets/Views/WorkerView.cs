﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.GameObjects;
using Assets.Core.GameObjects.Final;
using Assets.Views.Base;
using Assets.Views.Utils;
using UnityEngine;

namespace Assets.Views
{
    sealed class WorkerView : UnitView<IWorkerOrders, IWorkerInfo>
    {
        public override string Name => "Рабочий";

        public GameObject BuildingIndicator;

        protected override void OnLoad()
        {
            RegisterProperty(new SelectableViewProperty("X position", () => Info.Position.x.ToString("##.##")));
            RegisterProperty(new SelectableViewProperty("Y position", () => Info.Position.y.ToString("##.##")));
        }

        protected override void Update()
        {
            base.Update();

            BuildingIndicator.SetActive(Info.IsBuilding);
        }

        public override void OnRightClick(SelectableView view)
        {
            if (view is BuildingTemplateView)
                Orders.AttachAsBuilder(((BuildingTemplateView)view).Info.ID);
        }
    }
}
