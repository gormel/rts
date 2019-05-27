using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using Assets.Core;
using Assets.Core.Game;
using Assets.Core.GameObjects;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using Assets.Core.Map;
using Assets.Networking;
using Assets.Networking.NetworkCustoms;
using Assets.Networking.ServerClientPackages;
using Assets.Utils;
using Assets.Views;
using Assets.Views.Base;
using UnityEngine;
using GameObject = UnityEngine.GameObject;
using Random = UnityEngine.Random;

class Root : MonoBehaviour
{
    private class Factory : IGameObjectFactory
    {
        private readonly Game mGame;
        private readonly MapView mMap;
        private readonly GameObject mWorkerPrefab;
        private readonly GameObject mBuildingTemplatePrefab;
        private readonly GameObject mCentralBuildingPrefab;
        private readonly NetworkManager mNetworkManager;

        public event Action<SelectableView> ViewCreated;

        public Factory(Game game, MapView map, GameObject workerPrefab, GameObject buildingTemplatePrefab, GameObject centralBuildingPrefab, NetworkManager networkManager)
        {
            mGame = game;
            mMap = map;
            mWorkerPrefab = workerPrefab;
            mBuildingTemplatePrefab = buildingTemplatePrefab;
            mCentralBuildingPrefab = centralBuildingPrefab;
            mNetworkManager = networkManager;
        }

        private TModel CreateModelAndView<TView, TModel, TOrders, TInfo>(GameObject prefab, Func<TView, TModel> createModel, Vector2 position)
            where TView : ModelSelectableView<TOrders, TInfo>
            where TOrders : IGameObjectOrders
            where TInfo : IGameObjectInfo
            where TModel : RtsGameObject, TOrders, TInfo
        {
            var instance = Instantiate(prefab);
            var view = instance.GetComponent<TView>();
            if (view == null)
                throw new Exception("Prefab not contains View script.");

            var result = createModel(view);
            view.Map = mMap;
            view.LoadModel(result, result);

            instance.transform.parent = mMap.ChildContainer.transform;
            instance.transform.localPosition = mMap.GetWorldPosition(position);

            ViewCreated?.Invoke(view);

            return result;
        }

        public Worker CreateWorker(Vector2 position)
        {
            var worker = CreateModelAndView<WorkerView, Worker, IWorkerOrders, IWorkerInfo>(
                mWorkerPrefab,
                view => new Worker(mGame, view, position),
                position
            );
            mNetworkManager.Server.ObjectCreated<IWorkerOrders, IWorkerInfo>(worker, worker, new WorkerServerPackageProcessor(worker, worker));
            return worker;
        }

        public BuildingTemplate CreateBuildingTemplate(Vector2 position, Func<Vector2, Building> building, TimeSpan buildTime, Vector2 size, float maxHealth)
        {
            var template = CreateModelAndView<BuildingTemplateView, BuildingTemplate, IBuildingTemplateOrders, IBuildingTemplateInfo>(
                mBuildingTemplatePrefab,
                view => new BuildingTemplate(mGame, building, buildTime, size, position, maxHealth, view),
                position
            );

            mNetworkManager.Server.ObjectCreated<IBuildingTemplateOrders, IBuildingTemplateInfo>(template, template, 
                new BuildingTemplateServerPackageProcessor(template, template));
            return template;
        }

        public CentralBuilding CreateCentralBuilding(Vector2 position)
        {
            var centralBuilding = CreateModelAndView<CentralBuildingView, CentralBuilding, ICentralBuildingOrders, ICentralBuildingInfo>(
                mCentralBuildingPrefab,
                view => new CentralBuilding(mGame, position, view),
                position
            );

            mNetworkManager.Server.ObjectCreated<ICentralBuildingOrders, ICentralBuildingInfo>(centralBuilding, centralBuilding, null);
            return centralBuilding;
        }
    }
    public GameObject MapPrefab;
    public GameObject WorkerPrefab;
    public GameObject BuildingTemplatePrefab;
    public GameObject CentralBuildingPrefab;

    public NetworkManager NetworkManager;
    public Player Player { get; private set; }
    private Game Game { get; set; }
    public MapView MapView { get; private set; }

    void Start()
    {
        if (GameUtils.CurrentMode == GameMode.Server)
        {
            Game = new Game();
            MapView = CreateMap(Game.Map.Data);

            NetworkManager.Listen(GameUtils.Port);

            NetworkManager.Server.LoadMap(Game.Map.Data);
            NetworkManager.Server.ClientConnected += ServerOnClientConnected;

            var controlledFactory = new Factory(Game, MapView, WorkerPrefab, BuildingTemplatePrefab, CentralBuildingPrefab, NetworkManager);
            controlledFactory.ViewCreated += ControlledFactoryOnViewCreated;
            Player = new Player(controlledFactory);
            Player.Money.Store(100000);
            Game.AddPlayer(Player);
            Game.PlaceObject(Player.CreateWorker(new Vector2(Random.Range(0, 20), Random.Range(0, 20))));
        }

        if (GameUtils.CurrentMode == GameMode.Client)
        {
            NetworkManager.Connect(GameUtils.IP, GameUtils.Port);

            NetworkManager.Client.LoadMapData += ClientOnLoadMapData;
            NetworkManager.Client.ListenObjectType<IBuildingTemplateOrders, IBuildingTemplateInfo>(
                OnCreateBuildingTemplate, 
                new BuildingTemplateClientOrdersFactory(), 
                new BuildingTemplateClientInfoFactory()
            );
            NetworkManager.Client.ListenObjectType<IWorkerOrders, IWorkerInfo>(
                OnWorkerCreate, 
                new WorkerClientOrderFactory(), 
                new WorkerClientInfoFactory()
            );

            NetworkManager.Client.Connect();
        }
    }

    private static TView CreateClientView<TView, TOrders, TInfo>(GameObject prefab, TOrders orders, TInfo info, Vector2 position, MapView map)
        where TView : ModelSelectableView<TOrders, TInfo>
        where TOrders : IGameObjectOrders
        where TInfo : IGameObjectInfo
    {
        var instance = Instantiate(prefab);
        var view = instance.GetComponent<TView>();
        if (view == null)
            throw new Exception("Prefab not contains View script.");

        view.Map = map;
        view.LoadModel(orders, info);

        instance.transform.parent = map.ChildContainer.transform;
        instance.transform.localPosition = map.GetWorldPosition(position);

        return view;
    }

    private void OnWorkerCreate(IWorkerOrders orders, IWorkerInfo info, float x, float y)
    {
        var view = CreateClientView<WorkerView, IWorkerOrders, IWorkerInfo>(WorkerPrefab, orders, info, new Vector2(x, y), MapView);
        view.IsClient = true;
        view.IsControlable = true;
    }

    private void ClientOnLoadMapData(LoadMapDataPackage obj)
    {
        MapView = CreateMap(new MapData(obj.Width, obj.Length, obj.Heights));
    }

    private void OnCreateBuildingTemplate(IBuildingTemplateOrders orders, IBuildingTemplateInfo info, float x, float y)
    {
    }

    private void ControlledFactoryOnViewCreated(SelectableView selectableView)
    {
        selectableView.IsControlable = true;
    }

    private void ServerOnClientConnected(TcpClient tcpClient)
    {
        var clientPlayer = new Player(new Factory(Game, MapView, WorkerPrefab, BuildingTemplatePrefab, CentralBuildingPrefab, NetworkManager));
        clientPlayer.Money.Store(10000);
        Game.AddPlayer(clientPlayer);
        Game.PlaceObject(clientPlayer.CreateWorker(new Vector2(Random.Range(0, 20), Random.Range(0, 20))));
    }

    private MapView CreateMap(IMapData map)
    {
        if (MapPrefab == null)
            throw new Exception("Map prefab is not set.");

        var mapInstance = Instantiate(MapPrefab);
        var view = mapInstance.GetComponentInChildren<MapView>();
        if (view == null)
            throw new Exception("Map prefab not contains MapView.");

        view.LoadMap(map);
        mapInstance.transform.parent = transform;
        return view;
    }

    void Update()
    {
        if (GameUtils.CurrentMode == GameMode.Server)
        {
            Game.Update(TimeSpan.FromSeconds(Time.deltaTime));
        }
    }
}
