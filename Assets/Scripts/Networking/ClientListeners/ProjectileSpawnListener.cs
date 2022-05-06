using System.Threading;
using System.Threading.Tasks;
using Assets.Utils;
using Grpc.Core;
using UnityEngine;

namespace Networking.ClientListeners
{
    delegate void MissileSpawnDelegate(Vector2 from, Vector2 to, float speed, float radius);
    
    class ProjectileSpawnListener
    {
        private readonly UnitySyncContext mSyncContext;

        public event MissileSpawnDelegate OnMissileSpawn;

        public ProjectileSpawnListener(UnitySyncContext syncContext)
        {
            mSyncContext = syncContext;
        }

        public Task Listen(Channel channel)
        {
            var client = new ProjectilesService.ProjectilesServiceClient(channel);

            return Task.WhenAll(
                ListenMissiles(client, channel.ShutdownToken)
            );
        }

        private async Task ListenMissiles(ProjectilesService.ProjectilesServiceClient client, CancellationToken token)
        {
            using var call = client.ListenMissileProjectiles(new Empty(), cancellationToken: token);
            using var stream = call.ResponseStream;

            while (await stream.MoveNext(token))
            {
                var missile = stream.Current;
                await mSyncContext.Execute(() =>
                {
                    OnMissileSpawn?.Invoke(missile.From.ToUnity(), missile.To.ToUnity(), missile.Speed, missile.Radius);
                }, token);
            }
        }
    }
}