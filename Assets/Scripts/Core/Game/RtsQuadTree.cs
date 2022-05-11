using System;
using System.Collections.Generic;
using System.Linq;
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
        private Stack<Node> mQueryNodes = new();

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

            for (var index = 0; index < nodes.Count; index++)
            {
                var node = nodes[index];
                node.Objects.Remove(obj);
            }

            mContainingNodes.Remove(obj.ID);
        }

        public void Clear()
        {
            mRoot = new Node(mInitPosition, mInitSize);
        }

        public bool Any(Rect box)
        {
            mQueryNodes.Clear();
            mQueryNodes.Push(mRoot);

            while (mQueryNodes.Count > 0)
            {
                var root = mQueryNodes.Pop();
                
                if (root == null || !box.Overlaps(root.Rect))
                    continue;

                for (var index = 0; index < root.Objects.Count; index++)
                {
                    var gameObject = root.Objects[index];
                    if (gameObject.Overlaps(box))
                        return true;
                }
                
                mQueryNodes.Push(root.LeftUp);
                mQueryNodes.Push(root.RightUp);
                mQueryNodes.Push(root.LeftDown);
                mQueryNodes.Push(root.RightDown);
            }

            return false;
        }

        public void QueryNoAlloc(Vector2 center, float radius, ICollection<RtsGameObject> result)
        {
            mQueryNodes.Clear();
            mQueryNodes.Push(mRoot);

            while (mQueryNodes.Count > 0)
            {
                var root = mQueryNodes.Pop();
                
                if (root == null || PositionUtils.DistanceTo(root.Rect.center, root.Rect.size, center) > radius)
                    continue;
                
                for (var index = 0; index < root.Objects.Count; index++)
                {
                    var gameObject = root.Objects[index];
                    if (gameObject.Overlaps(center, radius))
                        result.Add(gameObject);
                }
                
                mQueryNodes.Push(root.LeftUp);
                mQueryNodes.Push(root.RightUp);
                mQueryNodes.Push(root.LeftDown);
                mQueryNodes.Push(root.RightDown);
            }
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