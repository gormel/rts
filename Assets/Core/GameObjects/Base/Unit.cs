﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Utils;
using Assets.Core.Map;
using UnityEngine;

namespace Assets.Core.GameObjects.Base
{
    interface IUnitOrders : IGameObjectOrders
    {
        Task GoTo(Vector2 position);
    }

    interface IUnitInfo : IGameObjectInfo
    {
        float Speed { get; }
        Vector2 Direction { get; }
        Vector2 Destignation { get; }
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

            protected override async Task OnBegin()
            {
                await mUnit.PathFinder.SetTarget(mPosition, mUnit.Game.Map.Data);
                mUnit.PathFinder.Arrived += PathFinderOnArrived;
            }

            private void PathFinderOnArrived()
            {
                mUnit.PathFinder.Arrived -= PathFinderOnArrived;
                End();
            }

            protected override void OnUpdate(TimeSpan deltaTime)
            {
            }

            protected override void OnCancel()
            {
                mUnit.PathFinder.Stop();
            }
        }

        protected Game.Game Game { get; }

        public float Speed { get; protected set; }
        public Vector2 Direction { get; protected set; }
        public Vector2 Destignation { get; protected set; }

        protected IPathFinder PathFinder { get; }

        private UnitOrder mOrder;

        protected bool HasNoOrder => mOrder == null;

        public Unit(Game.Game game, IPathFinder pathFinder, Vector2 position)
        {
            Game = game;
            PathFinder = pathFinder;
            Destignation = Position = position;
        }

        protected void SetOrder(UnitOrder order)
        {
            var o = mOrder;
            while (o != null)
            {
                o.Cancel();
                o = o.Next;
            }

            mOrder = order;
        }

        public async Task GoTo(Vector2 position)
        {
            SetOrder(new GoToOrder(this, position));
        }

        public override void Update(TimeSpan deltaTime)
        {
            if (mOrder != null)
            {
                mOrder.Update(deltaTime);
                if (mOrder.State == OrderState.Completed)
                    mOrder = mOrder.Next;
            }

            Position = PathFinder.CurrentPosition;
            Direction = PathFinder.CurrentDirection;
            Destignation = PathFinder.Target;
        }
    }
}