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
        private readonly int mMaxPlayers;

        private ConcurrentDictionary<string, TaskCompletionSource<bool>> mStartRequests = new();
        private ConcurrentDictionary<string, AsyncQueue<UserState>> mUserStateRequests = new();
        private ConcurrentDictionary<string, UserState> mActiveUsers = new();
        private ConcurrentDictionary<string, UserState> mBotUsers = new();
        public event Action<UserState> OnUserStateChanged;

        public LobbyServiceImpl(string hostID, int team, int maxPlayers)
        {
            mHostID = hostID;
            mHostTeam = team;
            mMaxPlayers = maxPlayers;
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
                IsBot = false,
            });
        }

        public void AddBot()
        {
            if (mBotUsers.Count + mActiveUsers.Count + 1 >= mMaxPlayers)
                return;
            
            var numbers = mBotUsers.Select(b => int.Parse(b.Key.Substring(4))).ToArray();
            var idx = 0;
            if (numbers.Length > 0)
                idx = numbers.Max() + 1;
            var nick = $"Bot_{idx}";
            var botState = new UserState()
            {
                ID = nick,
                Connected = true,
                Team = 1,
                IsBot = true,
            };
            mBotUsers.AddOrUpdate(nick, n => botState, (n, b) => botState);
            ReportUserState(botState);
        }

        public void RemoveBot(string botId)
        {
            if (mBotUsers.TryRemove(botId, out var botState))
            {
                botState.Connected = false;
                ReportUserState(botState);
            }
        }

        public void SetBotTeam(string botId, int team)
        {
            if (mBotUsers.TryGetValue(botId, out var botState))
            {
                botState.Team = team;
                mBotUsers.AddOrUpdate(botId, botState, (id, s) => botState);
                ReportUserState(botState);
            }
        }

        private void ReportUserState(UserState state)
        {
            OnUserStateChanged?.Invoke(state);

            foreach (var queue in mUserStateRequests.Values)
            {
                queue.Enqueue(state.Clone());
            }
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
            mActiveUsers.AddOrUpdate(request.ID, _ => request, (_, _) => request);
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

                await responseStream.WriteAsync(new UserState { ID = mHostID, Connected = true, Team = mHostTeam, IsBot = false });

                foreach (var activeUser in mActiveUsers)
                    await responseStream.WriteAsync(new UserState { ID = activeUser.Key, Connected = true, Team = activeUser.Value.Team, IsBot = false });

                foreach (var botUser in mBotUsers)
                    await responseStream.WriteAsync(new UserState { ID = botUser.Key, Connected = true, Team = botUser.Value.Team, IsBot = true });

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
