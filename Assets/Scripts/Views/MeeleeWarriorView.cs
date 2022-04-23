using Assets.Core.GameObjects.Final;
using Assets.Views.Base;
using UnityEngine;

namespace Assets.Views
{
    sealed class MeeleeWarriorView : WarriorUnitView<IMeeleeWarriorOrders, IMeeleeWarriorInfo>
    {
        public override string Name => "Колотитель";
    }
}