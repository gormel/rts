using System;
using Assets.Core.GameObjects.Base;
using Assets.Core.Map;
using Assets.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Views.Base
{
    abstract class UnitView<TOrders, TInfo> : ModelSelectableView<TOrders, TInfo>, IPathFinder 
        where TOrders : IUnitOrders
        where TInfo : IUnitInfo
    {
        public bool Active { get; private set; }
        
        public Vector2 CurrentPosition => GameUtils.GetFlatPosition(transform.localPosition);
        public Vector2 CurrentDirection => GameUtils.GetFlatPosition(transform.localRotation * Vector3.forward);

        private NavMeshAgent mNavMeshAgent;
        private Vector3 mTarget;

        public LineRenderer TargetLine;

        protected virtual void OnStart()
        {
            mNavMeshAgent = GetComponentInChildren<NavMeshAgent>();
            if (mNavMeshAgent == null)
                throw new Exception("NavMeshAgent is missing on unit.");

            mNavMeshAgent.updateRotation = false;
        }

        protected virtual void OnUpdate()
        {
            UpdateExecutions();

            mNavMeshAgent.speed = Info.Speed;

            if (!mNavMeshAgent.pathPending)
            {
                Active = Vector3.Distance(mTarget, transform.position) > 0.05;
            }

            TargetLine.gameObject.SetActive(IsSelected);
            TargetLine.SetPosition(0, transform.position);
            TargetLine.SetPosition(1, mNavMeshAgent.destination);

            UpdateProperties();
        }

        protected virtual void OnLateUpdate()
        {
            if (mNavMeshAgent.velocity.sqrMagnitude > 0.01)
                transform.rotation = Quaternion.LookRotation(mNavMeshAgent.velocity.normalized);
        }

        public void SetTarget(Vector2 position, Map map)
        {
            Active = true;
            Execute(() =>
            {
                mNavMeshAgent.SetDestination(mTarget = GameUtils.GetPosition(position, map));
            });
        }

        public void Stop()
        {
            Active = false;
            Execute(() =>
            {
                mNavMeshAgent.ResetPath();
            });
        }

        public void GoTo(Vector2 position)
        {
            Orders.GoTo(position);
        }

        public override void OnRightClick(Vector2 position)
        {
            GoTo(position);
        }
    }
}