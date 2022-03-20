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
        public static readonly TimeSpan MeeleeWarriorProductionTime = TimeSpan.FromSeconds(9);
        public static readonly TimeSpan RangedWarriorProductionTime = TimeSpan.FromSeconds(13);

        public static Vector2 BuildingSize { get; } = new Vector2(2, 2);
        public const float MaximumHealthConst = 300;

        protected override float MaxHealthBase => MaximumHealthConst;
        
        public Barrak(Game.Game game, Vector2 position, IPlacementService placementService)
            : base(game, position, placementService)
        {
        }

        public override void OnAddedToGame()
        {
            Size = BuildingSize;
            ViewRadius = 3;
            
            base.OnAddedToGame();
        }

        public Task<bool> QueueRanged()
        {
            return QueueUnit(Player.RangedWarriorCost, RangedWarriorProductionTime, async (f, p) => await f.CreateRangedWarrior(p));
        }

        public Task<bool> QueueMeelee()
        {
            return QueueUnit(Player.MeleeWarriorCost, MeeleeWarriorProductionTime, async (f, p) => await f.CreateMeeleeWarrior(p));
        }
    }
}
