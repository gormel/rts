using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using UnityEngine;

namespace Assets.Core.GameObjects.Final
{
    interface IBarrakInfo : IBuildingInfo
    {

    }

    interface IBarrakOrders : IBuildingOrders
    {
        Task SetWaypoint(Vector2 waypoint);
    }

    internal class Barrak : Building, IBarrakInfo, IBarrakOrders
    {
        public const int MeleeWarriorCost = 50;
        public const int RangedWarriorCost = 70;
        public static Vector2 BuildingSize { get; } = new Vector2(2, 2);
        public const float MaximumHealthConst = 300;

        public Barrak(Vector2 position)
        {
            Size = BuildingSize;
            Waypoint = Position = position;
            Health = MaxHealth = MaximumHealthConst;
        }

        public override void Update(TimeSpan deltaTime)
        {
        }

        public async Task SetWaypoint(Vector2 waypoint)
        {
            Waypoint = waypoint;
        }
    }
}
