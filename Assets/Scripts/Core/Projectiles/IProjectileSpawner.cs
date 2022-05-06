using UnityEngine;

namespace Core.Projectiles
{
    interface IProjectileSpawner
    {
        void SpawnMissile(Vector2 from, Vector2 to, float speed, float radius, float damage);
    }
}