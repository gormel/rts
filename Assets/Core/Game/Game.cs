using System;
using System.Collections.Generic;
using Assets.Core.GameObjects;
using Assets.Core.GameObjects.Base;
using UnityEngine;

namespace Assets.Core.Game
{
    class Player
    {
        public ResourceStorage Money { get; } = new ResourceStorage();
        public IGameObjectFactory GameObjectFactory { get; }
    }

    class Game
    {
        public Map.Map Map { get; }

        private IDictionary<Guid, RtsGameObject> mGameObjects = new Dictionary<Guid, RtsGameObject>();
        private ICollection<Guid> mRemoveRequested = new List<Guid>();
        private ICollection<RtsGameObject> mPlaceRequested = new List<RtsGameObject>();

        public Game()
        {
            Map = new Map.Map(50, 50);
        }

        public void PlaceObject(RtsGameObject obj)
        {
            mPlaceRequested.Add(obj);
        }

        public void RemoveObject(Guid objId)
        {
            mRemoveRequested.Add(objId);
        }

        public void Update(TimeSpan elapsed)
        {
            foreach (var o in mGameObjects.Values)
                o.Update(elapsed);

            foreach (var id in mRemoveRequested)
            {
                RtsGameObject gameObject;
                if (mGameObjects.TryGetValue(id, out gameObject))
                {
                    gameObject.ID = Guid.Empty;
                    mGameObjects.Remove(id);
                }
            }

            mRemoveRequested.Clear();

            foreach (var gameObject in mPlaceRequested)
            {
                var id = Guid.NewGuid();
                gameObject.ID = id;
                mGameObjects.Add(id, gameObject);
            }

            mPlaceRequested.Clear();
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

            return Map.GetIsAreaFree(position, size);
        }
    }
}
