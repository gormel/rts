﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using Assets.Utils;
using Grpc.Core;
using UnityEngine;

namespace Assets.Networking.ClientListeners
{
    internal class ClientBuildersLabOrders : IBuildersLabOrders
    {
        private readonly BuildersLabService.BuildersLabServiceClient mClient;
        private readonly string mID;

        public ClientBuildersLabOrders(BuildersLabService.BuildersLabServiceClient client, string id)
        {
            mClient = client;
            mID = id;
        }
        
        public Task QueueAttackUpgrade()
        {
            return mClient.QueueAttackUpgradeAsync(new QueueAttackUpgradeRequest
            {
                BuildingID = new ID { Value = mID }
            }).ResponseAsync;
        }

        public Task QueueDefenceUpgrade()
        {
            return mClient.QueueDefenceUpgradeAsync(new QueueDefenceUpgradeRequest
            {
                BuildingID = new ID { Value = mID }
            }).ResponseAsync;
        }

        public Task CancelResearch()
        {
            return mClient.CancelUpgradeAsync(new CancelUpgradeRequest
            {
                LabID = new ID { Value = mID }
            }).ResponseAsync;
        }
    }
    
    internal class ClientBuildersLabInfo : IBuildersLabInfo, IStateHolder<BuildersLabState>
    {
        public Guid ID => Guid.Parse(State.Base.Base.Base.ID.Value);
        public Guid PlayerID => Guid.Parse(State.Base.Base.Base.PlayerID.Value);
        public Vector2 Position => State.Base.Base.Base.Position.ToUnity();
        public float RecivedDamage => State.Base.Base.Base.RecivedDamage;
        public float MaxHealth => State.Base.Base.Base.MaxHealth;
        public float ViewRadius => State.Base.Base.Base.ViewRadius;
        public Vector2 Size => State.Base.Base.Size.ToUnity();
        public float Progress => State.Base.Progress;
        public int Queued => State.Base.Queued;
        public int AttackUpgradeCost => State.AttackUpgradeCost;
        public int DefenceUpgradeCost => State.DefenceUpgradeCost;
        public BuildersLabState State { get; private set; } = new BuildersLabState();
        
        public void ResetState()
        {
            State = new BuildersLabState();
        }
    }
    
    class BuildersLabCreationListener : CommonCreationStateListener<
        IBuildersLabOrders,
        IBuildersLabInfo,
        ClientBuildersLabOrders,
        ClientBuildersLabInfo,
        BuildersLabState,
        BuildersLabService.BuildersLabServiceClient>
    {
        public BuildersLabCreationListener(UnitySyncContext syncContext) 
            : base(syncContext)
        {
        }

        protected override AsyncServerStreamingCall<BuildersLabState> GetCreationCall(BuildersLabService.BuildersLabServiceClient client, CancellationToken token)
        {
            return client.ListenCreation(new Empty(), cancellationToken: token);
        }

        protected override AsyncServerStreamingCall<BuildersLabState> GetUpdatesCall(BuildersLabService.BuildersLabServiceClient client, ID id, CancellationToken token)
        {
            return client.ListenState(id, cancellationToken: token);
        }

        protected override BuildersLabService.BuildersLabServiceClient CreateClient(Channel channel)
        {
            return new BuildersLabService.BuildersLabServiceClient(channel);
        }

        protected override ClientBuildersLabOrders CreateOrders(BuildersLabService.BuildersLabServiceClient client, Guid id)
        {
            return new ClientBuildersLabOrders(client, id.ToString());
        }
    }
}