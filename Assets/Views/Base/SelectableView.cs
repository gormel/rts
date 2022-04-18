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
        interface IValueWatcher
        {
            void Update();
        }
        class ValueWatcher<T> : IValueWatcher where T : IComparable<T>
        {
            private readonly Func<T> mGetValue;
            private readonly T mBorder;
            private readonly Action<T> mOnMore;
            private readonly Action<T> mOnLess;
            private readonly Action<T> mOnEqual;
            private T mLastValue;
            public ValueWatcher(Func<T> getValue, T border, Action<T> onMore, Action<T> onLess, Action<T> onEqual)
            {
                mGetValue = getValue;
                mBorder = border;
                mOnMore = onMore;
                mOnLess = onLess;
                mOnEqual = onEqual;
                mLastValue = mGetValue();
            }

            public void Update()
            {
                var value = mGetValue();
                var lastComp = mLastValue.CompareTo(mBorder);
                var comp = value.CompareTo(mBorder);
                
                if (comp < 0 && lastComp >= 0)
                    mOnLess?.Invoke(value);
                else if (comp == 0 && lastComp != 0)
                    mOnEqual?.Invoke(value);
                else if (comp > 0 && lastComp <= 0)
                    mOnMore?.Invoke(value);
                
                mLastValue = value;
            }
        }
        
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
                for (int i = 0; i < Mathf.Min(MaterialTarget.Length, MyMaterial.Length, AllyMaterial.Length, EnemyMaterial.Length); i++)
                {
                    if (MaterialTarget[i] == null)
                        continue;
                    
                    MaterialTarget[i].sharedMaterial = 
                        mOwnershipRelation == ObjectOwnershipRelation.My ? MyMaterial[i] :
                        mOwnershipRelation == ObjectOwnershipRelation.Ally ? AllyMaterial[i] :
                        mOwnershipRelation == ObjectOwnershipRelation.Enemy ? EnemyMaterial[i]
                            : null;
                }

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
        public Renderer[] MaterialTarget;

        public ProgressBar HpBar;

        public UnitySyncContext SyncContext { get; set; }
        public ExternalUpdater Updater { get; set; }

        public int SelectionPriority;

        private List<IValueWatcher> mValueWatchers = new List<IValueWatcher>();
        private ObjectOwnershipRelation mOwnershipRelation;

        protected void WatchMore<T>(Func<T> getValue, T border, Action<T> onMore) where T : IComparable<T>
        {
            mValueWatchers.Add(new ValueWatcher<T>(getValue, border, onMore, null, null));
        }

        protected void WatchLess<T>(Func<T> getValue, T border, Action<T> onLess) where T : IComparable<T>
        {
            mValueWatchers.Add(new ValueWatcher<T>(getValue, border, null, onLess, null));
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
            foreach (var watcher in mValueWatchers)
            {
                watcher.Update();
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