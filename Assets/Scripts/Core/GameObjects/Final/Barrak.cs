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
        Task<bool> QueueArtillery();
    }

    internal class Barrak : FactoryBuilding, IBarrakInfo, IBarrakOrders
    {
        public static TimeSpan MeeleeWarriorProductionTime { get; } = TimeSpan.FromSeconds(9);
        public static TimeSpan RangedWarriorProductionTime { get; } = TimeSpan.FromSeconds(13);
        public static TimeSpan ArtilleryProductionTime { get; } = TimeSpan.FromSeconds(10);
        
        public static int MeleeWarriorCost { get; } = 50;
        public static int RangedWarriorCost { get; } = 90;
        public static int ArtilleryCost { get; } = 130;

        public static Vector2 BuildingSize { get; } = new Vector2(2, 2);
        public const float MaximumHealthConst = 300;

        public override float ViewRadius => 3;
        protected override float MaxHealthBase => MaximumHealthConst;
        public override Vector2 Size => BuildingSize;
        
        public Barrak(Game.Game game, Vector2 position, IPlacementService placementService)
            : base(game, position, Worker.BarrakBuildTime, placementService)
        {
        }

        public Task<bool> QueueRanged()
        {
            return QueueUnit(RangedWarriorCost, RangedWarriorProductionTime, async (f, p) => await f.CreateRangedWarrior(p));
        }

        public Task<bool> QueueMeelee()
        {
            return QueueUnit(MeleeWarriorCost, MeeleeWarriorProductionTime, async (f, p) => await f.CreateMeeleeWarrior(p));
        }

        public Task<bool> QueueArtillery()
        {
            if (!Player.ArilleryOrderAvaliable)
                return Task.FromResult(false);
            
            return QueueUnit(ArtilleryCost, ArtilleryProductionTime, async (f, p) => await f.CreateArtillery(p));
        }
    }
}
