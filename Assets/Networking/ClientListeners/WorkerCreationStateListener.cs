using System;
using System.Threading;
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
        public WorkerState State { get; private set; } = new WorkerState();

        public Guid ID => Guid.Parse(State.Base.Base.ID.Value);
        public Guid PlayerID => Guid.Parse(State.Base.Base.PlayerID.Value);
        public Vector2 Position => State.Base.Base.Position.ToUnity();
        public float RecivedDamage => State.Base.Base.RecivedDamage;
        public float MaxHealth => State.Base.Base.MaxHealth;
        public float Speed => State.Base.Speed;
        public Vector2 Direction => State.Base.Direction.ToUnity();
        public Vector2 Destignation => State.Base.Destignation.ToUnity();
        public bool IsBuilding => State.IsBuilding;
        public bool IsAttachedToMiningCamp => State.IsAttachedToMiningCamp;
        public float ViewRadius => State.Base.Base.ViewRadius;

        public void ResetState()
        {
            State = new WorkerState();
        }
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
                UnitID = new ID {Value = mID}
            }).ResponseAsync;
        }

        public Task Stop()
        {
            return mClient.StopAsync(new StopRequest
            {
                UnitUD = new ID {Value = mID}
            }).ResponseAsync;
        }

        public async Task<Guid> PlaceCentralBuildingTemplate(Vector2Int position)
        {
            var resp = await mClient.PlaceCentralBuildingTemplateAsync(new PlaceCentralBuildingTemplateRequest
            {
                Position = new Vector {X = position.x, Y = position.y},
                WorkerID = new ID {Value = mID}
            });
            return Guid.Parse(resp.Value);
        }

        public async Task<Guid> PlaceBarrakTemplate(Vector2Int position)
        {
            var resp = await mClient.PlaceBarrakTemplateAsync(new PlaceBarrakTemplateRequest
            {
                Position = new Vector {X = position.x, Y = position.y},
                WorkerID = new ID {Value = mID}
            });
            return Guid.Parse(resp.Value);
        }

        public async Task<Guid> PlaceTurretTemplate(Vector2Int position)
        {
            var resp = await mClient.PlaceTurretTemplateAsync(new PlaceTurretTemplateRequest
            {
                Position = new Vector {X = position.x, Y = position.y},
                WorkerID = new ID {Value = mID}
            });
            return Guid.Parse(resp.Value);
        }

        public async Task<Guid> PlaceBuildersLabTemplate(Vector2Int position)
        {
            var resp = await mClient.PlaceBuildersLabTemplateAsync(new PlaceBuildersLabTemplateRequest
            {
                Position = new Vector {X = position.x, Y = position.y},
                WorkerID = new ID {Value = mID}
            });
            return Guid.Parse(resp.Value);
        }

        public async Task<Guid> PlaceWarriorsLabTemplate(Vector2Int position)
        {
            var resp = await mClient.PlaceWarriorsLabTemplateAsync(new PlaceWarriorsLabTemplateRequest
            {
                Position = new Vector {X = position.x, Y = position.y},
                WorkerID = new ID {Value = mID}
            });
            return Guid.Parse(resp.Value);
        }

        public Task AttachAsBuilder(Guid templateId)
        {
            return mClient.AttachAsBuilderAsync(new AttachAsBuilderRequest
            {
                BuildingTemplateID = new ID {Value = templateId.ToString()},
                WorkerID = new ID {Value = mID}
            }).ResponseAsync;
        }

        public Task AttachToMiningCamp(Guid campId)
        {
            return mClient.AttachToMiningCampAsync(new AttachToMiningCampRequest
            {
                WorkerID = new ID {Value = mID},
                CampID = new ID {Value = campId.ToString()}
            }).ResponseAsync;
        }

        public async Task<Guid> PlaceMiningCampTemplate(Vector2Int position)
        {
            var resp = await mClient.PlaceMiningCampTemplateAsync(new PlaceMiningCampTemplateRequest
            {
                Position = new Vector {X = position.x, Y = position.y},
                WorkerID = new ID {Value = mID}
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

        protected override AsyncServerStreamingCall<WorkerState> GetCreationCall(
            WorkerService.WorkerServiceClient client, CancellationToken token)
        {
            return client.ListenCreation(new Empty(), cancellationToken: token);
        }

        protected override AsyncServerStreamingCall<WorkerState> GetUpdatesCall(
            WorkerService.WorkerServiceClient client, ID id, CancellationToken token)
        {
            return client.ListenState(id, cancellationToken: token);
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