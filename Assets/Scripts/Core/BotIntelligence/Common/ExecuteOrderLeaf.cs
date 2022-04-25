using System;
using System.Threading.Tasks;
using Assets.Core.BehaviorTree;

namespace Core.BotIntelligence
{
    abstract class ExecuteTaskLeaf : IBTreeLeaf
    {
        private bool mInProcess;
        private bool mDone;
        private BTreeLeafState mDoneState;

        protected abstract Task<BTreeLeafState> GetTask();
        
        public BTreeLeafState Update(TimeSpan deltaTime)
        {
            if (mInProcess)
                return BTreeLeafState.Processing;

            if (mDone)
            {
                mDone = false;
                return mDoneState;
            }
            
            mInProcess = true;
            GetTask().ContinueWith(t =>
            {
                if (t.IsFaulted || t.IsCanceled)
                    mDoneState = BTreeLeafState.Failed;
                else
                    mDoneState = t.Result;
                
                mInProcess = false;
                mDone = true;
            });
            
            return BTreeLeafState.Processing;
        }
    }
    
    abstract class ExecuteOrderLeaf<TMemory, TObject> : ExecuteTaskLeaf
    {
        private readonly TMemory mMemory;
        
        protected ExecuteOrderLeaf(TMemory memory)
        {
            mMemory = memory;
        }

        protected abstract TObject SelectObject(TMemory memory);
        protected abstract Task ExecuteOrder(TObject obj, TMemory memory);

        protected sealed override async Task<BTreeLeafState> GetTask()
        {
            var obj = SelectObject(mMemory);
            if (obj == null)
                return BTreeLeafState.Failed;
            await ExecuteOrder(obj, mMemory);
            return BTreeLeafState.Successed;
        }
    }
}