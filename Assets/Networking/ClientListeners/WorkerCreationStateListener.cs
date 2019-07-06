using System;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Final;
using Assets.Networking.ClientListeners;
using Assets.Utils;
using Grpc.Core;
using UnityEngine;

namespace Assets.Networking
{
    class ClientWorkerState : IWorkerInfo, IStateHolder<WorkerState>
    {
        public WorkerState State { get; } = new WorkerState();

        public Guid ID => Guid.Parse(State.Base.Base.ID.Value);
        public Guid PlayerID => Guid.Parse(State.Base.Base.PlayerID.Value);
        public Vector2 Position => State.Base.Base.Position.ToUnity();
        public float Health => State.Base.Base.Health;
        public float MaxHealth => State.Base.Base.MaxHealth;
        public float Speed => State.Base.Speed;
        public Vector2 Direction => State.Base.Direction.ToUnity();
        public Vector2 Destignation => State.Base.Destignation.ToUnity();
    }

    class ClientWorkerOrders : IWorkerOrders
    {
        private readonly WorkerService.WorkerServiceClient mClient;
        private readonly string mID;

        public ClientWorkerOrders(WorkerService.WorkerServiceClient client, Guid id)
        {
            mClient = client;
            mID = id.ToString();
        }

        public Task GoTo(Vector2 position)
        {
            return mClient.GoToAsync(new GoToRequest
            {
                Destignation = position.ToGrpc(),
                WorkerID = new ID { Value = mID }
            }).ResponseAsync;
        }

        public async Task<Guid> PlaceCentralBuildingTemplate(Vector2Int position)
        {
            var resp = await mClient.PlaceCentralBuildingTemplateAsync(new PlaceCentralBuildingTemplateRequest
            {
                Position = new Vector { X = position.x, Y = position.y },
                WorkerID = new ID { Value = mID }
            });
            return Guid.Parse(resp.Value);
        }

        public async Task<Guid> PlaceBarrakTemplate(Vector2Int position)
        {
            var resp = await mClient.PlaceBarrakTemplateAsync(new PlaceBarrakTemplateRequest
            {
                Position = new Vector { X = position.x, Y = position.y },
                WorkerID = new ID { Value = mID }
            });
            return Guid.Parse(resp.Value);
        }

        public Task AttachAsBuilder(Guid templateId)
        {
            return mClient.AttachAsBuilderAsync(new AttachAsBuilderRequest
            {
                BuildingTemplateID = new ID { Value = templateId.ToString() },
                WorkerID = new ID { Value = mID }
            }).ResponseAsync;
        }

        public async Task<Guid> PlaceMiningCampTemplate(Vector2Int position)
        {
            var resp = await mClient.PlaceMiningCampTemplateAsync(new PlaceMiningCampTemplateRequest
            {
                Position = new Vector { X = position.x, Y = position.y },
                WorkerID = new ID { Value = mID }
            });
            return Guid.Parse(resp.Value);
        }
    }

    class WorkerCreationStateListener : CommonCreationStateListener<
        IWorkerOrders, 
        IWorkerInfo, 
        ClientWorkerOrders, 
        ClientWorkerState,
        WorkerState,
        WorkerService.WorkerServiceClient>
    {
        public WorkerCreationStateListener(UnitySyncContext syncContext) : base(syncContext)
        {
        }

        protected override IAsyncStreamReader<WorkerState> GetCreationStream(WorkerService.WorkerServiceClient client)
        {
            return client.ListenCreation(new Empty()).ResponseStream;
        }

        protected override IAsyncStreamReader<WorkerState> GetUpdatesStream(WorkerService.WorkerServiceClient client, ID id)
        {
            return client.ListenState(id).ResponseStream;
        }

        protected override WorkerService.WorkerServiceClient CreateClient(Channel channel)
        {
            return new WorkerService.WorkerServiceClient(channel);
        }

        protected override ClientWorkerOrders CreateOrders(WorkerService.WorkerServiceClient client, Guid id)
        {
            return new ClientWorkerOrders(client, id);
        }
    }
}