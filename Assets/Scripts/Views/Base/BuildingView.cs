using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using Assets.Views.Utils;
using UnityEngine;

namespace Assets.Views.Base
{
    abstract class BuildingView<TOrderer, TInfo> : ModelSelectableView<TOrderer, TInfo>
        where TOrderer : IBuildingOrders
        where TInfo : IBuildingInfo
    {
        protected override Vector2 Position => Info.Position + Info.Size / 2;

        public PlacementService PlacementService;

        public GameObject BuildingStateModel;
        public GameObject CompleteStateModel;

        protected override void OnLoad()
        {
            base.OnLoad();
            PlacementService.SyncContext = SyncContext;
        }

        protected override void Update()
        {
            base.Update();
            
            BuildingStateModel.SetActive(Info.BuildingProgress == BuildingProgress.Building);
            CompleteStateModel.SetActive(Info.BuildingProgress == BuildingProgress.Complete);
        }
    }
}
