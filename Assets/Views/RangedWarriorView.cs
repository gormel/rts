using Assets.Core.GameObjects.Final;
using Assets.Views.Base;
using Assets.Views.Utils;

namespace Assets.Views {
    sealed class RangedWarriorView : UnitView<IRangedWarriorOrders, IRangedWarriorInfo>
    {
        public override string Name => "Стрелятель";

        void Start()
        {
            OnStart();
        }

        void Update()
        {
            OnUpdate();
        }

        void LateUpdate()
        {
            OnLateUpdate();
        }
    }
}