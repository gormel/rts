using System.Collections.Generic;
using UnityEngine;

namespace Assets.Interaction
{
    abstract class CollectionInterface : MonoBehaviour
    {
        public GameObject ElementPrefab;
        protected abstract int ElementCount { get; }
        private List<GameObject> mCachedElements = new List<GameObject>();
        public int MaximumElements = int.MaxValue;

        protected abstract void UpdateChild(int index, GameObject child);

        protected virtual void OnUpdate()
        {
            var index = 0;
            for (; index < Mathf.Min(ElementCount, MaximumElements); index++)
            {
                if (index > mCachedElements.Count - 1)
                {
                    mCachedElements.Add(Instantiate(ElementPrefab));
                    mCachedElements[index].transform.SetParent(transform);
                }

                var inst = mCachedElements[index];
                inst.SetActive(true);
                UpdateChild(index, inst);
            }

            for (int i = index; i < mCachedElements.Count; i++)
            {
                mCachedElements[i].SetActive(false);
            }
        }
    }
}