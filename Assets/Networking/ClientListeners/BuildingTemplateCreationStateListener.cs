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
    class ClientBuildingTemplateState : IBuildingTemplateInfo, IStateHolder<BuildingTemplateState>
    {
        public BuildingTemplateState State { get; private set; } = new BuildingTemplateState();

        public Guid ID => Guid.Parse(State.Base.Base.ID.Value);
        public Guid PlayerID => Guid.Parse(State.Base.Base.PlayerID.Value);
        public Vector2 Position => State.Base.Base.Position.ToUnity();
        public float Health => State.Base.Base.Health;
        public float MaxHealth => State.Base.Base.MaxHealth;
        public Vector2 Size => State.Base.Size.ToUnity();
        public float Progress => State.Progress;
        public int AttachedWorkers => State.AttachedWorkers;
        public float ViewRadius => State.Base.Base.ViewRadius;

        public void ResetState()
        {
            State = new BuildingTemplateState();
        }
    }

    class ClientBuildingTemplateOrders : IBuildingTemplateOrders
    {
        private readonly BuildingTemplateService.BuildingTemplateServiceClient mClient;
        private readonly string mID;

        public ClientBuildingTemplateOrders(BuildingTemplateService.BuildingTemplateServiceClient client, Guid id)
        {
            mClient = client;
            mID = id.ToString();
        }

        public Task Cancel()
        {
            return mClient.CancelAsync(new CancelRequest { BuildingTemplateID = new ID { Value = mID } }).ResponseAsync;
        }
    }

    class BuildingTemplateCreationStateListener : CommonCreationStateListener<
        IBuildingTemplateOrders,
        IBuildingTemplateInfo,
        ClientBuildingTemplateOrders,
        ClientBuildingTemplateState,
        BuildingTemplateState,
        BuildingTemplateService.BuildingTemplateServiceClient>
    {
        public BuildingTemplateCreationStateListener(UnitySyncContext syncContext) 
            : base(syncContext)
        {
        }

        protected override AsyncServerStreamingCall<BuildingTemplateState> GetCreationCall(BuildingTemplateService.BuildingTemplateServiceClient client, CancellationToken token)
        {
            return client.ListenCreation(new Empty(), cancellationToken: token);
        }

        protected override AsyncServerStreamingCall<BuildingTemplateState> GetUpdatesCall(BuildingTemplateService.BuildingTemplateServiceClient client, ID id, CancellationToken token)
        {
            return client.ListenState(id, cancellationToken: token);
        }

        protected override BuildingTemplateService.BuildingTemplateServiceClient CreateClient(Channel channel)
        {
            return new BuildingTemplateService.BuildingTemplateServiceClient(channel);
        }

        protected override ClientBuildingTemplateOrders CreateOrders(BuildingTemplateService.BuildingTemplateServiceClient client, Guid id)
        {
            return new ClientBuildingTemplateOrders(client, id);
        }
    }
}