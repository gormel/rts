using System;

namespace Core.Projectiles
{
    abstract class Projectile
    {
        private readonly float mSpeed;
        private readonly float mPathLenght;

        private float mPosition = 0;
        
        public bool Complete { get; private set; }

        protected Projectile(float speed, float pathLenght)
        {
            mSpeed = speed;
            mPathLenght = pathLenght;
        }
        
        public void Update(TimeSpan deltaTime)
        {
            if (Complete)
                return;
            
            mPosition += mSpeed * (float)deltaTime.TotalSeconds;
            if (mPosition >= mPathLenght)
            {
                OnComplete();
                Complete = true;
            }
        }

        protected abstract void OnComplete();
    }
}