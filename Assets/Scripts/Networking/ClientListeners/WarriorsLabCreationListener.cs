﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using Assets.Utils;
using Google.Protobuf;
using Grpc.Core;
using UnityEngine;

namespace Assets.Networking.ClientListeners
{
    internal class ClientWarriorsLabOrders : IWarriorsLabOrders
    {
        private readonly WarriorsLabService.WarriorsLabServiceClient mClient;
        private readonly string mID;

        public ClientWarriorsLabOrders(WarriorsLabService.WarriorsLabServiceClient client, string id)
        {
            mClient = client;
            mID = id;
        }

        public Task CancelOrderAt(int index)
        {
            return mClient.CancelOredrAsync(new CancelQueuedRequest
            {
                ObjectID = new ID {Value = mID},
                Index = index
            }).ResponseAsync;
        }

        public Task QueueDamageUpgrade()
        {
            return mClient.QueueDamageUpgradeAsync(new QueueDamageUpgradeRequest
            {
                LabID = new ID {Value = mID},
            }).ResponseAsync;
        }

        public Task QueueArmourUpgrade()
        {
            return mClient.QueueArmourUpgradeAsync(new QueueArmourUpgradeRequest
            {
                LabID = new ID {Value = mID},
            }).ResponseAsync;
        }

        public Task QueueAttackRangeUpgrade()
        {
            return mClient.QueueAttackRangeUpgradeAsync(new QueueAttackRangeUpgradeRequest
            {
                LabID = new ID {Value = mID},
            }).ResponseAsync;
        }

        public Task CancelBuilding()
        {
            return mClient.CancelBuildingAsync(new CancelBuildingRequest()
            {
                BuildingID = new ID() { Value = mID }
            }).ResponseAsync;
        }
    }
    
    internal class ClientWarriorsLabInfo : IWarriorsLabInfo, IStateHolder<WarriorsLabState>
    {
        public WarriorsLabState State { get; private set; } = new WarriorsLabState();
        
        public Guid ID => Guid.Parse(State.Base.Base.Base.ID.Value);
        public Guid PlayerID => Guid.Parse(State.Base.Base.Base.PlayerID.Value);
        public Vector2 Position => State.Base.Base.Base.Position.ToUnity();
        public float RecivedDamage => State.Base.Base.Base.RecivedDamage;
        public float MaxHealth => State.Base.Base.Base.MaxHealth;
        public float ViewRadius => State.Base.Base.Base.ViewRadius;
        public Vector2 Size => State.Base.Base.Size.ToUnity();
        public float Progress => State.Base.Progress;
        public int Queued => State.Base.Queued;
        public int Armour => State.Base.Base.Base.Armour;
        public BuildingProgress BuildingProgress => State.Base.Base.Progress;
        
        public void ResetState()
        {
            State = new WarriorsLabState();
        }
    }
    
    class WarriorsLabCreationListener : CommonCreationStateListener<
        IWarriorsLabOrders,
        IWarriorsLabInfo,
        ClientWarriorsLabOrders,
        ClientWarriorsLabInfo,
        WarriorsLabState,
        WarriorsLabService.WarriorsLabServiceClient>
    {
        public WarriorsLabCreationListener(UnitySyncContext syncContext) 
            : base(syncContext)
        {
        }

        protected override AsyncServerStreamingCall<WarriorsLabState> GetCreationCall(WarriorsLabService.WarriorsLabServiceClient client, CancellationToken token)
        {
            return client.ListenCreation(new Empty(), cancellationToken: token);
        }

        protected override AsyncServerStreamingCall<WarriorsLabState> GetUpdatesCall(WarriorsLabService.WarriorsLabServiceClient client, ID id, CancellationToken token)
        {
            return client.ListenState(id, cancellationToken: token);
        }

        protected override WarriorsLabService.WarriorsLabServiceClient CreateClient(Channel channel)
        {
            return new WarriorsLabService.WarriorsLabServiceClient(channel);
        }

        protected override ClientWarriorsLabOrders CreateOrders(WarriorsLabService.WarriorsLabServiceClient client, Guid id)
        {
            return new ClientWarriorsLabOrders(client, id.ToString());
        }
    }
}