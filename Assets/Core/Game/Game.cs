using System;
using System.Collections.Generic;
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
        private ICollection<Action> mRequested = new List<Action>();
        private ICollection<Player> mPlayers = new List<Player>();

        public Game()
        {
            Map = new Map.Map(50, 50);
        }

        public void AddPlayer(Player player)
        {
            mPlayers.Add(player);
        }

        public Task<Guid> PlaceObject(RtsGameObject obj)
        {
            var tcs = new TaskCompletionSource<Guid>();
            mRequested.Add(() =>
            {
                mGameObjects.Add(obj.ID, obj);
                obj.OnAddedToGame();
                tcs.SetResult(obj.ID);
            });

            return tcs.Task;
        }

        public Task<RtsGameObject> RemoveObject(Guid objId)
        {
            var tcs = new TaskCompletionSource<RtsGameObject>();
            mRequested.Add(() =>
            {
                RtsGameObject obj;
                if (!mGameObjects.TryGetValue(objId, out obj))
                    tcs.SetException(new ArgumentException("There are no object with this ID."));

                mGameObjects.Remove(objId);
                obj?.OnRemovedFromGame();
                tcs.SetResult(obj);
            });

            return tcs.Task;
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

            foreach (var request in mRequested)
                request();

            mRequested.Clear();
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
