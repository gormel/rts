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
        void Unregister(Guid id);
    }
    
    class RtsServer
    {
        private Server mServer;

        public IRegistrator<IRangedWarriorOrders, IRangedWarriorInfo> RangedWarriorRegistrator { get; private set; }
        public IRegistrator<IWorkerOrders, IWorkerInfo> WorkerRegistrator { get; private set; }
        public IRegistrator<IBuildingTemplateOrders, IBuildingTemplateInfo> BuildingTemplateRegistrator { get; private set; }
        public IRegistrator<ICentralBuildingOrders, ICentralBuildingInfo> CentralBuildingRegistrator { get; private set; }
        public IRegistrator<IMinigCampOrders, IMinigCampInfo> MiningCampRegistrator { get; private set; }
        public IRegistrator<IBarrakOrders, IBarrakInfo> BarrakRegistrator { get; private set; }

        public void Listen(UnitySyncContext syncContext, IGameObjectFactory enemyFactory, Game game)
        {
            mServer = new Server();
            mServer.Ports.Add(new ServerPort(GameUtils.IP.ToString(), GameUtils.Port, ServerCredentials.Insecure));
            mServer.Services.Add(GameService.BindService(new GameServiceImpl(game, enemyFactory, syncContext)));
            
            var rangedWarriorService = new RangedWarriorServiceImpl();
            RangedWarriorRegistrator = rangedWarriorService;
            mServer.Services.Add(RangedWarriorService.BindService(rangedWarriorService));
            
            var workerService = new WorkerServiceImpl();
            WorkerRegistrator = workerService;
            mServer.Services.Add(WorkerService.BindService(workerService));

            var buildingTemplateService = new BuildingTemplateServiceImpl();
            BuildingTemplateRegistrator = buildingTemplateService;
            mServer.Services.Add(BuildingTemplateService.BindService(buildingTemplateService));

            var centralBuildingService = new CentralBuildingServiceImpl();
            CentralBuildingRegistrator = centralBuildingService;
            mServer.Services.Add(CentralBuildingService.BindService(centralBuildingService));

            var miningCampService = new MiningCampServiceImpl();
            MiningCampRegistrator = miningCampService;
            mServer.Services.Add(MiningCampService.BindService(miningCampService));

            var barrakService = new BarrakServiceImpl();
            BarrakRegistrator = barrakService;
            mServer.Services.Add(BarrakService.BindService(barrakService));

            mServer.Start();
        }

        public Task Shutdown()
        {
            return mServer.ShutdownAsync();
        }
    }
}
