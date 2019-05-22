using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Core;
using Assets.Core.Game;
using Assets.Core.GameObjects;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using Assets.Core.Map;
using Assets.Networking;
using Assets.Utils;
using Assets.Views;
using Assets.Views.Base;
using UnityEngine;
using GameObject = UnityEngine.GameObject;

class Root : MonoBehaviour, IGameObjectFactory
{
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
        Game = new Game();
        Player = new Player(this);
        Player.Money.Store(100000);
        Game.AddPlayer(Player);

        MapView = CreateMap(Game.Map);

        Game.PlaceObject(Player.CreateWorker(new Vector2(14, 10)));
        Game.PlaceObject(Player.CreateWorker(new Vector2(15, 10)));
        Game.PlaceObject(Player.CreateWorker(new Vector2(16, 10)));
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

    private TModel CreateModelAndView<TView, TModel, TOrders, TInfo>(GameObject prefab, Func<TView, TModel> createModel, Vector2 position)
        where TView : ModelSelectableView<TOrders, TInfo>
        where TOrders : IGameObjectOrders
        where TInfo : IGameObjectInfo
        where TModel : RtsGameObject, TOrders, TInfo
    {
        if (WorkerPrefab == null)
            throw new Exception("Worker prefab is not set.");

        var instance = Instantiate(prefab);
        var view = instance.GetComponent<TView>();
        if (view == null)
            throw new Exception("Worker prefab not contains WorkerView.");

        var result = createModel(view);
        view.Map = MapView;
        view.LoadModel(result, result);

        instance.transform.parent = MapView.ChildContainer.transform;
        instance.transform.localPosition = MapView.GetWorldPosition(position);

        return result;
    }

    public Worker CreateWorker(Vector2 position)
    {
        return CreateModelAndView<WorkerView, Worker, IWorkerOrders, IWorkerInfo>(
            WorkerPrefab, 
            view => new Worker(Game, view, position),
            position);
    }

    public BuildingTemplate CreateBuildingTemplate(Vector2 position, Func<Vector2, Building> building, TimeSpan buildTime, Vector2 size, float maxHealth)
    {
        return CreateModelAndView<BuildingTemplateView, BuildingTemplate, IBuildingTemplateOrders, IBuildingTemplateInfo>(
            BuildingTemplatePrefab,
            view => new BuildingTemplate(Game, building, buildTime, size, position, maxHealth, view),
            position
        );
    }

    public CentralBuilding CreateCentralBuilding(Vector2 position)
    {
        return CreateModelAndView<CentralBuildingView, CentralBuilding, ICentralBuildingOrders, ICentralBuildingInfo>(
            CentralBuildingPrefab,
            view => new CentralBuilding(Game, position, view), 
            position
        );
    }
}
