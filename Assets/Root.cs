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

            return result;
        }

        public Worker CreateWorker(Vector2 position)
        {
            var worker = CreateModelAndView<WorkerView, Worker, IWorkerOrders, IWorkerInfo>(
                mWorkerPrefab,
                view => new Worker(mGame, view, position),
                position
            );
            var t = mNetworkManager.Server.ObjectCreated(worker, worker, null);
            return worker;
        }

        public BuildingTemplate CreateBuildingTemplate(Vector2 position, Func<Vector2, Building> building, TimeSpan buildTime, Vector2 size, float maxHealth)
        {
            var template = CreateModelAndView<BuildingTemplateView, BuildingTemplate, IBuildingTemplateOrders, IBuildingTemplateInfo>(
                mBuildingTemplatePrefab,
                view => new BuildingTemplate(mGame, building, buildTime, size, position, maxHealth, view),
                position
            );

            var t = mNetworkManager.Server.ObjectCreated(template, template, new BuildingTemplateServerPackageProcessor(template, template));
            return template;
        }

        public CentralBuilding CreateCentralBuilding(Vector2 position)
        {
            var centralBuilding = CreateModelAndView<CentralBuildingView, CentralBuilding, ICentralBuildingOrders, ICentralBuildingInfo>(
                mCentralBuildingPrefab,
                view => new CentralBuilding(mGame, position, view),
                position
            );

            var t = mNetworkManager.Server.ObjectCreated(centralBuilding, centralBuilding, null);
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
            NetworkManager.Server.LoadMap(Game.Map.Data);
            NetworkManager.Server.OnConnected += ServerOnOnConnected;

            Game = new Game();
            MapView = CreateMap(Game.Map);

            var controlledFactory = new Factory(Game, MapView, WorkerPrefab, BuildingTemplatePrefab, CentralBuildingPrefab, NetworkManager);
            controlledFactory.ViewCreated += ControlledFactoryOnViewCreated;
            Player = new Player(controlledFactory);
            Player.Money.Store(100000);
            Game.AddPlayer(Player);
            Game.PlaceObject(Player.CreateWorker(new Vector2(14, 10)));
        }
    }

    private void ControlledFactoryOnViewCreated(SelectableView selectableView)
    {
        selectableView.IsControlable = true;
    }

    private void ServerOnOnConnected(TcpClient tcpClient)
    {
        var clientPlayer = new Player(new Factory(Game, MapView, WorkerPrefab, BuildingTemplatePrefab, CentralBuildingPrefab, NetworkManager));
        clientPlayer.Money.Store(10000);
        Game.AddPlayer(clientPlayer);
        Game.PlaceObject(clientPlayer.CreateWorker(new Vector2(Random.Range(0, 20), Random.Range(0, 20))));
    }

    private MapView CreateMap(Map map)
    {
        if (MapPrefab == null)
            throw new Exception("Map prefab is not set.");

        var mapInstance = Instantiate(MapPrefab);
        var view = mapInstance.GetComponentInChildren<MapView>();
        if (view == null)
            throw new Exception("Map prefab not contains MapView.");

        view.LoadMap(map.Data);
        mapInstance.transform.parent = transform;
        return view;
    }

    void Update()
    {
        Game.Update(TimeSpan.FromSeconds(Time.deltaTime));
    }
}
