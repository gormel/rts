using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Utils
{
    class AsyncDictionary<TKey, TValue>
    {
        private readonly object mLocker = new object();
        private readonly Dictionary<TKey, TaskCompletionSource<TValue>> mTaskSources = new Dictionary<TKey, TaskCompletionSource<TValue>>();

        public IReadOnlyCollection<TKey> Keys
        {
            get
            {
                lock(mLocker)
                    return new List<TKey>(mTaskSources.Keys);
            }
        } 

        public void AddOrUpdate(TKey key, TValue value)
        {
            TaskCompletionSource<TValue> taskSource;

            lock(mLocker)
            {
                if (!mTaskSources.TryGetValue(key, out taskSource) || taskSource.Task.IsCanceled || taskSource.Task.IsFaulted || taskSource.Task.IsCompleted)
                {
                    taskSource = new TaskCompletionSource<TValue>();
                    mTaskSources[key] = taskSource;
                    taskSource.SetResult(value);
                    return;
                }
            }

            taskSource.SetResult(value);
        }

        public void Remove(TKey key)
        {
            lock (mLocker)
                mTaskSources.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default(TValue);

            TaskCompletionSource<TValue> taskSource;
            lock (mLocker)
            {
                if (!mTaskSources.TryGetValue(key, out taskSource))
                    return false;
            }

            if (!taskSource.Task.IsCompleted)
                return false;

            value = taskSource.Task.Result;
            return true;
        }

        public Task<TValue> GetValueAsync(TKey key, CancellationToken token = default(CancellationToken))
        {
            TaskCompletionSource<TValue> taskSource;
            lock (mLocker)
            {
                if (!mTaskSources.TryGetValue(key, out taskSource))
                    taskSource = mTaskSources[key] = new TaskCompletionSource<TValue>();
            }
            token.Register(() => taskSource.SetCanceled());
            return taskSource.Task;
        }
    }
}