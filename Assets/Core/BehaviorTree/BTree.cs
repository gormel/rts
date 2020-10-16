using System;
using System.Collections.Generic;
using System.Linq;
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
        IBTreeBuilder Inverter(Func<IBTreeBuilder, IBTreeBuilder> children);
        IBTreeBuilder Succeder(Func<IBTreeBuilder, IBTreeBuilder> children);
        IBTreeBuilder Repeater(Func<IBTreeBuilder, IBTreeBuilder> children);
        IBTreeBuilder RepeatUntilFail(Func<IBTreeBuilder, IBTreeBuilder> children);
        IBTreeBuilder Leaf(IBTreeLeaf leaf);
        BTree Build();
    }

    class BTree
    {
        private abstract class Node
        {
            public Node()
            {
                
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
                throw new NotImplementedException();
            }

            public IBTreeBuilder Selector(Func<IBTreeBuilder, IBTreeBuilder> children)
            {
                throw new NotImplementedException();
            }

            public IBTreeBuilder Inverter(Func<IBTreeBuilder, IBTreeBuilder> children)
            {
                throw new NotImplementedException();
            }

            public IBTreeBuilder Succeder(Func<IBTreeBuilder, IBTreeBuilder> children)
            {
                throw new NotImplementedException();
            }

            public IBTreeBuilder Repeater(Func<IBTreeBuilder, IBTreeBuilder> children)
            {
                throw new NotImplementedException();
            }

            public IBTreeBuilder RepeatUntilFail(Func<IBTreeBuilder, IBTreeBuilder> children)
            {
                throw new NotImplementedException();
            }

            public IBTreeBuilder Leaf(IBTreeLeaf leaf)
            {
                throw new NotImplementedException();
            }

            private List<Node> BuildInner()
            {
                return null;
            }

            public BTree Build()
            {
                return null;
            }
        }

        private BTree(Node root)
        {
            
        }

        public static IBTreeBuilder Build()
        {
            return null;
        }

        public void Update(TimeSpan deltaTime)
        {
        }
    }
}
