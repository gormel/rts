using System;
using System.Threading.Tasks;
using Assets.Core.Game;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using Assets.Core.Map;
using Assets.Networking;
using Assets.Utils;
using Assets.Views;
using Assets.Views.Base;
using Assets.Views.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameObject = UnityEngine.GameObject;

class Root : MonoBehaviour
{
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

        public event Action<SelectableView> ViewCreated;

        public Factory(Root root)
        {
            mServer = root.mServer;
            mSyncContext = root.SyncContext;
            mUpdater = root.Updater;
            mGame = root.mGame;
            mMap = root.MapView;
            mRangedWarriorPrefab = root.RangedWarriorPrefab;
            mMeeleeWarriorPrefab = root.MeeleeWarriorPrefab;
            mWorkerPrefab = root.WorkerPrefab;
            mBuildingTemplatePrefab = root.BuildingTemplatePrefab;
            mCentralBuildingPrefab = root.CentralBuildingPrefab;
            mMiningCampPrefab = root.MiningCampPrefab;
            mBarrakPrefab = root.BarrakPrefab;
            mTurretPrefab = root.TurretPrefab;
            mBuildersLabPrefab = root.BuildersLabPrefab;
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

        public async Task<BuildingTemplate> CreateBuildingTemplate(Vector2 position, Func<Vector2, Task<Building>> building, TimeSpan buildTime, Vector2 size, float maxHealth)
        {
            var template = await CreateModelAndView<BuildingTemplateView, BuildingTemplate, IBuildingTemplateOrders, IBuildingTemplateInfo>(
                mBuildingTemplatePrefab,
                view => new BuildingTemplate(mGame, building, buildTime, size, position, maxHealth, view),
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
    public GameObject BuildingTemplatePrefab;
    public GameObject CentralBuildingPrefab;
    public GameObject MiningCampPrefab;
    public GameObject TurretPrefab;
    public GameObject BuildersLabPrefab;
    
    public UnitySyncContext SyncContext;
    public ExternalUpdater Updater;

    private RtsServer mServer;
    private RtsClient mClient;
    private Game mGame;

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

            var enemyFactory = new Factory(this);
            var controlledFactory = new Factory(this);
            var allyFactory = new Factory(this);
            allyFactory.ViewCreated += AllyFactoryOnViewCreated;
            controlledFactory.ViewCreated += ControlledFactoryOnViewCreated;
            enemyFactory.ViewCreated += EnemyFactoryOnViewCreated;

            var player = new Player(controlledFactory, GameUtils.Team);
            Player = player;
            mGame.AddPlayer(player);

            mServer.MessageRecived += OnChatMessageRecived;

            mServer.Listen(SyncContext, enemyFactory, allyFactory, mGame);

            var success = GameUtils.TryCreateBase(mGame, player, out var basePos);
            PlaseCamera(basePos);
        }

        if (GameUtils.CurrentMode == GameMode.Client)
        {
            mClient = new RtsClient(SyncContext);

            mClient.MapLoaded += data => MapView = CreateMap(data, false);
            mClient.BaseCreated += pos => PlaseCamera(pos);
            mClient.PlayerConnected += state => Player = state;
            mClient.DisconnectedFromServer +=() => SceneManager.LoadScene(GuiSceneName);

            mClient.MeeleeWarriorCreated += ClientOnMeeleeWarriorCreated;
            mClient.RangedWarriorCreated += ClientOnRangedWarriorCreated;
            mClient.WorkerCreated += ClientOnWorkerCreated;
            mClient.BuildingTemplateCreated += ClientOnBuildingTemplateCreated;
            mClient.CentralBuildingCreated += ClientOnCentralBuildingCreated;
            mClient.MiningCampCreated += ClientOnMiningCampCreated;
            mClient.BarrakCreated += ClientOnBarrakCreated;
            mClient.TurretCreated += ClientOnTurretCreated;
            mClient.BuildersLabCreated += ClientOnBuildersLabCreated;

            mClient.ObjectDestroyed += ClientOnObjectDestroyed;

            mClient.ChatMessageRecived += OnChatMessageRecived;

            mClient.Listen();
        }
    }

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

    public void PlaseCamera(Vector2 pos)
    {
        var cameraY = Camera.main.transform.position.y;
        var dY = cameraY - MapView.transform.position.y;
        var dX = 0f;
        var dZ = 0f;

        if (Camera.main.transform.eulerAngles.z > 0)
            dX = dY / Mathf.Tan(Camera.main.transform.eulerAngles.z * Mathf.Deg2Rad);

        if (Camera.main.transform.eulerAngles.x > 0)
            dZ = dY / Mathf.Tan(Camera.main.transform.eulerAngles.x * Mathf.Deg2Rad);

        Camera.main.transform.position = new Vector3(pos.x - dX, cameraY, pos.y - dZ);
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

        view.Map = MapView;
        view.IsClient = true;
        view.OwnershipRelation = info.PlayerID == Player.ID ? ObjectOwnershipRelation.My : ;
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

    void OnDestroy()
    {
        if (mServer != null)
            mServer.Shutdown();

        if (mClient != null)
            mClient.Shutdown();
    }
}
