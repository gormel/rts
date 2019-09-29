using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Assets.Utils;
using Grpc.Core;
using UnityEngine;

namespace Assets.Networking.Lobby
{
    class LobbyServiceImpl : LobbyService.LobbyServiceBase
    {
        private readonly string mHostID;
        private ConcurrentDictionary<string, TaskCompletionSource<bool>> mStartRequests = new ConcurrentDictionary<string, TaskCompletionSource<bool>>();
        private ConcurrentDictionary<string, AsyncQueue<UserState>> mUserStateRequests = new ConcurrentDictionary<string, AsyncQueue<UserState>>();
        public event Action<string, bool> OnUserStateChanged;

        public LobbyServiceImpl(string hostID)
        {
            mHostID = hostID;
        }

        public void StartGame()
        {
            RespStartRequests(true);
        }

        private void RespStartRequests(bool result)
        {
            foreach (var key in mStartRequests.Keys.ToArray())
            {
                if (mStartRequests.TryGetValue(key, out var tcs))
                {
                    tcs.TrySetResult(result);
                }
            }
        }

        private void ReportUserState(string id, bool state)
        {
            OnUserStateChanged?.Invoke(id, state);

            foreach (var queue in mUserStateRequests.Values)
            {
                queue.Enqueue(new UserState { ID = id, Connected = state });
            }
        }

        public void Leave()
        {
            RespStartRequests(false);
        }

        public override async Task ListenStart(UserState request, IServerStreamWriter<StartState> responseStream, ServerCallContext context)
        {
            if (mStartRequests.ContainsKey(request.ID))
                throw new AuthenticationException("This nickname already busy");

            ReportUserState(request.ID, true);

            try
            {
                var tcs = new TaskCompletionSource<bool>();
                using (context.CancellationToken.Register(() =>
                {
                    tcs.SetCanceled();
                    mStartRequests.TryRemove(request.ID, out var tcs1);
                }))
                {
                    mStartRequests.TryAdd(request.ID, tcs);
                    var started = await tcs.Task;
                    await responseStream.WriteAsync(new StartState { Start = started });
                }
            }
            catch(Exception ex)
            {
                Debug.LogError(ex);
                ReportUserState(request.ID, false);
                throw;
            }
        }

        public override async Task ListenUserState(Empty request, IServerStreamWriter<UserState> responseStream, ServerCallContext context)
        {
            var key = Guid.NewGuid().ToString();
            try
            {
                var queue = new AsyncQueue<UserState>();
                if (!mUserStateRequests.TryAdd(key, queue))
                    throw new Exception("Cannot register user state listener!");

                foreach (var startRequestsKey in mStartRequests.Keys)
                    await responseStream.WriteAsync(new UserState {ID = startRequestsKey, Connected = true});

                await responseStream.WriteAsync(new UserState { ID = mHostID, Connected = true });

                while (true)
                {
                    var state = await queue.DequeueAsync(context.CancellationToken);
                    context.CancellationToken.ThrowIfCancellationRequested();
                    await responseStream.WriteAsync(state);
                }

            }
            catch (Exception e)
            {
                Debug.LogError(e);
                mUserStateRequests.TryRemove(key, out var q);
                throw;
            }
        }
    }
}
