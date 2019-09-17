using System;
using System.Security.Cryptography;
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
        public bool IsArrived { get; private set; } = true;
        public event Action Arrived;
        public Vector2 CurrentPosition => GameUtils.GetFlatPosition(transform.localPosition);
        public Vector2 CurrentDirection => GameUtils.GetFlatPosition(transform.localRotation * Vector3.forward);

        private NavMeshAgent mNavMeshAgent;
        private float mLastDistance;

        public LineRenderer TargetLine;
        public GameObject WaypointPrefab;

        private GameObject mWaypointInst;

        public override Rect FlatBounds => new Rect(CurrentPosition, Vector2.zero);

        protected virtual void Start()
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
            else
            {
                mNavMeshAgent.Warp(transform.position);
                mWaypointInst = Instantiate(WaypointPrefab);
                mWaypointInst.SetActive(false);
            }
        }

        protected virtual void Update()
        {
            if (!IsClient)
            {
                mNavMeshAgent.speed = Info.Speed;
            }
            else
            {
                //maybe lerp
                transform.localPosition = Map.GetWorldPosition(Info.Position);
                transform.localEulerAngles = new Vector3(0, Mathf.Rad2Deg * Mathf.Atan2(Info.Direction.x, Info.Direction.y), 0);
            }
            
            TargetLine.gameObject.SetActive(IsSelected);
            TargetLine.SetPosition(0, transform.position);
            TargetLine.SetPosition(1, Map.GetWorldPosition(Info.Destignation));

            UpdateProperties();
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (IsClient)
                return;

            if (IsArrived)
                return;

            var pf = other.gameObject.GetComponentInChildren<IPathFinder>();
            if (pf != null && pf.IsArrived && other.gameObject.activeSelf)
            {
                mNavMeshAgent.ResetPath();
                Arrived?.Invoke();
                IsArrived = true;
                mWaypointInst.SetActive(false);
            }
        }

        protected virtual void LateUpdate()
        {
            if (!IsClient && mNavMeshAgent.velocity.sqrMagnitude > 0.01)
                transform.rotation = Quaternion.LookRotation(mNavMeshAgent.velocity.normalized);
        }

        public Task SetTarget(Vector2 position, IMapData mapData)
        {
            return SyncContext.Execute(() =>
            {
                IsArrived = false;
                var target = GameUtils.GetPosition(position, mapData);
                mWaypointInst.transform.position = target;
                mWaypointInst.SetActive(true);
                mNavMeshAgent.SetDestination(target);
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