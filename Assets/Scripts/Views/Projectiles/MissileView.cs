using System;
using Assets.Views;
using UnityEngine;

namespace Views.Projectiles
{
    class MissileView : MonoBehaviour
    {
        private float mSpeed;
        private float mDistance;

        private float mPassed;

        public GameObject ExplosionPrefub;

        private MissileSpawnerView mSpawner;
        private Vector2 mFrom;
        private Vector2 mTo;
        
        public void Init(MissileSpawnerView spawner, MapView map, Vector2 from, Vector2 to, float speed, float radius)
        {
            transform.position = map.GetWorldPosition(from);

            mSpeed = speed;
            mDistance = spawner.GetTrajectoryLength(from, to);
            mPassed = 0;
            mSpawner = spawner;
            mFrom = from;
            mTo = to;
        }

        private void Update()
        {
            mPassed += mSpeed * Time.deltaTime;
            
            transform.position = mSpawner.GetTrajectoryPosition(mFrom, mTo, mPassed / mDistance);
            
            if (mPassed >= mDistance)
            {
                transform.SetParent(null);
                Instantiate(ExplosionPrefub, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }
    }
}