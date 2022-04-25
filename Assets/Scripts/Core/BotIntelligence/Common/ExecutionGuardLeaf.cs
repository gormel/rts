using System;
using Assets.Core.BehaviorTree;

namespace Core.BotIntelligence
{
    enum ExecutionGuardMode
    {
        Wait,
        Release,
    }
    
    class ExecutionGuardLeaf : IBTreeLeaf
    {
        private readonly ExecutionGuardFastMemory mMemory;
        private readonly int mMaxExecutions;
        private readonly ExecutionGuardMode mMode;

        public ExecutionGuardLeaf(ExecutionGuardFastMemory memory, int maxExecutions, ExecutionGuardMode mode)
        {
            mMemory = memory;
            mMaxExecutions = maxExecutions;
            mMode = mode;
        }
        public BTreeLeafState Update(TimeSpan deltaTime)
        {
            switch (mMode)
            {
                case ExecutionGuardMode.Wait:
                    if (mMemory.Executions >= mMaxExecutions)
                        return BTreeLeafState.Failed;
                    mMemory.Executions++;
                    return BTreeLeafState.Successed;
                case ExecutionGuardMode.Release:
                    mMemory.Executions--;
                    break;
            }

            return BTreeLeafState.Successed;
        }
    }
}