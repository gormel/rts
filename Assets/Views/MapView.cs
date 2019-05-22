using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Map;
using Assets.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Views
{
    class MapView : MonoBehaviour
    {
        public const int AgentTypeID = 0;
        public GameObject ChildContainer;
        private IMapData mMapData;

        public void LoadMap(IMapData mapData)
        {
            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
                throw new Exception("MeshFilter on map not found.");

            if (meshFilter.mesh == null)
                meshFilter.mesh = new Mesh();

            var mapVertices = new List<Vector3>();
            var mapUVs = new List<Vector2>();
            var i = 0;
            var mapIndices = new int[(mapData.Width - 1) * (mapData.Length - 1) * 4];
            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Length; y++)
                {
                    mapVertices.Add(GameUtils.GetPosition(new Vector2(x, y), mapData));
                    mapUVs.Add(new Vector2(x, y) / 4);

                    if (y == mapData.Length - 1 || x == mapData.Width - 1)
                        continue;

                    mapIndices[i++] = x * mapData.Length + y;
                    mapIndices[i++] = x * mapData.Length + y + 1;
                    mapIndices[i++] = (x + 1) * mapData.Length + y + 1;
                    mapIndices[i++] = (x + 1) * mapData.Length + y;
                }
            }

            meshFilter.mesh.SetVertices(mapVertices);
            meshFilter.mesh.SetIndices(mapIndices, MeshTopology.Quads, 0);
            meshFilter.mesh.SetUVs(0, mapUVs);
            meshFilter.mesh.RecalculateNormals();
            meshFilter.mesh.RecalculateTangents();
            meshFilter.mesh.RecalculateBounds();

            var collider = GetComponent<MeshCollider>();
            if (collider != null)
            {
                collider.sharedMesh = meshFilter.mesh;
            }

            gameObject.isStatic = true;

            var settings = NavMesh.CreateSettings();
            settings.agentTypeID = AgentTypeID;
            settings.agentRadius = 0;

            var source = new NavMeshBuildSource();
            source.shape = NavMeshBuildSourceShape.Mesh;
            source.sourceObject = meshFilter.mesh;
            source.transform = transform.localToWorldMatrix;

            var navMeshData = NavMeshBuilder.BuildNavMeshData(
                settings,
                new List<NavMeshBuildSource> {source},
                new Bounds(new Vector3((float) mapData.Width / 2, 0, (float) mapData.Length / 2), new Vector3(mapData.Width, 2, mapData.Length)),
                transform.position, 
                transform.rotation
                );

            NavMesh.AddNavMeshData(navMeshData);
            mMapData = mapData;
        }

        public bool IsAreaFree(Vector2 position, Vector2 size)
        {
            return true;
        }

        public Vector3 GetWorldPosition(Vector2 flatPosition)
        {
            return GameUtils.GetPosition(flatPosition, mMapData);
        }
    }
}
