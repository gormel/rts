﻿using Assets.Core.GameObjects.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Utils;
using UnityEngine;

namespace Assets.Core.GameObjects.Final
{
    interface IMinigCampInfo : IBuildingInfo
    {
        float MiningSpeed { get; }
        int WorkerCount { get; }
    }

    interface IMinigCampOrders : IBuildingOrders
    {
        Task FreeWorker();
    }

    class MiningCamp : Building, IMinigCampInfo, IMinigCampOrders
    {
        private readonly Game.Game mGame;
        public IPlacementService PlacementService { get; }
        public static Vector2 BuildingSize { get; } = new Vector2(1, 1);
        public const float MaximumHealthConst = 100;
        public const int MaxWorkers = 4;

        public float MiningSpeed { get; } = 2.5f;
        public int WorkerCount => mWorkers.Count;

        private double mMinedTemp;
        private int mMinedTotal;

        public Stack<Worker> mWorkers = new Stack<Worker>();

        public MiningCamp(Game.Game game, Vector2 position, IPlacementService placementService)
        {
            mGame = game;
            PlacementService = placementService;
            Position = position;
            Size = BuildingSize;
            Health = MaxHealth = MaximumHealthConst;
            ViewRadius = 2;
        }

        public bool TryPutWorker(Worker worker)
        {
            if (mWorkers.Count >= MaxWorkers)
                return false;

            if (mWorkers.Contains(worker))
                return false;
            
            mWorkers.Push(worker);
            return true;
        }
        
        public override void Update(TimeSpan deltaTime)
        {
            mMinedTemp += MiningSpeed * (mWorkers.Count + 1) * deltaTime.TotalSeconds;
            if (mMinedTemp > 1)
            {
                var ceiled = Mathf.CeilToInt((float)mMinedTemp);
                Player.Money.Store(ceiled);
                mMinedTemp -= ceiled;
                mMinedTotal += ceiled;
            }
        }

        public override void OnRemovedFromGame()
        {
            base.OnRemovedFromGame();

            while (mWorkers.Count > 0)
            {
                var worker = mWorkers.Pop();
                mGame.RemoveObject(worker.ID);
            }
        }

        public async Task FreeWorker()
        {
            if (mWorkers.Count == 0)
                return;
            
            var point = await PlacementService.TryAllocatePoint();
            if (point == PlacementPoint.Invalid)
                return;

            var unit = mWorkers.Pop();
            unit.IsAttachedToMiningCamp = false;
            await unit.PathFinder.Teleport(point.Position, mGame.Map.Data);
            await PlacementService.ReleasePoint(point.ID);
        }
    }
}
