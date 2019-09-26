using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using UnityEngine;

namespace Assets.Views.Base
{
    abstract class BuildingView<TOrderer, TInfo> : ModelSelectableView<TOrderer, TInfo>
        where TOrderer : IBuildingOrders
        where TInfo : IBuildingInfo
    {
        protected override Vector2 Position => Info.Position + Info.Size / 2;
    }
}
