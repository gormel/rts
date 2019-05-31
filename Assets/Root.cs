﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Assets.Core;
using Assets.Core.Game;
using Assets.Core.GameObjects;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using Assets.Core.Map;
using Assets.Networking;
using Assets.Networking.Services;
using Assets.Utils;
using Assets.Views;
using Assets.Views.Base;
using Grpc.Core;
using UnityEngine;
using GameObject = UnityEngine.GameObject;
using Random = UnityEngine.Random;
using Server = Grpc.Core.Server;

class Root : MonoBehaviour
{
    private class Factory : IGameObjectFactory
    {
        private readonly RtsServer mServer;
        private readonly Game mGame;
        private readonly MapView mMap;
        private readonly GameObject mWorkerPrefab;
        private readonly GameObject mBuildingTemplatePrefab;
        private readonly GameObject mCentralBuildingPrefab;

        public event Action<SelectableView> ViewCreated;

        public Factory(RtsServer server, Game game, MapView map, GameObject workerPrefab, GameObject buildingTemplatePrefab, GameObject centralBuildingPrefab)
        {
            mServer = server;
            mGame = game;
            mMap = map;
            mWorkerPrefab = workerPrefab;
            mBuildingTemplatePrefab = buildingTemplatePrefab;
            mCentralBuildingPrefab = centralBuildingPrefab;
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
            mServer.WorkerRegistrator.Register(worker, worker);
            return worker;
        }

        public BuildingTemplate CreateBuildingTemplate(Vector2 position, Func<Vector2, Building> building, TimeSpan buildTime, Vector2 size, float maxHealth)
        {
            var template = CreateModelAndView<BuildingTemplateView, BuildingTemplate, IBuildingTemplateOrders, IBuildingTemplateInfo>(
                mBuildingTemplatePrefab,
                view => new BuildingTemplate(mGame, building, buildTime, size, position, maxHealth, view),
                position
            );
            
            return template;
        }

        public CentralBuilding CreateCentralBuilding(Vector2 position)
        {
            var centralBuilding = CreateModelAndView<CentralBuildingView, CentralBuilding, ICentralBuildingOrders, ICentralBuildingInfo>(
                mCentralBuildingPrefab,
                view => new CentralBuilding(mGame, position, view),
                position
            );
            
            return centralBuilding;
        }
    }

    public GameObject MapPrefab;
    public GameObject WorkerPrefab;
    public GameObject BuildingTemplatePrefab;
    public GameObject CentralBuildingPrefab;
    public UnitySyncContext SyncContext;

    private RtsServer mServer;
    private RtsClient mClient;
    private Game mGame;

    public IPlayerState Player { get; private set; }
    public MapView MapView { get; private set; }

    void Start()
    {
        if (GameUtils.CurrentMode == GameMode.Server)
        {
            mGame = new Game();
            mServer = new RtsServer();
            MapView = CreateMap(mGame.Map.Data);

            var enemyFactory = new Factory(mServer, mGame, MapView, WorkerPrefab, BuildingTemplatePrefab, CentralBuildingPrefab);
            var controlledFactory = new Factory(mServer, mGame, MapView, WorkerPrefab, BuildingTemplatePrefab, CentralBuildingPrefab);
            controlledFactory.ViewCreated += ControlledFactoryOnViewCreated;

            var player = new Player(controlledFactory);
            Player = player;
            player.Money.Store(100000);
            mGame.AddPlayer(player);

            mServer.Listen(SyncContext, enemyFactory, mGame);

            mGame.PlaceObject(player.CreateWorker(new Vector2(Random.Range(0, 20), Random.Range(0, 20))));
        }

        if (GameUtils.CurrentMode == GameMode.Client)
        {
            mClient = new RtsClient(SyncContext);

            mClient.MapLoaded += data => MapView = CreateMap(data);
            mClient.PlayerConnected += state => Player = state;
            mClient.WorkerCreated += ClientOnWorkerCreated;

            mClient.Listen();
        }
    }

    private void ClientOnWorkerCreated(IWorkerOrders workerOrders, IWorkerInfo workerInfo)
    {
        var instance = Instantiate(WorkerPrefab);
        var view = instance.GetComponent<WorkerView>();
        if (view == null)
            throw new Exception("Prefab not contains View script.");
        
        view.Map = MapView;
        view.LoadModel(workerOrders, workerInfo);
        view.IsControlable = true;
        view.IsClient = true;

        instance.transform.parent = MapView.ChildContainer.transform;
        instance.transform.localPosition = MapView.GetWorldPosition(workerInfo.Position);
    }

    private void ControlledFactoryOnViewCreated(SelectableView selectableView)
    {
        selectableView.IsControlable = true;
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
