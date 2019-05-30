using System;
using System.Threading.Tasks;

namespace Assets.Core.GameObjects.Utils
{
    enum OrderState
    {
        Uninitialized,
        Initialization,
        Work,
        Completed
    }
    abstract class UnitOrder
    {
        public UnitOrder Next { get; private set; }
        public OrderState State { get; private set; } = OrderState.Uninitialized;

        protected void End()
        {
            State = OrderState.Completed;
        }

        public void Cancel()
        {
            if (State != OrderState.Completed)
            {
                OnCancel();
                End();
            }
        }

        public void Update(TimeSpan deltaTime)
        {
            switch (State)
            {
                case OrderState.Uninitialized:
                    State = OrderState.Initialization;
                    OnBegin().ContinueWith(t => { State = OrderState.Work; });
                    break;
                case OrderState.Work:
                    OnUpdate(deltaTime);
                    break;
            }
        }

        protected abstract Task OnBegin();
        protected abstract void OnUpdate(TimeSpan deltaTime);
        protected abstract void OnCancel();

        public UnitOrder Then(UnitOrder next)
        {
            Next = next;
            return this;
        }
    }
}