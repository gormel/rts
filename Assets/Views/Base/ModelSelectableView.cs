using Assets.Core.GameObjects.Base;

namespace Assets.Views.Base
{
    abstract class ModelSelectableView<TOrderer, TInfo> : SelectableView 
        where TOrderer : IGameObjectOrders
        where TInfo : IGameObjectInfo
    {
        public sealed override float Health => Info.Health;
        public sealed override float MaxHealth => Info.MaxHealth;

        public TOrderer Orders { get; private set; }
        public TInfo Info { get; private set; }

        public MapView Map { get; set; }

        protected virtual void OnLoad()
        {
        }

        public void LoadModel(TOrderer orderer, TInfo info)
        {
            Orders = orderer;
            Info = info;
            OnLoad();
        }
    }
}