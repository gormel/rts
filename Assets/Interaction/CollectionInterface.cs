using UnityEngine;

namespace Assets.Interaction
{
    abstract class CollectionInterface : MonoBehaviour
    {
        public GameObject ElementPrefab;

        protected abstract int ElementCount { get; }

        protected abstract void UpdateChild(int index, GameObject child);

        protected virtual void OnUpdate()
        {
            var index = 0;
            for (; index < ElementCount; index++)
            {
                var inst = index < transform.childCount ? transform.GetChild(index).gameObject : Instantiate(ElementPrefab);
                UpdateChild(index, inst);

                inst.transform.SetParent(transform);
            }

            while (index < transform.childCount)
            {
                var tr = transform.GetChild(transform.childCount - 1);
                Destroy(tr.gameObject);
                tr.SetParent(null, false);
            }
        }
    }
}