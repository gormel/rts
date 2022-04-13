using System;
using System.Collections.Generic;

namespace Assets.Core.GameObjects.Utils
{
    public class IndexedQueue<T>
    {
        private List<T> mContainer = new List<T>();
        public int Count => mContainer.Count;
        
        public void Enqueue(T value)
        {
            mContainer.Add(value);
        }

        public T Dequeue()
        {
            if (mContainer.Count < 1)
                throw new ArgumentException("Queue is empty");

            var result = mContainer[0];
            mContainer.RemoveAt(0);
            return result;
        }

        public bool TryRemoveAt(int index, out T value)
        {
            if (index < 0 || index >= mContainer.Count)
            {
                value = default;
                return false;
            }

            value = mContainer[index];
            mContainer.RemoveAt(index);
            return true;
        }
    }
}