using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.GameObjects;
using Assets.Core.GameObjects.Base;
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
        public Color TargetLinePlaceToColor;
        protected override void OnLoad()
        {
            Updater.Register(Info.ID, () =>
            {
                gameObject.SetActive(!Info.IsAttachedToMiningCamp);
                if (Info.IsAttachedToMiningCamp && IsSelected)
                    IsSelected = false;
            });
        }

        protected override void Update()
        {
            base.Update();

            BuildingIndicator.SetActive(Info.IsBuilding);

            switch (Info.MovementType)
            {
                case WorkerMovementType.Common:
                    TargetLine.endColor = TargetLine.startColor = TargetLineMovementColor;
                    break;
                case WorkerMovementType.Attach:
                    TargetLine.endColor = TargetLine.startColor = TargetLinePlaceToColor;
                    break;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            
            Updater.Free(Info.ID);
        }

        public override void OnRightClick(SelectableView view)
        {
            if (view.InfoBase is IBuildingInfo info && info.BuildingProgress == BuildingProgress.Building)
                Orders.AttachAsBuilder(info.ID);

            if (view is MiningCampView miningCamp && miningCamp.Info.BuildingProgress == BuildingProgress.Complete) 
                Orders.AttachToMiningCamp(miningCamp.Info.ID);
        }
    }
}
