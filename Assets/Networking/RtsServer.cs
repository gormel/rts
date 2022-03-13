using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        private GameServiceImpl mGameService;

        public IRegistrator<IRangedWarriorOrders, IRangedWarriorInfo> RangedWarriorRegistrator { get; private set; }
        public IRegistrator<IMeeleeWarriorOrders, IMeeleeWarriorInfo> MeeleeWarriorRegistrator { get; private set; }
        public IRegistrator<IWorkerOrders, IWorkerInfo> WorkerRegistrator { get; private set; }
        public IRegistrator<IBuildingTemplateOrders, IBuildingTemplateInfo> BuildingTemplateRegistrator { get; private set; }
        public IRegistrator<ICentralBuildingOrders, ICentralBuildingInfo> CentralBuildingRegistrator { get; private set; }
        public IRegistrator<IMinigCampOrders, IMinigCampInfo> MiningCampRegistrator { get; private set; }
        public IRegistrator<IBarrakOrders, IBarrakInfo> BarrakRegistrator { get; private set; }
        public IRegistrator<ITurretOrders, ITurretInfo> TurretRegistrator { get; private set; }

        public event Action<string, int> MessageRecived;

        public void Listen(UnitySyncContext syncContext, IGameObjectFactory enemyFactory, Game game)
        {
            mServer = new Server();
            mServer.Ports.Add(new ServerPort(IPAddress.Any.ToString(), GameUtils.GamePort, ServerCredentials.Insecure));
            mServer.Services.Add(GameService.BindService(mGameService = new GameServiceImpl(game, enemyFactory, syncContext)));
            mGameService.MessageRecived += GameServiceOnMessageRecived;

            var meeleeWarriorService = new MeeleeWarriorServiceImpl();
            MeeleeWarriorRegistrator = meeleeWarriorService;
            mServer.Services.Add(MeeleeWarriorService.BindService(meeleeWarriorService));

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

            var turretService = new TurretServiceImpl();
            TurretRegistrator = turretService;
            mServer.Services.Add(TurretService.BindService(turretService));

            mServer.Start();
        }

        private void GameServiceOnMessageRecived(string nickname, int stickerID)
        {
            MessageRecived?.Invoke(nickname, stickerID);
        }

        public void SendChatMessage(string nickname, int stickerID)
        {
            mGameService?.SendChatMessage(new ChatMessage { Nickname = nickname, StickerID = stickerID });
        }

        public Task Shutdown()
        {
            return mServer.KillAsync();
        }
    }
}
