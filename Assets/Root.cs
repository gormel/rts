using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Core;
using Assets.Core.Game;
using Assets.Core.GameObjects;
using Assets.Core.GameObjects.Base;
using Assets.Core.GameObjects.Final;
using Assets.Core.Map;
using Assets.Utils;
using Assets.Views;
using UnityEngine;
using GameObject = UnityEngine.GameObject;

class Root : MonoBehaviour, IGameObjectFactory
{
    public GameObject MapPrefab;
    public GameObject WorkerPrefab;
    public GameObject BuildingTemplatePrefab;
    public GameObject CentralBuildingPrefab;

    public Game Game { get; private set; }
    public MapView MapView { get; private set; }
    public Player Controller { get; private set; }

    void Start()
    {
        Game = new Game(this);
        Controller = Game.GreenPlayer;
        Controller.Money.Store(1000000);

        MapView = CreateMap(Game.Map);

        Game.PlaceObject(CreateWorker(Controller, new Vector2(14, 10)));
        Game.PlaceObject(CreateWorker(Controller, new Vector2(15, 10)));
        Game.PlaceObject(CreateWorker(Controller, new Vector2(16, 10)));
    }

    private MapView CreateMap(Map map)
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
        Game.Update(TimeSpan.FromSeconds(Time.deltaTime));
    }

    private TModel CreateModelAndView<TView, TModel>(GameObject prefab, Func<TView, TModel> createModel, Vector2 position)
        where TView : ModelSelectableView<TModel>
        where TModel : RtsGameObject
    {
        if (WorkerPrefab == null)
            throw new Exception("Worker prefab is not set.");

        var instance = Instantiate(prefab);
        var view = instance.GetComponent<TView>();
        if (view == null)
            throw new Exception("Worker prefab not contains WorkerView.");

        var result = createModel(view);
        view.Map = MapView;
        view.LoadModel(result);

        instance.transform.parent = MapView.ChildContainer.transform;
        instance.transform.localPosition = GameUtils.GetPosition(position, Game.Map);

        return result;
    }

    public Worker CreateWorker(Player controller, Vector2 position)
    {
        return CreateModelAndView<WorkerView, Worker>(
            WorkerPrefab, 
            view => new Worker(Game, controller, view, position),
            position);
    }

    public BuildingTemplate CreateBuildingTemplate(Player controller, Vector2 position, Func<Vector2, Building> building, TimeSpan buildTime, Vector2 size, float maxHealth)
    {
        return CreateModelAndView<BuildingTemplateView, BuildingTemplate>(
            BuildingTemplatePrefab,
            view => new BuildingTemplate(Game, controller, building, buildTime, size, position, maxHealth, view),
            position
        );
    }

    public CentralBuilding CreateCentralBuilding(Player controller, Vector2 position)
    {
        return CreateModelAndView<CentralBuildingView, CentralBuilding>(
            CentralBuildingPrefab,
            view => new CentralBuilding(Game, controller, position, view), 
            position
        );
    }
}
