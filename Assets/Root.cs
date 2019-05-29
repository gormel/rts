using System;
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
using Assets.Utils;
using Assets.Views;
using Assets.Views.Base;
using Grpc.Core;
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

        public event Action<SelectableView> ViewCreated;

        public Factory(Game game, MapView map, GameObject workerPrefab, GameObject buildingTemplatePrefab, GameObject centralBuildingPrefab)
        {
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

    private class WOROET : TestService.TestServiceBase
    {
        public override async Task GetMessages(Message request, IServerStreamWriter<Message> responseStream, ServerCallContext context)
        {
            int a = 0;
            while (true)
            {
                await responseStream.WriteAsync(new Message()
                {
                    Text = (a++).ToString()
                });
                await Task.Delay(3000);
            }
        }
    }

    public GameObject MapPrefab;
    public GameObject WorkerPrefab;
    public GameObject BuildingTemplatePrefab;
    public GameObject CentralBuildingPrefab;
    
    public Player Player { get; private set; }
    private Game Game { get; set; }
    public MapView MapView { get; private set; }

    void Start()
    {
        if (GameUtils.CurrentMode == GameMode.Server)
        {
            Task.Run(() =>
            {
                try
                {
                    new Server()
                    {
                        Services = { TestService.BindService(new WOROET()) },
                        Ports = { new ServerPort(GameUtils.IP.ToString(), GameUtils.Port, ServerCredentials.Insecure) }
                    }.Start();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            });

            Game = new Game();
            MapView = CreateMap(Game.Map.Data);
            var controlledFactory = new Factory(Game, MapView, WorkerPrefab, BuildingTemplatePrefab, CentralBuildingPrefab);
            controlledFactory.ViewCreated += ControlledFactoryOnViewCreated;
            Player = new Player(controlledFactory);
            Player.Money.Store(100000);
            Game.AddPlayer(Player);
            Game.PlaceObject(Player.CreateWorker(new Vector2(Random.Range(0, 20), Random.Range(0, 20))));
        }

        if (GameUtils.CurrentMode == GameMode.Client)
        {
            ProcessMessage(new TestService.TestServiceClient(new Channel(GameUtils.IP.ToString(), GameUtils.Port, ChannelCredentials.Insecure)));
        }
    }

    private async void ProcessMessage(TestService.TestServiceClient client)
    {
        var stream = client.GetMessages(new Message()).ResponseStream;
        while (await stream.MoveNext())
        {
            var mess = stream.Current;
            Debug.Log(mess.Text);
        }
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
            Game.Update(TimeSpan.FromSeconds(Time.deltaTime));
        }
    }
}
