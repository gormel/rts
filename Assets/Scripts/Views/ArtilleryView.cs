using Assets.Views.Base;
using Core.GameObjects.Final;

namespace Assets.Views
{
    class ArtilleryView : UnitView<IArtilleryOrders, IArtilleryInfo>
    {
        public override string Name => "Артелерия";
    }
}