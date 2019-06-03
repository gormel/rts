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
        private readonly UnitySyncContext mSyncContext;
        private readonly Game mGame;
        private readonly MapView mMap;
        private readonly GameObject mWorkerPrefab;
        private readonly GameObject mBuildingTemplatePrefab;
        private readonly GameObject mCentralBuildingPrefab;

        public event Action<SelectableView> ViewCreated;

        public Factory(Root root)
        {
            mServer = root.mServer;
            mSyncContext = root.SyncContext;
            mGame = root.mGame;
            mMap = root.MapView;
            mWorkerPrefab = root.WorkerPrefab;
            mBuildingTemplatePrefab = root.BuildingTemplatePrefab;
            mCentralBuildingPrefab = root.CentralBuildingPrefab;
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
            MapView = CreateMap(mGame.Map.Data, true);

            var enemyFactory = new Factory(this);
            var controlledFactory = new Factory(this);
            controlledFactory.ViewCreated += ControlledFactoryOnViewCreated;
            enemyFactory.ViewCreated += EnemyFactoryOnViewCreated;

            var player = new Player(controlledFactory);
            Player = player;
            player.Money.Store(100000);
            mGame.AddPlayer(player);

            mServer.Listen(SyncContext, enemyFactory, mGame);

            player.CreateWorker(new Vector2(Random.Range(0, 20), Random.Range(0, 20))).ContinueWith(t => mGame.PlaceObject(t.Result));
        }

        if (GameUtils.CurrentMode == GameMode.Client)
        {
            mClient = new RtsClient(SyncContext);

            mClient.MapLoaded += data => MapView = CreateMap(data, false);
            mClient.PlayerConnected += state => Player = state;

            mClient.WorkerCreated += ClientOnWorkerCreated;
            mClient.BuildingTemplateCreated += ClientOnBuildingTemplateCreated;
            mClient.CentralBuildingCreated += ClientOnCentralBuildingCreated;

            mClient.ObjectDestroyed += ClientOnObjectDestroyed;

            mClient.Listen();
        }
    }

    private void ClientOnObjectDestroyed(IGameObjectInfo objectInfo)
    {
        for (int i = 0; i < MapView.ChildContainer.transform.childCount; i++)
        {
            var child = MapView.ChildContainer.transform.GetChild(i);
            var view = child.GetComponent<IInfoIdProvider>();
            if (view != null && view.ID == objectInfo.ID)
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
        view.IsControlable = info.PlayerID == Player.ID;
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
        obj.IsControlable = false;
    }

    private void ClientOnBuildingTemplateCreated(IBuildingTemplateOrders arg1, IBuildingTemplateInfo arg2)
    {
        CreateClientView(arg1, arg2, BuildingTemplatePrefab);
    }

    private void ClientOnWorkerCreated(IWorkerOrders workerOrders, IWorkerInfo workerInfo)
    {
        CreateClientView(workerOrders, workerInfo, WorkerPrefab);
    }

    private void ControlledFactoryOnViewCreated(SelectableView selectableView)
    {
        selectableView.IsControlable = true;
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
