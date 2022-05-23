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
using Core.GameObjects.Final;
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
        public IRegistrator<IArtilleryOrders, IArtilleryInfo> ArtilleryRegistrator { get; private set; }
        public IRegistrator<IWorkerOrders, IWorkerInfo> WorkerRegistrator { get; private set; }
        public IRegistrator<ICentralBuildingOrders, ICentralBuildingInfo> CentralBuildingRegistrator { get; private set; }
        public IRegistrator<IMinigCampOrders, IMinigCampInfo> MiningCampRegistrator { get; private set; }
        public IRegistrator<IBarrakOrders, IBarrakInfo> BarrakRegistrator { get; private set; }
        public IRegistrator<ITurretOrders, ITurretInfo> TurretRegistrator { get; private set; }
        public IRegistrator<IBuildersLabOrders, IBuildersLabInfo> BuildersLabRegistrator { get; private set; }
        public IRegistrator<IWarriorsLabOrders, IWarriorsLabInfo> WarriorsLabRegistrator { get; private set; }
        
        public IServerProjectileSpawner ProjectileSpawner { get; private set; }

        public event Action<string, int> MessageRecived;
        public event Action GameStarted;

        public void Listen(UnitySyncContext syncContext, IGameObjectFactory enemyFactory,
            IGameObjectFactory allyFactory, Game game, Player hostPlayer,
            IDictionary<string, UserState> registredPlayers, IDictionary<string, UserState> botPlayers)
        {
            mServer = new Server();
            mServer.Ports.Add(new ServerPort(IPAddress.Any.ToString(), GameUtils.GamePort, ServerCredentials.Insecure));
            mServer.Services.Add(GameService.BindService(mGameService = new GameServiceImpl(game, hostPlayer, enemyFactory, allyFactory, syncContext, registredPlayers)));
            mGameService.MessageRecived += GameServiceOnMessageRecived;
            mGameService.GameStarted += GameServiceOnGameStarted;
            var t = mGameService.InitBotPlayersAndStartGame(botPlayers);

            var projectileService = new ProjectileServiceImpl();
            ProjectileSpawner = projectileService;
            mServer.Services.Add(ProjectilesService.BindService(projectileService));

            var meeleeWarriorService = new MeeleeWarriorServiceImpl();
            MeeleeWarriorRegistrator = meeleeWarriorService;
            mServer.Services.Add(MeeleeWarriorService.BindService(meeleeWarriorService));

            var rangedWarriorService = new RangedWarriorServiceImpl();
            RangedWarriorRegistrator = rangedWarriorService;
            mServer.Services.Add(RangedWarriorService.BindService(rangedWarriorService));

            var artilleryService = new ArtilleryServiceImpl();
            ArtilleryRegistrator = artilleryService;
            mServer.Services.Add(ArtilleryService.BindService(artilleryService));
            
            var workerService = new WorkerServiceImpl();
            WorkerRegistrator = workerService;
            mServer.Services.Add(WorkerService.BindService(workerService));

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

            var buildersLabService = new BuildersLabServiceImpl();
            BuildersLabRegistrator = buildersLabService;
            mServer.Services.Add(BuildersLabService.BindService(buildersLabService));

            var warriorsLabService = new WarriorsLabServiceImpl();
            WarriorsLabRegistrator = warriorsLabService;
            mServer.Services.Add(WarriorsLabService.BindService(warriorsLabService));

            mServer.Start();
        }

        private void GameServiceOnGameStarted()
        {
            GameStarted?.Invoke();
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
