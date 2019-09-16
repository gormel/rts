using Assets.Core.GameObjects.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Core.GameObjects.Final
{
    interface IMinigCampInfo : IBuildingInfo
    {
        float MiningSpeed { get; }
    }
    interface IMinigCampOrders : IBuildingOrders { }

    class MiningCamp : Building, IMinigCampInfo, IMinigCampOrders
    {
        public static Vector2 BuildingSize { get; } = new Vector2(1, 1);
        public const float MaximumHealthConst = 100;

        public float MiningSpeed { get; } = 10;

        private double mMinedTemp;
        private int mMinedTotal;

        public MiningCamp(Vector2 position)
        {
            Position = position;
            Size = BuildingSize;
            Health = MaxHealth = MaximumHealthConst;
        }

        public override void Update(TimeSpan deltaTime)
        {
            mMinedTemp += MiningSpeed * deltaTime.TotalSeconds;
            if (mMinedTemp > 1)
            {
                var ceiled = Mathf.CeilToInt((float)mMinedTemp);
                Player.Money.Store(ceiled);
                mMinedTemp -= ceiled;
                mMinedTotal += ceiled;
            }
        }
    }
}
