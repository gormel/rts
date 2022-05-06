using System;
using System.Collections;
using Assets.Core.GameObjects.Base;
using UnityEngine;

namespace Assets.Views.Base
{
    abstract class ModelSelectableView<TOrderer, TInfo> : SelectableView
        where TOrderer : IGameObjectOrders
        where TInfo : IGameObjectInfo
    {
        public sealed override float Health => Info.MaxHealth - Info.RecivedDamage;
        public sealed override float MaxHealth => Info.MaxHealth;

        public TOrderer Orders { get; private set; }
        public TInfo Info { get; private set; }

        public override IGameObjectOrders OrdersBase => Orders;
        public override IGameObjectInfo InfoBase => Info;

        public MapView Map { get; set; }

        public GameObject ExplosionPrefab;
        public GameObject MinimapBrush;

        protected virtual void OnLoad()
        {
        }

        protected virtual Vector2 Position => Info.Position;

        public virtual void OnDestroy()
        {
            var inst = Instantiate(ExplosionPrefab, Map.transform, true);
            inst.transform.localPosition = Map.GetWorldPosition(Position);
        }

        public void LoadModel(TOrderer orderer, TInfo info)
        {
            Orders = orderer;
            Info = info;
            OnLoad();
        }

        protected override void Update()
        {
            base.Update();

            MinimapBrush.transform.localScale = new Vector3(Info.ViewRadius * 2, 1, Info.ViewRadius * 2);
        }
    }
}