﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Utils
{
    public class AsyncQueue<T>
    {
        private ConcurrentQueue<T> _bufferQueue;
        private ConcurrentQueue<TaskCompletionSource<T>> _promisesQueue;
        private object _syncRoot = new object();

        /// <summary>
        /// Gets a value indicating whether this instance has promises.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has promises; otherwise, <c>false</c>.
        /// </value>
        public bool HasPromises
        {
            get { return _promisesQueue.Any(p => !p.Task.IsCanceled); }
        }
        
        public AsyncQueue()
        {
            _bufferQueue = new ConcurrentQueue<T>();
            _promisesQueue = new ConcurrentQueue<TaskCompletionSource<T>>();
        }

        /// <summary>
        /// Enqueues the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Enqueue(T item)
        {
            TaskCompletionSource<T> promise;
            do
            {
                if (_promisesQueue.TryDequeue(out promise) &&
                    !promise.Task.IsCanceled &&
                    promise.TrySetResult(item))
                {
                    return;
                }
            }
            while (promise != null);

            lock (_syncRoot)
            {
                if (_promisesQueue.TryDequeue(out promise) &&
                    !promise.Task.IsCanceled &&
                    promise.TrySetResult(item))
                {
                    return;
                }

                _bufferQueue.Enqueue(item);
            }
        }

        /// <summary>
        /// Dequeues the asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public Task<T> DequeueAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            T item;

            if (!_bufferQueue.TryDequeue(out item))
            {
                lock (_syncRoot)
                {
                    if (!_bufferQueue.TryDequeue(out item))
                    {
                        var promise = new TaskCompletionSource<T>();
                        var registration = cancellationToken.Register(() => promise.TrySetCanceled());

                        _promisesQueue.Enqueue(promise);

                        return promise.Task.ContinueWith(t =>
                        {
                            registration.Dispose();
                            if (t.IsFaulted)
                                throw t.Exception;

                            if (t.IsCanceled)
                                throw new OperationCanceledException();
                            
                            return t.Result;
                        });
                    }
                }
            }

            return Task.FromResult(item);
        }
    }
}
