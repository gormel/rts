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
        public Vector2 CurrentPosition => Initialized ? GameUtils.GetFlatPosition(transform.localPosition) : throw new Exception("Unitialized.");

        public bool Initialized { get; private set; } = false;
        public Vector2 CurrentDirection => Initialized ? GameUtils.GetFlatPosition(transform.localRotation * Vector3.forward) : throw new Exception("Unitialized.");
        
        public bool InProgress { get; private set; }
        public Vector2 Target { get; private set; }

        private NavMeshAgent mNavMeshAgent;
        private float mLastDistance;

        public Color TargetLineMovementColor;
        public LineRenderer TargetLine;
        public GameObject WaypointPrefab;

        private GameObject mWaypointInst;
        private Vector3? mLookTarget;

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

            TargetLine.endColor = TargetLine.startColor = TargetLineMovementColor;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Destroy(mWaypointInst);
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
            
            base.Update();
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (IsClient)
                return;

            if (IsArrived)
                return;

            var pf = other.gameObject.GetComponentInChildren<IPathFinderBase>();
            if (pf != null && pf.IsArrived && other.gameObject.activeSelf && pf.Target == Target)
            {
                mNavMeshAgent.ResetPath();
                IsArrived = true;
                mWaypointInst.SetActive(false);
                InProgress = false;
            }
        }

        protected virtual void LateUpdate()
        {
            if (!IsClient && !IsArrived && mNavMeshAgent.velocity.sqrMagnitude > 0)
                transform.rotation = Quaternion.LookRotation(mNavMeshAgent.velocity.normalized);
            else if (mLookTarget.HasValue && Vector3.Distance(mLookTarget.Value, transform.localPosition) > 0.01)
                transform.rotation = Quaternion.LookRotation(mLookTarget.Value - transform.localPosition);
        }

        public async Task Initialize(Vector2 position, Vector2 destignation, IMapData mapData)
        {
            await Teleport(position, mapData);
            await SetTarget(destignation, mapData);
            Initialized = true;
        }

        public Task SetLookAt(Vector2 position, IMapData mapData)
        {
            return SyncContext.Execute(() =>
            {
                mLookTarget = GameUtils.GetPosition(position, mapData);
                if (Vector3.Distance(mLookTarget.Value, transform.localPosition) > 0.01)
                    transform.rotation = Quaternion.LookRotation(mLookTarget.Value - transform.localPosition);
            });
        }

        public Task SetTarget(Vector2 position, IMapData mapData)
        {
            return SyncContext.Execute(() =>
            {
                if (!mNavMeshAgent.isOnNavMesh)
                    return;
                
                InProgress = true;
                mLookTarget = null;
                Target = position;
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
                InProgress = false;
                Target = Info.Position;
                mNavMeshAgent.ResetPath();
            });
        }

        public Task Teleport(Vector2 position, IMapData mapData)
        {
            return SyncContext.Execute(() =>
            {
                mNavMeshAgent.Warp(GameUtils.GetPosition(position, mapData));
            });
        }

        public override void OnRightClick(Vector2 position)
        {
            Orders.GoTo(position);
        }
    }
}