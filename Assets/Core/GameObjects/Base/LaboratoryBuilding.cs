using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Core.Game;
using Assets.Core.GameObjects.Base;
using UnityEngine;

namespace Assets.Core.GameObjects.Base
{
    interface ILaboratoryBuildingInfo : IBuildingInfo, IQueueOrdersInfo
    {
    }

    interface ILaboratoryBuildingOrders : IBuildingOrders
    {
        Task CancelResearch();
    }

    abstract class LaboratoryBuilding : Building, ILaboratoryBuildingInfo, ILaboratoryBuildingOrders
    {
        private interface IUpgradeDecorator
        {
            void BeginUpgrade();
            void EndUpgrade();
            void CancelUpgrade();
        }

        private class UpgradeDecorator<T> : IUpgradeDecorator
        {
            private readonly Upgrade<T> mUpgrade;

            public UpgradeDecorator(Upgrade<T> upgrade)
            {
                mUpgrade = upgrade;
            }

            public void BeginUpgrade()
            {
                mUpgrade.BeginLevelUp();
            }

            public void EndUpgrade()
            {
                mUpgrade.EndLevelUp();
            }

            public void CancelUpgrade()
            {
                mUpgrade.CancelLevelUp();
            }
        }
        
        private readonly Vector2 mInitialPosition;
        public float Progress => 1 - (float)(mUpgradeTimeLeft.TotalSeconds / mUpgradeTime.TotalSeconds);
        public int Queued => mUpgradesQueue.Count + (mProcessingUpgrade == null ? 0 : 1);

        private readonly Queue<(IUpgradeDecorator Upgrade, TimeSpan Time)> mUpgradesQueue = new Queue<(IUpgradeDecorator, TimeSpan)>();
        private IUpgradeDecorator mProcessingUpgrade;
        private TimeSpan mUpgradeTime = TimeSpan.FromSeconds(1);
        private TimeSpan mUpgradeTimeLeft = TimeSpan.Zero;

        public LaboratoryBuilding(Vector2 position)
        {
            mInitialPosition = position;
        }

        public override void OnAddedToGame()
        {
            Position = mInitialPosition;
            base.OnAddedToGame();
        }

        protected Task QueueUpgrade<T>(Upgrade<T> upgrade, TimeSpan upgradeTime)
        {
            upgrade.BeginLevelUp();
            mUpgradesQueue.Enqueue((new UpgradeDecorator<T>(upgrade), upgradeTime));
            return Task.CompletedTask;
        }

        public override void Update(TimeSpan deltaTime)
        {
            if (mProcessingUpgrade != null)
            {
                mUpgradeTimeLeft -= deltaTime;
                if (mUpgradeTimeLeft <= TimeSpan.Zero)
                {
                    mProcessingUpgrade.EndUpgrade();
                    mProcessingUpgrade = null;
                }
            }
            else if (mUpgradesQueue.Count > 0)
            {
                var info = mUpgradesQueue.Dequeue();
                mProcessingUpgrade = info.Upgrade;
                mUpgradeTimeLeft = mUpgradeTime = info.Time;
            }
        }

        public Task CancelResearch()
        {
            if (mProcessingUpgrade == null)
                return Task.CompletedTask;
            
            mProcessingUpgrade.CancelUpgrade();
            mUpgradeTimeLeft = TimeSpan.Zero;
            mProcessingUpgrade = null;
            return Task.CompletedTask;
        }
    }
}