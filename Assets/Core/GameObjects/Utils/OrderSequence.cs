using System;
using System.Collections.Generic;

namespace Assets.Core.GameObjects.Utils
{
    class OrderSequence : UnitOrder
    {
        private Queue<UnitOrder> mSequence;
        public OrderSequence(params UnitOrder[] orders)
        {
            mSequence = new Queue<UnitOrder>(orders);
        }

        protected override void OnBegin()
        {
            mSequence.Peek().Begin();
        }

        protected override void OnUpdate(TimeSpan deltaTime)
        {
            if (mSequence.Count == 0)
            {
                End();
                return;
            }

            if (!mSequence.Peek().Active)
            {
                mSequence.Dequeue();
                if (mSequence.Count > 0)
                    mSequence.Peek().Begin();

                return;
            }

            mSequence.Peek().Update(deltaTime);
        }

        protected override void OnCancel()
        {
            foreach (var order in mSequence)
            {
                order.Cancel();
            }
        }
    }
}