using System;

namespace Assets.Core.GameObjects.Utils
{
    abstract class UnitOrder
    {
        public bool Active { get; private set; }

        public void Begin()
        {
            Active = true;
            OnBegin();
        }

        protected void End()
        {
            Active = false;
        }

        public void Cancel()
        {
            OnCancel();
            End();
        }

        public void Update(TimeSpan deltaTime)
        {
            if (!Active)
                return;

            OnUpdate(deltaTime);
        }

        protected abstract void OnBegin();
        protected abstract void OnUpdate(TimeSpan deltaTime);
        protected abstract void OnCancel();
    }
}