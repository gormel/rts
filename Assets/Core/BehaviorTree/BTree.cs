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

            public Builder(Builder parent)
            {
                mParent = parent;
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

            private Node[] BuildInner()
            {
            }

            public BTree Build()
            {
            }
        }

        private BTree(Node root)
        {
            
        }

        public static IBTreeBuilder Build()
        {
        }

        public void Update(TimeSpan deltaTime)
        {
        }
    }
}
