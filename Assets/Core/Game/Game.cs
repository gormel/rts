using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assets.Core.GameObjects;
using Assets.Core.GameObjects.Base;
using Assets.Core.Map;
using UnityEngine;

namespace Assets.Core.Game
{
    class Game
    {
        public Map.Map Map { get; }

        private IDictionary<Guid, RtsGameObject> mGameObjects = new Dictionary<Guid, RtsGameObject>();
        private ConcurrentDictionary<Guid, Action> mRequested = new ConcurrentDictionary<Guid, Action>();
        private ICollection<Player> mPlayers = new List<Player>();

        public Game()
        {
            Map = new Map.Map(70, 70);
        }

        public void AddPlayer(Player player)
        {
            mPlayers.Add(player);
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
