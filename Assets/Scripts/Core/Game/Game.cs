﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assets.Core.GameObjects;
using Assets.Core.GameObjects.Base;
using Assets.Core.Map;
using Core.BotIntelligence;
using UnityEngine;

namespace Assets.Core.Game
{
    class Game
    {
        public Map.Map Map { get; }

        private IDictionary<Guid, RtsGameObject> mGameObjects = new Dictionary<Guid, RtsGameObject>();
        private ConcurrentDictionary<Guid, Action> mRequested = new();
        private Dictionary<Guid, BotPlayer> mBotPlayers = new();
        private Dictionary<Guid, Player> mPlayers = new();

        public Game()
        {
            Map = new Map.Map(70, 70);
        }

        public Task<Guid> PlaceObject(RtsGameObject obj)
        {
            return AddRequest<Guid>(tcs => {
                mGameObjects.Add(obj.ID, obj);
                obj.OnAddedToGame();
                tcs.SetResult(obj.ID);
            });
        }

        private Task<T> AddRequest<T>(Action<TaskCompletionSource<T>> action)
        {
            var requestId = Guid.NewGuid();
            var tcs = new TaskCompletionSource<T>();

            if (!mRequested.TryAdd(requestId, () => action(tcs)))
                throw new Exception("Cannot add game request");

            return tcs.Task;
        }

        public IEnumerable<T> RequestPlayerObjects<T>(Player player) where T : RtsGameObject
        {
            foreach (var gameObject in mGameObjects.Values.ToList())
            {
                if (gameObject.PlayerID == player.ID && gameObject is T rtsGameObject)
                    yield return rtsGameObject;
            }
        }

        public IEnumerable<Player> GetPlayers() => mPlayers.Values.Concat(mBotPlayers.Values);

        public Task<RtsGameObject> RemoveObject(Guid objId)
        {
            return AddRequest<RtsGameObject>(tcs =>
            {
                RtsGameObject obj;
                if (!mGameObjects.TryGetValue(objId, out obj))
                {
                    tcs.SetException(new ArgumentException("There are no object with this ID."));
                }
                else
                {
                    mGameObjects.Remove(objId);
                    obj?.OnRemovedFromGame();
                    tcs.SetResult(obj);
                }
            });
        }

        public void AddBotPlayer(BotPlayer bot) =>
            mBotPlayers.Add(bot.ID, bot);
        
        public void AddPlayer(Player player) =>
            mPlayers.Add(player.ID, player);

        public Player GetPlayer(Guid playerId)
        {
            if (mPlayers.TryGetValue(playerId, out var pl))
                return pl;

            if (mBotPlayers.TryGetValue(playerId, out var pl1))
                return pl1;

            throw new ArgumentException("Player does not exist.", nameof(playerId));
        }

        public T GetObject<T>(Guid objectId) where T : RtsGameObject
        {
            RtsGameObject result;
            if (!mGameObjects.TryGetValue(objectId, out result))
                throw new ArgumentException("There are no object with this ID.");

            if (!(result is T))
                throw new ArgumentException("Object type does not match.");

            return (T)result;
        }

        public void Update(TimeSpan elapsed)
        {
            foreach (var o in mGameObjects.Values)
                o.Update(elapsed);

            var requestKeys = mRequested.Keys.ToList();
            foreach (var key in requestKeys.ToList())
            {
                if (mRequested.TryRemove(key, out var request))
                    request.Invoke();
            }

            foreach (var player in mBotPlayers.Values) 
                player.Update(elapsed);
        }

        public IEnumerable<RtsGameObject> QueryObjects(Vector2 position, float radius)
        {
            foreach (var gameObject in mGameObjects.Values.ToList())
            {
                if (Vector2.Distance(position, gameObject.Position) < radius)
                {
                    yield return gameObject;
                    continue;
                }

                if (gameObject is Building)
                {
                    var rect = new Rect(gameObject.Position, ((Building) gameObject).Size);
                    var test = position;

                    if (position.x < rect.min.x)
                        test.x = rect.min.x;
                    else if (position.x > rect.max.x)
                        test.x = rect.max.x;
                    if (position.y < rect.min.y)
                        test.y = rect.min.y;
                    else if (position.y > rect.max.y)
                        test.y = rect.max.y;


                    if (Vector2.Distance(test, position) < radius)
                    {
                        yield return gameObject;
                    }
                }
            }
        }

        public bool GetIsAreaFree(Vector2 position, Vector2 size)
        {
            var rect = new Rect(position, size);
            foreach (var gameObject in mGameObjects.Values)
            {
                if (rect.Contains(gameObject.Position))
                    return false;

                if (gameObject is Building)
                {
                    if (rect.Overlaps(new Rect(gameObject.Position, ((Building) gameObject).Size)))
                        return false;
                }
            }

            return Map.Data.GetIsAreaFree(position, size);
        }
    }
}