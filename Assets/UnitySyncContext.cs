using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

class UnitySyncContext : MonoBehaviour
{
    private struct Execution
    {
        public Action Action { get; }
        public TaskCompletionSource<bool> TaskSource { get; }

        public Execution(Action action)
        {
            Action = action;
            TaskSource = new TaskCompletionSource<bool>();
        }
    }

    private ConcurrentQueue<Execution> mExecutions = new ConcurrentQueue<Execution>();

    public Task Execute(Action action, CancellationToken token = default)
    {
        var exec = new Execution(action);
        mExecutions.Enqueue(exec);
        token.Register(() => exec.TaskSource.TrySetCanceled(token));
        return exec.TaskSource.Task;
    }

    public async Task<T> Execute<T>(Func<T> func, CancellationToken token = default)
    {
        var result = default(T);
        await Execute(() => { result = func(); }, token);
        return result;
    }

    void Update()
    {
        while (mExecutions.Count > 0)
        {
            Execution toExec;
            if (mExecutions.TryDequeue(out toExec))
            {
                try
                {
                    toExec.Action();
                    toExec.TaskSource.SetResult(true);
                }
                catch (Exception ex)
                {
                    toExec.TaskSource.TrySetException(ex);
                }

            }
        }
    }
}