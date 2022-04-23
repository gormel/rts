using System;
using System.Threading.Tasks;
using Assets.Core.BehaviorTree;

namespace Core.BotIntelligence
{
    class ExecuteOrderLeaf<TMemory, TObject> : IBTreeLeaf
    {
        private readonly TMemory mMemory;
        private readonly Func<TMemory, TObject> mSelectObject;
        private readonly Func<TObject, TMemory, Task> mDoOrder;

        private bool mInProcess;
        private bool mDone;
        
        public ExecuteOrderLeaf(TMemory memory, Func<TMemory, TObject> selectObject, Func<TObject, TMemory, Task> doOrder)
        {
            mMemory = memory;
            mSelectObject = selectObject;
            mDoOrder = doOrder;
        }
        public BTreeLeafState Update(TimeSpan deltaTime)
        {
            if (mInProcess)
                return BTreeLeafState.Processing;

            if (mDone)
            {
                mDone = false;
                return BTreeLeafState.Successed;
            }

            var obj = mSelectObject(mMemory);
            if (obj == null)
                return BTreeLeafState.Failed;
            
            mInProcess = true;
            mDoOrder(obj, mMemory).ContinueWith(_ =>
            {
                mInProcess = false;
                mDone = true;
            });
            
            return BTreeLeafState.Processing;
        }
    }
}