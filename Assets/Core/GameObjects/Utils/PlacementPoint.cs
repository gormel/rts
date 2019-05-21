using UnityEngine;

namespace Assets.Core.GameObjects.Utils
{
    struct PlacementPoint
    {
        public int ID { get; }
        public Vector2 Position { get; }

        public PlacementPoint(int id, Vector2 position)
        {
            ID = id;
            Position = position;
        }
    }
}