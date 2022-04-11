using System;
using System.Collections.Generic;
using Assets.Core.GameObjects.Base;
using Assets.Views.Utils;
using UnityEngine;

namespace Assets.Views.Base
{
    interface IFlatBoundsOwner
    {
        Rect FlatBounds { get; }
    }

    enum ObjectOwnershipRelation
    {
        My,
        Ally,
        Enemy
    }
    abstract class SelectableView : MonoBehaviour, IFlatBoundsOwner
    {
        public Sprite Icon;
        public abstract string Name { get; }
        public abstract float Health { get; }
        public abstract float MaxHealth { get; }
        public abstract Rect FlatBounds { get; }

        public abstract IGameObjectInfo InfoBase { get; }
        public abstract IGameObjectOrders OrdersBase { get; }

        private bool mIsSelected;

        public bool IsSelected
        {
            get { return mIsSelected; }
            set
            {
                SelectionObject.SetActive(value);
                mIsSelected = value;
            }
        }

        public bool IsMouseOver { get; set; }

        public ObjectOwnershipRelation OwnershipRelation
        {
            get => mOwnershipRelation;
            set
            {
                mOwnershipRelation = value;
                for (int i = 0; i < MaterialTarget.Length; i++)
                    MaterialTarget[i].sharedMaterial = 
                        mOwnershipRelation == ObjectOwnershipRelation.My ? MyMaterial[i] :
                        mOwnershipRelation == ObjectOwnershipRelation.Ally ? AllyMaterial[i] :
                        mOwnershipRelation == ObjectOwnershipRelation.Enemy ? EnemyMaterial[i]
                            : null;

                FogOfWarBrush.SetActive(mOwnershipRelation != ObjectOwnershipRelation.Enemy);
            }
        }

        public bool IsClient { get; set; }

        public GameObject FogOfWarBrush;
        public GameObject SelectionObject;
        public GameObject MouseOverObject;
        public Material[] AllyMaterial;
        public Material[] MyMaterial;
        public Material[] EnemyMaterial;
        public MeshRenderer[] MaterialTarget;

        public ProgressBar HpBar;

        public UnitySyncContext SyncContext { get; set; }
        public ExternalUpdater Updater { get; set; }

        public int SelectionPriority;

        private List<SelectableViewProperty> mProperties = new List<SelectableViewProperty>();
        private ObjectOwnershipRelation mOwnershipRelation;
        public IReadOnlyList<SelectableViewProperty> Properties => mProperties;

        protected void RegisterProperty(SelectableViewProperty prop)
        {
            mProperties.Add(prop);
        }

        public virtual void OnRightClick(Vector2 position)
        {
        }

        public virtual void OnRightClick(SelectableView view)
        {
        }

        public virtual void OnEnemyRightClick(SelectableView view)
        {
        }

        protected virtual void Update()
        {
            foreach (var property in mProperties)
            {
                property.Update();
            }

            if (HpBar != null)
            {
                HpBar.Progress = Health / MaxHealth;
                HpBar.gameObject.SetActive(HpBar.Progress < 1 || IsSelected);
            }

            if (MouseOverObject != null)
                MouseOverObject.SetActive(IsMouseOver && !IsSelected);
        }
    }
}