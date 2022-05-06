using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Assets.Utils;
using Core.Projectiles;
using Grpc.Core;
using UnityEngine;
using MissileCreationQueue = Assets.Utils.AsyncQueue<MissileProjectileCreation>;

namespace Assets.Networking.Services
{
    interface IServerProjectileSpawner
    {
        void SpawnMissile(Vector2 from, Vector2 to, float speed, float radius);
    }
    class ProjectileServiceImpl : ProjectilesService.ProjectilesServiceBase, IServerProjectileSpawner
    {
        private readonly ConcurrentDictionary<Guid, MissileCreationQueue> mMissileCreations = new();
        
        public override async Task ListenMissileProjectiles(Empty request, IServerStreamWriter<MissileProjectileCreation> responseStream, ServerCallContext context)
        {
            var id = Guid.NewGuid();
            try
            {
                var creations = new MissileCreationQueue();
                mMissileCreations.AddOrUpdate(id, creations, (_,_) => creations);
                while (!context.CancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var creation = await creations.DequeueAsync(context.CancellationToken);
                        await responseStream.WriteAsync(creation);
                    }
                    catch (RpcException e)
                    {
                        if (e.StatusCode != StatusCode.Unavailable)
                            throw;

                        await Task.Delay(TimeSpan.FromSeconds(0.5), context.CancellationToken);
                    }
                }
            }
            finally
            {
                mMissileCreations.TryRemove(id, out _);
            }
        }

        public void SpawnMissile(Vector2 from, Vector2 to, float speed, float radius)
        {
            foreach (var creationsValue in mMissileCreations.Values)
            {
                creationsValue.Enqueue(new MissileProjectileCreation
                {
                    From = from.ToGrpc(),
                    To = to.ToGrpc(),
                    Radius = radius,
                    Speed = speed,
                });
            }
        }
    }
}