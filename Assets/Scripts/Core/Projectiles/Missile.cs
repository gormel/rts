using System;
using System.Linq;
using Assets.Core.Game;
using UnityEngine;

namespace Core.Projectiles
{
    class Missile : Projectile
    {
        private readonly float mExplodeRadius;
        private readonly Game mGame;
        private readonly float mDamage;
        private readonly Vector2 mExplodePosition;

        public Missile(float speed, float pathLenght, float explodeRadius, Game game, float damage, Vector2 explodePosition)
            : base(speed, pathLenght)
        {
            mExplodeRadius = explodeRadius;
            mGame = game;
            mDamage = damage;
            mExplodePosition = explodePosition;
        }

        protected override void OnComplete()
        {
            var queried = mGame.QueryObjects(mExplodePosition, mExplodeRadius).ToList();
            foreach (var obj in queried)
                obj.RecivedDamage += Math.Max(1, mDamage - obj.Armour);

            foreach (var obj in queried)
                if (obj.RecivedDamage > obj.MaxHealth)
                    mGame.RemoveObject(obj.ID);
        }
    }
}