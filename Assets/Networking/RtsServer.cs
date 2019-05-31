using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Game;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using Assets.Networking.Services;
using Assets.Utils;
using Grpc.Core;

namespace Assets.Networking
{
    interface IRegistrator<TOrders, TInfo>
        where TOrders : IGameObjectOrders
        where TInfo : IGameObjectInfo
    {
        void Register(TOrders orders, TInfo info);
    }
    
    class RtsServer
    {
        private Server mServer;

        public IRegistrator<IWorkerOrders, IWorkerInfo> WorkerRegistrator { get; }
        
        public void Listen(UnitySyncContext syncContext, IGameObjectFactory enemyFactory, Game game)
        {
            mServer = new Server();
            mServer.Ports.Add(new ServerPort(GameUtils.IP.ToString(), GameUtils.Port, ServerCredentials.Insecure));
            mServer.Services.Add(GameService.BindService(new GameServiceImpl(game, enemyFactory, syncContext)));
            mServer.Services.Add(WorkerService.BindService(new WorkerServiceImpl(syncContext)));

            mServer.Start();
        }

        public Task Shutdown()
        {
            return mServer.ShutdownAsync();
        }
    }
}
