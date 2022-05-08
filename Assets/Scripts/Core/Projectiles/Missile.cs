using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Game;
using Assets.Core.GameObjects.Base;
using UnityEngine;

namespace Core.Projectiles
{
    class Missile : Projectile
    {
        private readonly float mExplodeRadius;
        private readonly Game mGame;
        private readonly float mDamage;
        private readonly Vector2 mExplodePosition;

        private List<RtsGameObject> mQueried = new();

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
            mQueried.Clear();
            mGame.QueryObjectsNoAlloc(mExplodePosition, mExplodeRadius, mQueried);
            foreach (var obj in mQueried)
                obj.RecivedDamage += Math.Max(1, mDamage - obj.Armour);

            foreach (var obj in mQueried)
                if (obj.RecivedDamage > obj.MaxHealth)
                    mGame.RemoveObject(obj.ID);
        }
    }
}