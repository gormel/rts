using Assets.Core.GameObjects.Final;
using Assets.Views.Base;
using Assets.Views.Utils;
using UnityEngine;

namespace Assets.Views {
    sealed class RangedWarriorView : WarriorUnitView<IRangedWarriorOrders, IRangedWarriorInfo>
    {
        public override string Name => "Стрелятель";
    }
}