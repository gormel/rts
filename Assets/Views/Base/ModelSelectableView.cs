using System;
using System.Collections;
using Assets.Core.GameObjects.Base;
using UnityEngine;

namespace Assets.Views.Base
{
    abstract class ModelSelectableView<TOrderer, TInfo> : SelectableView , IInfoIdProvider
        where TOrderer : IGameObjectOrders
        where TInfo : IGameObjectInfo
    {
        public sealed override float Health => Info.Health;
        public sealed override float MaxHealth => Info.MaxHealth;

        public TOrderer Orders { get; private set; }
        public TInfo Info { get; private set; }

        public MapView Map { get; set; }

        Guid IInfoIdProvider.ID => Info.ID;

        public GameObject ExplosionPrefab;

        protected virtual void OnLoad()
        {
        }

        protected virtual Vector2 Position => Info.Position;

        public virtual void OnDestroy()
        {
            if (Map.isActiveAndEnabled)
                Map.StartCoroutine(ShowExplosion());
        }

        private IEnumerator ShowExplosion()
        {
            if (ExplosionPrefab == null)
                yield break;

            var inst = Instantiate(ExplosionPrefab);
            inst.transform.parent = Map.transform;
            inst.transform.localPosition = Map.GetWorldPosition(Position);

            yield return new WaitForSeconds(2);

            Destroy(inst);
        }

        public void LoadModel(TOrderer orderer, TInfo info)
        {
            Orders = orderer;
            Info = info;
            OnLoad();
        }
    }
}