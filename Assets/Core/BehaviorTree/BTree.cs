using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Core.BehaviorTree
{
    enum BTreeLeafState
    {
        Failed,
        Processing,
        Successed
    }

    interface IBTreeLeaf
    {
        BTreeLeafState Update(TimeSpan deltaTime);
    }

    interface IBTreeBuilder
    {
        IBTreeBuilder Sequence(Func<IBTreeBuilder, IBTreeBuilder> children);
        IBTreeBuilder Selector(Func<IBTreeBuilder, IBTreeBuilder> children);
        IBTreeBuilder Leaf(IBTreeLeaf leaf);
        BTree Build();
    }

    class BTree
    {
        private readonly Node mRoot;

        private abstract class Node
        {
            public Node[] Children { get; }

            public Node(Node[] children)
            {
                Children = children ?? new Node[0];
            }

            public abstract BTreeLeafState Update(TimeSpan deltaTime);
        }

        private class SquenceNode : Node
        {
            public SquenceNode(Node[] children)
                : base(children)
            {
            }

            public override BTreeLeafState Update(TimeSpan deltaTime)
            {
                for (int i = 0; i < Children.Length; i++)
                {
                    var updateResult = Children[i].Update(deltaTime);
                    if (updateResult == BTreeLeafState.Processing || updateResult == BTreeLeafState.Failed)
                        return updateResult;
                }

                return BTreeLeafState.Successed;
            }
        }

        private class SelectorNode : Node
        {
            public SelectorNode(Node[] children)
                : base(children)
            {
            }

            public override BTreeLeafState Update(TimeSpan deltaTime)
            {
                for (int i = 0; i < Children.Length; i++)
                {
                    var updateResult = Children[i].Update(deltaTime);
                    if (updateResult == BTreeLeafState.Processing || updateResult == BTreeLeafState.Successed)
                        return updateResult;
                }

                return BTreeLeafState.Failed;
            }
        }

        private class LeafNode : Node
        {
            private readonly IBTreeLeaf mLeaf;

            public LeafNode(Node[] children, IBTreeLeaf leaf)
                : base(children)
            {
                mLeaf = leaf;
            }

            public override BTreeLeafState Update(TimeSpan deltaTime)
            {
                if (mLeaf == null)
                    return BTreeLeafState.Failed;

                return mLeaf.Update(deltaTime);
            }
        }

        private class Builder : IBTreeBuilder
        {
            private readonly Builder mParent;
            private readonly Func<Node> mCreateNode;

            public Builder(Builder parent, Func<Node> createNode)
            {
                mParent = parent;
                mCreateNode = createNode;
            }

            public IBTreeBuilder Sequence(Func<IBTreeBuilder, IBTreeBuilder> children)
            {
                var childrenBuilder = new Builder(null, null);
                return new Builder(this, () => new SquenceNode((children(childrenBuilder) as Builder)?.BuildInner()?.ToArray()));
            }

            public IBTreeBuilder Selector(Func<IBTreeBuilder, IBTreeBuilder> children)
            {
                var childrenBuilder = new Builder(null, null);
                return new Builder(this, () => new SelectorNode((children(childrenBuilder) as Builder)?.BuildInner()?.ToArray()));
            }

            public IBTreeBuilder Leaf(IBTreeLeaf leaf)
            {
                var childrenBuilder = new Builder(null, null);
                return new Builder(this, () => new LeafNode(null, leaf));
            }

            private List<Node> BuildInner()
            {
                var nodes = new List<Node>();
                if (mParent != null)
                    nodes.AddRange(mParent.BuildInner());

                if (mCreateNode != null)
                    nodes.Add(mCreateNode());

                return nodes;
            }

            public BTree Build()
            {
                return new BTree(BuildInner().Single());
            }
        }

        private BTree(Node root)
        {
            mRoot = root;
        }

        public static IBTreeBuilder Build()
        {
            return new Builder(null, null);
        }

        public BTreeLeafState Update(TimeSpan deltaTime)
        {
            return mRoot.Update(deltaTime);
        }
    }
}
