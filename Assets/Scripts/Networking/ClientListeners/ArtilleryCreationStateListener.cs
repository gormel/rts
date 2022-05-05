using System;
using System.Threading;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Final;
using Assets.Networking.ClientListeners;
using Assets.Utils;
using Core.GameObjects.Final;
using Grpc.Core;
using UnityEngine;

namespace Assets.Networking
{
    class ClientArtilleryState : IArtilleryInfo, IStateHolder<ArtilleryState>
    {
        public ArtilleryState State { get; private set; } = new ArtilleryState();

        public Guid ID => Guid.Parse(State.Base.Base.ID.Value);
        public Guid PlayerID => Guid.Parse(State.Base.Base.PlayerID.Value);
        public Vector2 Position => State.Base.Base.Position.ToUnity();
        public float RecivedDamage => State.Base.Base.RecivedDamage;
        public float MaxHealth => State.Base.Base.MaxHealth;
        public float Speed => State.Base.Speed;
        public Vector2 Direction => State.Base.Direction.ToUnity();
        public Vector2 Destignation => State.Base.Destignation.ToUnity();
        public float ViewRadius => State.Base.Base.ViewRadius;
        public int Armour => State.Base.Base.Armour;
        public bool LaunchAvaliable => State.LaunchAvaliable;
        public float MissileSpeed => State.MissileSpeed;
        public float MissileRadius => State.MissileRadius;
        public float MissileDamage => State.MissileDamage;
        public float LaunchRange => State.LaunchRange;

        public void ResetState()
        {
            State = new ArtilleryState();
        }

    }

    class ClientArtilleryOrders : IArtilleryOrders
    {
        private readonly ArtilleryService.ArtilleryServiceClient mClient;
        private readonly string mID;

        public ClientArtilleryOrders(ArtilleryService.ArtilleryServiceClient client, Guid id)
        {
            mClient = client;
            mID = id.ToString();
        }
        
        public Task GoTo(Vector2 position)
        {
            return mClient.GoToAsync(new GoToRequest()
            {
                Destignation = position.ToGrpc(),
                UnitID = new ID {Value = mID},
            }).ResponseAsync;
        }

        public Task Stop()
        {
            return mClient.StopAsync(new StopRequest
            {
                UnitUD = new ID {Value = mID},
            }).ResponseAsync;
        }

        public Task Launch(Vector2 target)
        {
            return mClient.LaunchAsync(new LaunchReqest
            {
                UnitID = new ID {Value = mID},
                Target = target.ToGrpc(),
            }).ResponseAsync;
        }
    }
    
    
    class ArtilleryCreationStateListener : CommonCreationStateListener<
        IArtilleryOrders,
        IArtilleryInfo,
        ClientArtilleryOrders,
        ClientArtilleryState,
        ArtilleryState,
        ArtilleryService.ArtilleryServiceClient>
    {
        public ArtilleryCreationStateListener(UnitySyncContext syncContext) 
            : base(syncContext)
        {
        }

        protected override AsyncServerStreamingCall<ArtilleryState> GetCreationCall(ArtilleryService.ArtilleryServiceClient client, CancellationToken token)
        {
            return client.ListenCreation(new Empty(), cancellationToken: token);
        }

        protected override AsyncServerStreamingCall<ArtilleryState> GetUpdatesCall(ArtilleryService.ArtilleryServiceClient client, ID id, CancellationToken token)
        {
            return client.ListenState(id, cancellationToken: token);
        }

        protected override ArtilleryService.ArtilleryServiceClient CreateClient(Channel channel)
        {
            return new ArtilleryService.ArtilleryServiceClient(channel);
        }

        protected override ClientArtilleryOrders CreateOrders(ArtilleryService.ArtilleryServiceClient client, Guid id)
        {
            return new ClientArtilleryOrders(client, id);
        }
    }
}