using System;
using Assets.Core.GameObjects.Utils;
using Assets.Core.Map;
using UnityEngine;

namespace Assets.Core.GameObjects.Base
{
    interface IUnitOrders : IGameObjectOrders
    {
        void GoTo(Vector2 position);
    }

    interface IUnitInfo : IGameObjectInfo
    {
        float Speed { get; }
        Vector2 Direction { get; }
    }

    abstract class Unit : RtsGameObject, IUnitInfo, IUnitOrders
    {
        protected class GoToOrder : UnitOrder
        {
            private Unit mUnit;
            private readonly Vector2 mPosition;

            public GoToOrder(Unit unit, Vector2 position)
            {
                mUnit = unit;
                mPosition = position;
            }

            protected override void OnBegin()
            {
                mUnit.mPathFinder.SetTarget(mPosition, mUnit.mGame.Map);
            }

            protected override void OnUpdate(TimeSpan deltaTime)
            {
                mUnit.Position = mUnit.mPathFinder.CurrentPosition;
                mUnit.Direction = mUnit.mPathFinder.CurrentDirection;
                if (!mUnit.mPathFinder.Active)
                    End();
            }

            protected override void OnCancel()
            {
                mUnit.mPathFinder.Stop();
            }
        }

        protected readonly Game.Game mGame;
        private readonly IPathFinder mPathFinder;

        public float Speed { get; protected set; }
        public Vector2 Direction { get; protected set; }

        private UnitOrder mOrder;

        public Unit(Game.Game game, IPathFinder pathFinder, Vector2 position)
        {
            mGame = game;
            mPathFinder = pathFinder;
            Position = position;
        }

        protected void SetOrder(UnitOrder order)
        {
            if (mOrder != null && mOrder.Active)
                mOrder.Cancel();

            mOrder = order;
            mOrder.Begin();
        }

        public void GoTo(Vector2 position)
        {
            SetOrder(new GoToOrder(this, position));
        }

        public override void Update(TimeSpan deltaTime)
        {
            if (mOrder != null)
                mOrder.Update(deltaTime);
        }
    }
}