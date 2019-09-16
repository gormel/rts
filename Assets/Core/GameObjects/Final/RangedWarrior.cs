using Assets.Core.GameObjects.Base;
using Assets.Core.Map;
using UnityEngine;

namespace Assets.Core.GameObjects.Final {

    interface IRangedWarriorInfo : IUnitInfo
    {
    }

    interface IRangedWarriorOrders : IUnitOrders
    {
    }

    internal class RangedWarrior : Unit, IRangedWarriorInfo, IRangedWarriorOrders
    {
        public RangedWarrior(Game.Game game, IPathFinder pathFinder, Vector2 position)
            : base(game, pathFinder, position)
        {
            Speed = 4;
            MaxHealth = Health = 50;
        }
    }
}