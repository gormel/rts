using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Utils;
using UnityEngine;

namespace Assets.Core.GameObjects.Final
{
    interface IBarrakInfo : IFactoryBuildingInfo
    {
    }

    interface IBarrakOrders : IFactoryBuildingOrders
    {
        Task<bool> QueueRanged();
        Task<bool> QueueMeelee();
    }

    internal class Barrak : FactoryBuilding, IBarrakInfo, IBarrakOrders
    {
        public const int MeleeWarriorCost = 50;
        public static readonly TimeSpan MeeleeWarriorProductionTime = TimeSpan.FromSeconds(9);

        public const int RangedWarriorCost = 90;
        public static readonly TimeSpan RangedWarriorProductionTime = TimeSpan.FromSeconds(13);

        public static Vector2 BuildingSize { get; } = new Vector2(2, 2);
        public const float MaximumHealthConst = 300;
        
        public Barrak(Game.Game game, Vector2 position, IPlacementService placementService)
            : base(game, position, placementService)
        {
            Size = BuildingSize;
            Health = MaxHealth = MaximumHealthConst;
            ViewRadius = 3;
        }

        public Task<bool> QueueRanged()
        {
            return QueueUnit(RangedWarriorCost, RangedWarriorProductionTime, async (f, p) => await f.CreateRangedWarrior(p));
        }

        public Task<bool> QueueMeelee()
        {
            return QueueUnit(MeleeWarriorCost, MeeleeWarriorProductionTime, async (f, p) => await f.CreateMeeleeWarrior(p));
        }
    }
}
