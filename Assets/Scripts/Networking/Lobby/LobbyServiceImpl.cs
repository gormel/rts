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
        private int mHostTeam;
        
        private ConcurrentDictionary<string, TaskCompletionSource<bool>> mStartRequests = new ConcurrentDictionary<string, TaskCompletionSource<bool>>();
        private ConcurrentDictionary<string, AsyncQueue<UserState>> mUserStateRequests = new ConcurrentDictionary<string, AsyncQueue<UserState>>();
        private ConcurrentDictionary<string, UserState> mActiveUsers = new ConcurrentDictionary<string, UserState>();
        public event Action<UserState> OnUserStateChanged;

        public LobbyServiceImpl(string hostID, int team)
        {
            mHostID = hostID;
            mHostTeam = team;
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

        public void SetHostTeam(int team)
        {
            mHostTeam = team;
            ReportUserState(new UserState
            {
                Connected = true,
                ID = mHostID,
                Team = mHostTeam,
            });
        }

        private void ReportUserState(UserState state)
        {
            OnUserStateChanged?.Invoke(state);

            foreach (var queue in mUserStateRequests.Values)
            {
                queue.Enqueue(state.Clone());
            }

            if (state.Connected)
                GameUtils.RegistredPlayers.AddOrUpdate(state.ID, state.Team, (n, t) => state.Team);
            else
                GameUtils.RegistredPlayers.TryRemove(state.ID, out _);
        }

        public void Leave()
        {
            RespStartRequests(false);
        }

        public override async Task ListenStart(UserState request, IServerStreamWriter<StartState> responseStream, ServerCallContext context)
        {
            if (mStartRequests.ContainsKey(request.ID) || request.ID == mHostID)
                throw new AuthenticationException("This nickname already busy");

            ReportUserState(request);

            try
            {
                var tcs = new TaskCompletionSource<bool>();
                using (context.CancellationToken.Register(() =>
                       {
                           tcs.SetCanceled();
                       }))
                {
                    mStartRequests.TryAdd(request.ID, tcs);
                    mActiveUsers.AddOrUpdate(request.ID, request, (id, req) => request);
                    var started = await tcs.Task;

                    await responseStream.WriteAsync(new StartState
                    {
                        Start = started,
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                var reported = request.Clone();
                reported.Connected = false;
                ReportUserState(reported);
                throw;
            }
            finally
            {
                mStartRequests.TryRemove(request.ID, out _);
                mActiveUsers.TryRemove(request.ID, out _);
            }
        }

        public override async Task<Empty> UpdateState(UserState request, ServerCallContext context)
        {
            ReportUserState(request);
            return new Empty();
        }

        public override async Task ListenUserState(Empty request, IServerStreamWriter<UserState> responseStream, ServerCallContext context)
        {
            var key = Guid.NewGuid().ToString();
            try
            {
                var queue = new AsyncQueue<UserState>();
                if (!mUserStateRequests.TryAdd(key, queue))
                    throw new Exception("Cannot register user state listener!");

                await responseStream.WriteAsync(new UserState { ID = mHostID, Connected = true, Team = mHostTeam });

                foreach (var activeUser in mActiveUsers)
                    await responseStream.WriteAsync(new UserState { ID = activeUser.Key, Connected = true, Team = activeUser.Value.Team });

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
