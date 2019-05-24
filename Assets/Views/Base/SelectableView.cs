using System;
using System.Collections.Generic;
using Assets.Views.Utils;
using UnityEngine;

namespace Assets.Views.Base
{
    abstract class SelectableView : MonoBehaviour
    {
        public Sprite Icon;
        public abstract string Name { get; }
        public abstract float Health { get; }
        public abstract float MaxHealth { get; }

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

        public bool IsControlable { get; set; }

        public GameObject SelectionObject;

        private List<SelectableViewProperty> mProperties = new List<SelectableViewProperty>();
        public IReadOnlyList<SelectableViewProperty> Properties => mProperties;

        private Queue<Action> mExecutions = new Queue<Action>();

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

        protected void UpdateExecutions()
        {
            while (mExecutions.Count > 0)
            {
                mExecutions.Dequeue()();
            }
        }

        protected void Execute(Action action)
        {
            mExecutions.Enqueue(action);
        }

        protected void UpdateProperties()
        {
            foreach (var property in mProperties)
            {
                property.Update();
            }
        }
    }
}