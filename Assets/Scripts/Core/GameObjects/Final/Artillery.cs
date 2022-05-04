using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Assets.Core.Game;
using Assets.Core.GameObjects.Base;
using Assets.Core.Map;
using UnityEngine;

namespace Core.GameObjects.Final
{
    struct ProjectileInfo
    {
        public static ProjectileInfo Invalid { get; } = new() { mInvalid = true };
        
        public Vector2 StartPoint { get; set; }
        public Vector2 EndPoint { get; set; }

        private bool mInvalid;

        public ProjectileInfo(Vector2 position, Vector2 target)
        {
            StartPoint = position;
            EndPoint = target;
            mInvalid = false;
        }
    }
    interface IArtilleryInfo : IUnitInfo
    {
        bool LaunchAvaliable { get; }
        float MissileSpeed { get; }
        float MissileRadius { get; }
        float MissileDamage { get; }
        float LaunchRange { get; }
    }

    interface IArtilleryOrders : IUnitOrders
    {
        Task<ProjectileInfo> Launch(Vector2 target);
    }
    
    class Artillery : Unit, IArtilleryInfo, IArtilleryOrders
    {
        private readonly ITrajectoryService mTrajectoryService;
        public override float ViewRadius => 2;
        public override int Armour => Player.Upgrades.UnitArmourUpgrade.Calculate(ArmourBase);
        protected override float MaxHealthBase => 30;
        protected override int ArmourBase => 1;
        public override float Speed => 1.5f;
        public bool LaunchAvaliable => mLaunchTimer.Elapsed.TotalSeconds > 2;
        public float MissileSpeed => 5;
        public float MissileRadius => 1;
        public float MissileDamage => 60;
        public float LaunchRange => 8;

        private Stopwatch mLaunchTimer = Stopwatch.StartNew();
        
        public Artillery(Game game, IPathFinder pathFinder, Vector2 position, ITrajectoryService trajectoryService) 
            : base(game, pathFinder, position)
        {
            mTrajectoryService = trajectoryService;
        }

        public async Task<ProjectileInfo> Launch(Vector2 target)
        {
            if (!LaunchAvaliable)
                return ProjectileInfo.Invalid;
            
            mLaunchTimer = Stopwatch.StartNew();
            return new ProjectileInfo(Position, target);
        }

        private async Task LaunchMissile(Vector2 to, float trajectoryLenght)
        {
            await Task.Delay(TimeSpan.FromSeconds(trajectoryLenght / MissileSpeed));
            
            foreach (var gameObject in Game.QueryObjects(to, MissileRadius)) 
                gameObject.RecivedDamage += Math.Max(MissileDamage - gameObject.Armour, 1);
        }
    }
}