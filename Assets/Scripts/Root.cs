using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Assets.Core.Game;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using Assets.Core.Map;
using Assets.Interaction;
using Assets.Networking;
using Assets.Networking.Services;
using Assets.Utils;
using Assets.Views;
using Assets.Views.Base;
using Assets.Views.Utils;
using Core.BotIntelligence;
using Core.GameObjects.Final;
using Core.Projectiles;
using Interaction.Debug;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Views.Projectiles;
using GameObject = UnityEngine.GameObject;

class Root : MonoBehaviour
{
    private class ProjectileSpawner : IProjectileSpawner
    {
        private readonly Game mGame;
        private readonly RtsServer mServer;
        private readonly MissileSpawnerView mMissileTrajectoryService;

        public ProjectileSpawner(Game game, RtsServer server, MissileSpawnerView missileTrajectoryService)
        {
            mGame = game;
            mServer = server;
            mMissileTrajectoryService = missileTrajectoryService;
        }

        public void SpawnMissile(Vector2 from, Vector2 to, float speed, float radius, float damage)
        {
            mGame.SpawnProjectile(new Missile(
                speed,
                mMissileTrajectoryService.GetTrajectoryLength(from, to),
                radius,
                mGame,
                damage,
                to
                ));
            
            mServer.ProjectileSpawner.SpawnMissile(from, to, speed, radius);
            mMissileTrajectoryService.Spawn(from, to, speed, radius);
        }
    }
    
    private class Factory : IGameObjectFactory
    {
        private readonly RtsServer mServer;
        private readonly UnitySyncContext mSyncContext;
        private readonly ExternalUpdater mUpdater;
        private readonly Game mGame;
        private readonly MapView mMap;
        private readonly GameObject mRangedWarriorPrefab;
        private readonly GameObject mMeeleeWarriorPrefab;
        private readonly GameObject mWorkerPrefab;
        private readonly GameObject mBuildingTemplatePrefab;
        private readonly GameObject mCentralBuildingPrefab;
        private readonly GameObject mMiningCampPrefab;
        private readonly GameObject mBarrakPrefab;
        private readonly GameObject mTurretPrefab;
        private readonly GameObject mBuildersLabPrefab;
        private readonly GameObject mWarriorsLabPrefab;
        private readonly GameObject mArtilleryPrefab;

        private readonly ProjectileSpawner mProjectileSpawner;

        public event Action<SelectableView> ViewCreated;

        public Factory(RtsServer server, Game game, Root root)
        {
            mServer = server;
            mGame = game;
            mSyncContext = root.SyncContext;
            mUpdater = root.Updater;
            mMap = root.MapView;
            mRangedWarriorPrefab = root.RangedWarriorPrefab;
            mMeeleeWarriorPrefab = root.MeeleeWarriorPrefab;
            mArtilleryPrefab = root.ArtilleryPrefab;
            mWorkerPrefab = root.WorkerPrefab;
            mBuildingTemplatePrefab = root.BuildingTemplatePrefab;
            mCentralBuildingPrefab = root.CentralBuildingPrefab;
            mMiningCampPrefab = root.MiningCampPrefab;
            mBarrakPrefab = root.BarrakPrefab;
            mTurretPrefab = root.TurretPrefab;
            mBuildersLabPrefab = root.BuildersLabPrefab;
            mWarriorsLabPrefab = root.WarriorsLabPrefab;

            mProjectileSpawner = new ProjectileSpawner(mGame, mServer, root.MissileSpawner);
        }

        private Task<TModel> CreateModelAndView<TView, TModel, TOrders, TInfo>(GameObject prefab, Func<TView, TModel> createModel, Vector2 position)
            where TView : ModelSelectableView<TOrders, TInfo>
            where TOrders : IGameObjectOrders
            where TInfo : IGameObjectInfo
            where TModel : RtsGameObject, TOrders, TInfo
        {
            return mSyncContext.Execute(() =>
            {
                var instance = Instantiate(prefab);
                var view = instance.GetComponent<TView>();
                if (view == null)
                    throw new Exception("Prefab not contains View script.");

                var result = createModel(view);
                result.RemovedFromGame += o =>
                {
                    Destroy(instance);
                    instance.transform.parent = null;
                };
                view.Map = mMap;
                view.SyncContext = mSyncContext;
                view.Updater = mUpdater;
                view.LoadModel(result, result);

                instance.transform.parent = mMap.ChildContainer.transform;
                instance.transform.localPosition = mMap.GetWorldPosition(position);

                ViewCreated?.Invoke(view);

                return result;
            });
        }

        public async Task<Worker> CreateWorker(Vector2 position)
        {
            var worker = await CreateModelAndView<WorkerView, Worker, IWorkerOrders, IWorkerInfo>(
                mWorkerPrefab,
                view => new Worker(mGame, view, position),
                position
            );
            worker.AddedToGame += o => mServer.WorkerRegistrator.Register(worker, worker);
            worker.RemovedFromGame += o => mServer.WorkerRegistrator.Unregister(o.ID);
            return worker;
        }

        public async Task<RangedWarrior> CreateRangedWarrior(Vector2 position)
        {
            var rangedWarrior = await CreateModelAndView<RangedWarriorView, RangedWarrior, IRangedWarriorOrders, IRangedWarriorInfo>(
                mRangedWarriorPrefab,
                view => new RangedWarrior(mGame, view, position), 
                position
            );
            rangedWarrior.AddedToGame += o => mServer.RangedWarriorRegistrator.Register(rangedWarrior, rangedWarrior);
            rangedWarrior.RemovedFromGame += o => mServer.RangedWarriorRegistrator.Unregister(o.ID);
            return rangedWarrior;
        }

        public async Task<MeeleeWarrior> CreateMeeleeWarrior(Vector2 position)
        {
            var meeleeWarrior = await CreateModelAndView<MeeleeWarriorView, MeeleeWarrior, IMeeleeWarriorOrders, IMeeleeWarriorInfo> (
                mMeeleeWarriorPrefab,
                view => new MeeleeWarrior(mGame, view, position), 
                position
            );
            meeleeWarrior.AddedToGame += o => mServer.MeeleeWarriorRegistrator.Register(meeleeWarrior, meeleeWarrior);
            meeleeWarrior.RemovedFromGame += o => mServer.MeeleeWarriorRegistrator.Unregister(o.ID);
            return meeleeWarrior;
        }

        public async Task<Artillery> CreateArtillery(Vector2 position)
        {
            var artillery = await CreateModelAndView<ArtilleryView, Artillery, IArtilleryOrders, IArtilleryInfo> (
                mArtilleryPrefab,
                view => new Artillery(mGame, view, position, mProjectileSpawner), 
                position
            );
            artillery.AddedToGame += o => mServer.ArtilleryRegistrator.Register(artillery, artillery);
            artillery.RemovedFromGame += o => mServer.ArtilleryRegistrator.Unregister(o.ID);
            return artillery;
        }

        public async Task<BuildingTemplate> CreateBuildingTemplate(Vector2 position, Func<Vector2, Task<Building>> building, TimeSpan buildTime, Vector2 size, float maxHealth, int cost)
        {
            var template = await CreateModelAndView<BuildingTemplateView, BuildingTemplate, IBuildingTemplateOrders, IBuildingTemplateInfo>(
                mBuildingTemplatePrefab,
                view => new BuildingTemplate(mGame, building, buildTime, size, position, maxHealth, view, cost),
                position
            );
            template.AddedToGame += o => mServer.BuildingTemplateRegistrator.Register(template, template);
            template.RemovedFromGame += o => mServer.BuildingTemplateRegistrator.Unregister(o.ID);
            return template;
        }

        public async Task<CentralBuilding> CreateCentralBuilding(Vector2 position)
        {
            var centralBuilding = await CreateModelAndView<CentralBuildingView, CentralBuilding, ICentralBuildingOrders, ICentralBuildingInfo>(
                mCentralBuildingPrefab,
                view => new CentralBuilding(mGame, position, view),
                position
            );
            centralBuilding.AddedToGame += o => mServer.CentralBuildingRegistrator.Register(centralBuilding, centralBuilding);
            centralBuilding.RemovedFromGame += o => mServer.CentralBuildingRegistrator.Unregister(o.ID);
            return centralBuilding;
        }

        public async Task<Barrak> CreateBarrak(Vector2 position)
        {
            var barrak = await CreateModelAndView<BarrakView, Barrak, IBarrakOrders, IBarrakInfo>(
                mBarrakPrefab,
                view => new Barrak(mGame, position, view),
                position
            );

            barrak.AddedToGame += o => mServer.BarrakRegistrator.Register(barrak, barrak);
            barrak.RemovedFromGame += o => mServer.BarrakRegistrator.Unregister(o.ID);
            return barrak;
        }

        public async Task<Turret> CreateTurret(Vector2 position)
        {
            var turret = await CreateModelAndView<TurretView, Turret, ITurretOrders, ITurretInfo>(
                mTurretPrefab,
                view => new Turret(mGame, position),
                position
            );

            turret.AddedToGame += o => mServer.TurretRegistrator.Register(turret, turret);
            turret.RemovedFromGame += o => mServer.TurretRegistrator.Unregister(o.ID);
            return turret;
        }

        public async Task<BuildersLab> CreateBuildersLab(Vector2 position)
        {
            var buildersLab = await CreateModelAndView<BuildersLabView, BuildersLab, IBuildersLabOrders, IBuildersLabInfo>(
                mBuildersLabPrefab,
                view => new BuildersLab(position),
                position
            );

            buildersLab.AddedToGame += o => mServer.BuildersLabRegistrator.Register(buildersLab, buildersLab);
            buildersLab.RemovedFromGame += o => mServer.BuildersLabRegistrator.Unregister(o.ID);
            return buildersLab;
        }

        public async Task<WarriorsLab> CreateWarriorsLab(Vector2 position)
        {
            var warriorsLab = await CreateModelAndView<WarriorsLabView, WarriorsLab, IWarriorsLabOrders, IWarriorsLabInfo>(
                mWarriorsLabPrefab,
                view => new WarriorsLab(position),
                position
            );

            warriorsLab.AddedToGame += o => mServer.WarriorsLabRegistrator.Register(warriorsLab, warriorsLab);
            warriorsLab.RemovedFromGame += o => mServer.WarriorsLabRegistrator.Unregister(o.ID);
            return warriorsLab;
        }

        public async Task<MiningCamp> CreateMiningCamp(Vector2 position)
        {
            var miningCamp = await CreateModelAndView<MiningCampView, MiningCamp, IMinigCampOrders, IMinigCampInfo>(
                mMiningCampPrefab,
                view => new MiningCamp(mGame, position, view),
                position
            );
            miningCamp.AddedToGame += o => mServer.MiningCampRegistrator.Register(miningCamp, miningCamp);
            miningCamp.RemovedFromGame += o => mServer.MiningCampRegistrator.Unregister(o.ID);
            return miningCamp;
        }
    }

    public string GuiSceneName;

    public GameObject BarrakPrefab;
    public GameObject MapPrefab;
    public GameObject WorkerPrefab;
    public GameObject RangedWarriorPrefab;
    public GameObject MeeleeWarriorPrefab;
    public GameObject ArtilleryPrefab;
    public GameObject BuildingTemplatePrefab;
    public GameObject CentralBuildingPrefab;
    public GameObject MiningCampPrefab;
    public GameObject TurretPrefab;
    public GameObject BuildersLabPrefab;
    public GameObject WarriorsLabPrefab;
    
    public UnitySyncContext SyncContext;
    public ExternalUpdater Updater;
    public GameObject PlayerScreen;

    public GameObject DebugPanelRoot;
    public GameObject DebugPanelPrefub;

    public MissileSpawnerView MissileSpawner;

    private RtsServer mServer;
    private RtsClient mClient;
    private Game mGame;

    private ConcurrentDictionary<Guid, IPlayerState> mClientOtherPlayers = new ConcurrentDictionary<Guid, IPlayerState>();
    public IPlayerState Player { get; private set; }
    public MapView MapView { get; private set; }

    public event Action<IMapData> MapLoaded;
    public event Action<string, int> ChatMessageRecived;

    void Start()
    {
        if (GameUtils.CurrentMode == GameMode.Server)
        {
            mGame = new Game();
            mServer = new RtsServer();
            MapView = CreateMap(mGame.Map.Data, true);

            var enemyFactory = new Factory(mServer, mGame, this);
            var controlledFactory = new Factory(mServer, mGame, this);
            var allyFactory = new Factory(mServer, mGame, this);
            allyFactory.ViewCreated += AllyFactoryOnViewCreated;
            controlledFactory.ViewCreated += ControlledFactoryOnViewCreated;
            enemyFactory.ViewCreated += EnemyFactoryOnViewCreated;

#if HOST_IS_BOT
            var player = new BotPlayer(mGame, GameUtils.Nickname, controlledFactory, GameUtils.Team);
            mGame.AddBotPlayer(player);
#else
            var player = new Player(GameUtils.Nickname, controlledFactory, GameUtils.Team);
            mGame.AddPlayer(player);
#endif
            Player = player;

            mServer.MessageRecived += OnChatMessageRecived;
#if DEVELOPMENT_BUILD
            mServer.GameStarted += ServerOnGameStarted;
#endif

            mServer.Listen(SyncContext, enemyFactory, allyFactory, mGame, player, GameUtils.RegistredPlayers, GameUtils.BotPlayers);

            var success = GameUtils.TryCreateBase(mGame, player, out var basePos);
            
            PlaceCamera(basePos);
        }

        if (GameUtils.CurrentMode == GameMode.Client)
        {
            mClient = new RtsClient(SyncContext);

            mClient.MapLoaded += data => MapView = CreateMap(data, false);
            mClient.BaseCreated += pos => PlaceCamera(pos);
            mClient.PlayerConnected += state => Player = state;
            mClient.DisconnectedFromServer += () => SceneManager.LoadScene(GuiSceneName);
            mClient.OtherPlayerConnected += (nick, player) => mClientOtherPlayers.AddOrUpdate(player.ID, player, (id, p) => player);

            mClient.MeeleeWarriorCreated += ClientOnMeeleeWarriorCreated;
            mClient.RangedWarriorCreated += ClientOnRangedWarriorCreated;
            mClient.ArtilleryCreated += ClientOnArtilleryCreated;
            mClient.WorkerCreated += ClientOnWorkerCreated;
            mClient.BuildingTemplateCreated += ClientOnBuildingTemplateCreated;
            mClient.CentralBuildingCreated += ClientOnCentralBuildingCreated;
            mClient.MiningCampCreated += ClientOnMiningCampCreated;
            mClient.BarrakCreated += ClientOnBarrakCreated;
            mClient.TurretCreated += ClientOnTurretCreated;
            mClient.BuildersLabCreated += ClientOnBuildersLabCreated;
            mClient.WarriorsLabCreated += ClientOnWarriorsLabCreated;

            mClient.MissileSpawned += (from, to, speed, radius) => MissileSpawner.Spawn(from, to, speed, radius);

            mClient.ObjectDestroyed += ClientOnObjectDestroyed;

            mClient.ChatMessageRecived += OnChatMessageRecived;

            mClient.Listen();
        }
    }

#if DEVELOPMENT_BUILD
    private void ServerOnGameStarted()
    {
        var panelInst = Instantiate(DebugPanelPrefub, DebugPanelRoot.transform, false);
        var panel = panelInst.GetComponent<DebugPanel>();
        panel.ApplyPlayers(mGame.GetPlayers());
    }
#endif

    private void AllyFactoryOnViewCreated(SelectableView view)
    {
        view.OwnershipRelation = ObjectOwnershipRelation.Ally;
    }

    public void SendChatMessage(string nickname, int stickerID)
    {
        if (mServer != null)
            mServer.SendChatMessage(nickname, stickerID);

        if (mClient != null)
            mClient.SendChatMessage(nickname, stickerID);
    }

    private void OnChatMessageRecived(string nickname, int stickerID)
    {
        ChatMessageRecived?.Invoke(nickname, stickerID);
    }

    private void ClientOnMeeleeWarriorCreated(IMeeleeWarriorOrders orders, IMeeleeWarriorInfo info)
    {
        CreateClientView(orders, info, MeeleeWarriorPrefab);
    }

    private void ClientOnRangedWarriorCreated(IRangedWarriorOrders orders, IRangedWarriorInfo info)
    {
        CreateClientView(orders, info, RangedWarriorPrefab);
    }

    private void ClientOnArtilleryCreated(IArtilleryOrders orders, IArtilleryInfo info)
    {
        CreateClientView(orders, info, ArtilleryPrefab);
    }

    public void PlaceCamera(Vector2 pos)
    {
        var cameraY = PlayerScreen.transform.position.y;
        var dY = cameraY - MapView.transform.position.y;
        var dX = 0f;
        var dZ = 0f;

        if (PlayerScreen.transform.eulerAngles.z > 0)
            dX = dY / Mathf.Tan(PlayerScreen.transform.eulerAngles.z * Mathf.Deg2Rad);

        if (PlayerScreen.transform.eulerAngles.x > 0)
            dZ = dY / Mathf.Tan(PlayerScreen.transform.eulerAngles.x * Mathf.Deg2Rad);

        PlayerScreen.transform.position = new Vector3(pos.x - dX, cameraY, pos.y - dZ);
    }

    private void ClientOnObjectDestroyed(IGameObjectInfo objectInfo)
    {
        for (int i = 0; i < MapView.ChildContainer.transform.childCount; i++)
        {
            var child = MapView.ChildContainer.transform.GetChild(i);
            var view = child.GetComponent<SelectableView>();
            if (view != null && view.InfoBase.ID == objectInfo.ID)
            {
                Destroy(child.gameObject);
                child.parent = null;
                break;
            }
        }
    }

    private void CreateClientView<TOrders, TInfo>(TOrders orders, TInfo info, GameObject prefab) 
        where TOrders : IGameObjectOrders 
        where TInfo : IGameObjectInfo
    {
        var instance = Instantiate(prefab);
        var view = instance.GetComponent<ModelSelectableView<TOrders, TInfo>>();
        if (view == null)
            throw new Exception("Prefab not contains View script.");

        IPlayerState player;
        if (!mClientOtherPlayers.TryGetValue(info.PlayerID, out player))
            throw new Exception("Unknown player.");

        view.Map = MapView;
        view.IsClient = true;
        view.OwnershipRelation = 
                player.ID == Player.ID ? ObjectOwnershipRelation.My : 
                player.Team == Player.Team ? ObjectOwnershipRelation.Ally :
                ObjectOwnershipRelation.Enemy;
        view.Updater = Updater;
        view.SyncContext = SyncContext;
        view.LoadModel(orders, info);

        instance.transform.parent = MapView.ChildContainer.transform;
        instance.transform.localPosition = MapView.GetWorldPosition(info.Position);
    }

    private void ClientOnCentralBuildingCreated(ICentralBuildingOrders centralBuildingOrders, ICentralBuildingInfo centralBuildingInfo)
    {
        CreateClientView(centralBuildingOrders, centralBuildingInfo, CentralBuildingPrefab);
    }

    private void EnemyFactoryOnViewCreated(SelectableView obj)
    {
        obj.OwnershipRelation = ObjectOwnershipRelation.Enemy;
    }

    private void ClientOnBuildingTemplateCreated(IBuildingTemplateOrders arg1, IBuildingTemplateInfo arg2)
    {
        CreateClientView(arg1, arg2, BuildingTemplatePrefab);
    }

    private void ClientOnBarrakCreated(IBarrakOrders arg1, IBarrakInfo arg2)
    {
        CreateClientView(arg1, arg2, BarrakPrefab);
    }

    private void ClientOnTurretCreated(ITurretOrders arg1, ITurretInfo arg2)
    {
        CreateClientView(arg1, arg2, TurretPrefab);
    }

    private void ClientOnWorkerCreated(IWorkerOrders workerOrders, IWorkerInfo workerInfo)
    {
        CreateClientView(workerOrders, workerInfo, WorkerPrefab);
    }

    private void ClientOnMiningCampCreated(IMinigCampOrders orders, IMinigCampInfo info)
    {
        CreateClientView(orders, info, MiningCampPrefab);
    }

    private void ClientOnBuildersLabCreated(IBuildersLabOrders orders, IBuildersLabInfo info)
    {
        CreateClientView(orders, info, BuildersLabPrefab);
    }

    private void ClientOnWarriorsLabCreated(IWarriorsLabOrders orders, IWarriorsLabInfo info)
    {
        CreateClientView(orders, info, WarriorsLabPrefab);
    }

    private void ControlledFactoryOnViewCreated(SelectableView selectableView)
    {
        selectableView.OwnershipRelation = ObjectOwnershipRelation.My;
    }

    private MapView CreateMap(IMapData map, bool generateNavMesh)
    {
        if (MapPrefab == null)
            throw new Exception("Map prefab is not set.");

        var mapInstance = Instantiate(MapPrefab);
        var view = mapInstance.GetComponentInChildren<MapView>();
        if (view == null)
            throw new Exception("Map prefab not contains MapView.");

        view.LoadMap(map, generateNavMesh);
        mapInstance.transform.parent = transform;
        MapLoaded?.Invoke(map);
        return view;
    }

    void Update()
    {
        if (GameUtils.CurrentMode == GameMode.Server)
        {
            mGame.Update(TimeSpan.FromSeconds(Time.deltaTime));
        }
    }

    public void AddResources()
    {
        if (GameUtils.CurrentMode == GameMode.Server)
            ((Player)Player).Money.Store(100);
    }

    public void Close()
    {
        SceneManager.LoadScene("Start");
    }

    void OnDestroy()
    {
        if (mServer != null)
            mServer.Shutdown();

        if (mClient != null)
            mClient.Shutdown();
    }
}
