using System;
using System.Collections.Generic;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Utils;
using UnityEngine;

namespace Assets.Core.Game
{
    class RtsQuadTree
    {
        class Node
        {
            public List<RtsGameObject> Objects { get; } = new();
            
            public Vector2 Position { get; }
            public Vector2 Size { get; }
            
            public Rect Rect { get; }
            
            public Node LeftUp { get; private set; }
            public Node RightUp { get; private set; }
            public Node LeftDown { get; private set; }
            public Node RightDown { get; private set; }

            public Node(Vector2 position, Vector2 size)
            {
                Position = position;
                Size = size;
                Rect = new Rect(Position, Size);
            }

            public void Deep()
            {
                var deepSize = Size / 2;
                LeftUp = new Node(Position, deepSize);
                RightUp = new Node(Position + new Vector2(deepSize.x, 0), deepSize);
                LeftDown = new Node(Position + new Vector2(0, deepSize.y), deepSize);
                RightDown = new Node(Position + deepSize, deepSize);
            }
        }
        
        private readonly int mChunkSize;
        private readonly Vector2 mInitPosition;
        private readonly Vector2 mInitSize;

        private Node mRoot;
        private Dictionary<Guid, List<Node>> mContainingNodes = new();

        public RtsQuadTree(int chunkSize, Vector2 initPosition, Vector2 initSize)
        {
            mChunkSize = chunkSize;
            mInitPosition = initPosition;
            mInitSize = initSize;
            mRoot = new Node(initPosition, initSize);
        }

        public void Add(RtsGameObject obj)
        {
            Add(mRoot, obj);
        }

        public void Remove(RtsGameObject obj)
        {
            if (!mContainingNodes.TryGetValue(obj.ID, out var nodes))
                return;
            
            foreach (var node in nodes) 
                node.Objects.Remove(obj);

            mContainingNodes.Remove(obj.ID);
        }

        public void Clear()
        {
            mRoot = new Node(mInitPosition, mInitSize);
        }

        public bool Any(Rect box)
        {
            return Any(box, mRoot);
        }

        private bool Any(Rect box, Node root)
        {
            if (root == null || !box.Overlaps(root.Rect))
                return false;

            foreach (var gameObject in root.Objects)
                if (gameObject.Overlaps(box))
                    return true;

            return Any(box, root.LeftUp) || 
                   Any(box, root.RightUp) || 
                   Any(box, root.LeftDown) || 
                   Any(box, root.RightDown);
        }

        private void Query(Vector2 center, float radius, Node root, ICollection<RtsGameObject> result)
        {
            if (root == null || !Overlaps(root.Rect, center, radius))
                return;

            foreach (var gameObject in root.Objects)
                if (gameObject.Overlaps(center, radius))
                    result.Add(gameObject);

            Query(center, radius, root.LeftUp, result);
            Query(center, radius, root.RightUp, result);
            Query(center, radius, root.LeftDown, result);
            Query(center, radius, root.RightDown, result);
        }
        
        
        public void QueryNoAlloc(Vector2 center, float radius, ICollection<RtsGameObject> result)
        {
            Query(center, radius, mRoot, result);
        }

        private bool Overlaps(Rect rect, Vector2 center, float radius)
        {
            return rect.DistanceTo(center) <= radius;
        }

        private void Add(Node root, RtsGameObject obj)
        {
            if (!obj.Overlaps(root.Rect))
                return;
            
            if (root.Objects.Count < mChunkSize)
            {
                root.Objects.Add(obj);
                List<Node> cache;
                if (!mContainingNodes.TryGetValue(obj.ID, out cache))
                    cache = mContainingNodes[obj.ID] = new List<Node>();
                
                cache.Add(root);
                return;
            }
            
            if (root.LeftUp == null)
                root.Deep();
            
            Add(root.LeftUp, obj);
            Add(root.RightUp, obj);
            Add(root.LeftDown, obj);
            Add(root.RightDown, obj);
        }
    }
}