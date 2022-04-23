using System;
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
    sealed class BuildingTemplateView : PlacementServiceBuildingView<IBuildingTemplateOrders, IBuildingTemplateInfo>
    {
        public override string Name => "Строительство";

        public override Rect FlatBounds => new Rect(Info.Position, Info.Size);
    }
}
