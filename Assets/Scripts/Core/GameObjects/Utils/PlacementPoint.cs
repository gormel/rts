using UnityEngine;

namespace Assets.Core.GameObjects.Utils
{
    struct PlacementPoint
    {
        public static readonly PlacementPoint Invalid = new PlacementPoint(-1, Vector2.zero);

        public int ID { get; }
        public Vector2 Position { get; }

        public PlacementPoint(int id, Vector2 position)
        {
            ID = id;
            Position = position;
        }

        public static bool operator ==(PlacementPoint a, PlacementPoint b)
        {
            return a.ID == b.ID;
        }

        public static bool operator !=(PlacementPoint a, PlacementPoint b)
        {
            return a.ID != b.ID;
        }

        public bool Equals(PlacementPoint other)
        {
            return ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is PlacementPoint other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ID * 397) ^ Position.GetHashCode();
            }
        }
    }
}