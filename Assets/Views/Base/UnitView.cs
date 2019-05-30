using System;
using System.Threading.Tasks;
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
        public event Action Arrived;
        public Vector2 CurrentPosition => GameUtils.GetFlatPosition(transform.localPosition);
        public Vector2 CurrentDirection => GameUtils.GetFlatPosition(transform.localRotation * Vector3.forward);

        public bool IsClient { get; set; }

        private NavMeshAgent mNavMeshAgent;
        private Vector3 mTarget;
        private float mLastDistance;

        public LineRenderer TargetLine;
        public UnitySyncContext SyncContext;

        protected virtual void OnStart()
        {
            mNavMeshAgent = GetComponentInChildren<NavMeshAgent>();
            if (mNavMeshAgent == null)
                throw new Exception("NavMeshAgent is missing on unit.");

            mNavMeshAgent.updateRotation = false;

            if (IsClient)
            {
                Destroy(mNavMeshAgent);
                mNavMeshAgent = null;
            }
        }

        protected virtual void OnUpdate()
        {
            if (!IsClient)
            {
                mNavMeshAgent.speed = Info.Speed;

                if (!mNavMeshAgent.pathPending)
                {
                    var distance = Vector3.Distance(mTarget, transform.position);

                    if (mLastDistance > 0.05 && distance <= 0.05)
                        Arrived?.Invoke();

                    mLastDistance = distance;
                }
            }


            TargetLine.gameObject.SetActive(IsSelected);
            TargetLine.SetPosition(0, transform.position);
            TargetLine.SetPosition(1, Map.GetWorldPosition(Info.Destignation));

            UpdateProperties();
        }

        protected virtual void OnLateUpdate()
        {
            if (mNavMeshAgent.velocity.sqrMagnitude > 0.01)
                transform.rotation = Quaternion.LookRotation(mNavMeshAgent.velocity.normalized);
        }

        public Task SetTarget(Vector2 position, IMapData mapData)
        {
            return SyncContext.Execute(() =>
            {
                mNavMeshAgent.SetDestination(mTarget = GameUtils.GetPosition(position, mapData));
            });
        }

        public Task Stop()
        {
            return SyncContext.Execute(() =>
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