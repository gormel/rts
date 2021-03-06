﻿using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Assets.Core.Game;
using Assets.Utils;
using Grpc.Core;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;
using Random = UnityEngine.Random;

namespace Assets.Networking.Services
{
    class GameServiceImpl : GameService.GameServiceBase
    {
        public event Action<string, int> MessageRecived;

        private readonly Game mGame;
        private readonly IGameObjectFactory mServerFactory;
        private readonly UnitySyncContext mSyncContext;
        private readonly ConcurrentDictionary<Guid, AsyncQueue<ChatMessage>> mChatListeners = new ConcurrentDictionary<Guid, AsyncQueue<ChatMessage>>();

        public GameServiceImpl(Game game, IGameObjectFactory serverFactory, UnitySyncContext syncContext)
        {
            mGame = game;
            mServerFactory = serverFactory;
            mSyncContext = syncContext;
        }

        private GameState CollectGameState((IPlayerState player, Vector2 basePose) info, bool collectMap)
        {
            var result = new GameState();
            result.BasePos = info.basePose.ToGrpc();

            result.Player = new PlayerState()
            {
                ID = new ID() {Value = info.player.ID.ToString()},
                Money = info.player.Money
            };

            if (collectMap)
            {
                result.Map = new MapState()
                {
                    Width = mGame.Map.Width,
                    Lenght = mGame.Map.Length
                };

                for (int y = 0; y < mGame.Map.Length; y++)
                {
                    for (int x = 0; x < mGame.Map.Width; x++)
                    {
                        result.Map.Heights.Add(mGame.Map.Data.GetHeightAt(x, y));
                        result.Map.Objects.Add((int) mGame.Map.Data.GetMapObjectAt(x, y));
                    }
                }
            }

            return result;
        }

        public override async Task ListenChat(Empty request, IServerStreamWriter<ChatMessage> responseStream, ServerCallContext context)
        {
            var key = Guid.NewGuid();
            try
            {
                var queue = new AsyncQueue<ChatMessage>();
                if (!mChatListeners.TryAdd(key, queue))
                    throw new Exception("Cannot register user state listener!");

                while (true)
                {
                    var message = await queue.DequeueAsync(context.CancellationToken);
                    context.CancellationToken.ThrowIfCancellationRequested();
                    await responseStream.WriteAsync(message);
                }

            }
            catch (Exception e)
            {
                Debug.LogError(e);
                mChatListeners.TryRemove(key, out var q);
                throw;
            }
        }

        public void SendChatMessage(ChatMessage request)
        {
            MessageRecived?.Invoke(request.Nickname, request.StickerID);

            foreach (var queue in mChatListeners.Values)
                queue.Enqueue(request);
        }

        public override async Task<Empty> SendChatMessage(ChatMessage request, ServerCallContext context)
        {
            SendChatMessage(request);
            return new Empty();
        }

        public override async Task ConnectAndListenState(Empty request, IServerStreamWriter<GameState> responseStream, ServerCallContext context)
        {
            try
            {
                var player = await mSyncContext.Execute(() =>
                {
                    var pl = new Player(mServerFactory);
                    mGame.AddPlayer(pl);
                    var basePos = GameUtils.CreateBase(mGame, pl);
                    return (pl, basePos);
                });

                context.CancellationToken.ThrowIfCancellationRequested();
                await responseStream.WriteAsync(CollectGameState(player, true));

                while (true)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();
                    await responseStream.WriteAsync(CollectGameState(player, false));
                    await Task.Delay(30, context.CancellationToken);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }
    }
}
