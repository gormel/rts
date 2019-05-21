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

        private Map mMap;

        public void LoadMap(Map map)
        {
            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
                throw new Exception("MeshFilter on map not found.");

            if (meshFilter.mesh == null)
                meshFilter.mesh = new Mesh();

            var mapVertices = new List<Vector3>();
            var mapUVs = new List<Vector2>();
            var i = 0;
            var mapIndices = new int[(map.Width - 1) * (map.Length - 1) * 4];
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Length; y++)
                {
                    mapVertices.Add(GameUtils.GetPosition(new Vector2(x, y), map));
                    mapUVs.Add(new Vector2(x, y) / 4);

                    if (y == map.Length - 1 || x == map.Width - 1)
                        continue;

                    mapIndices[i++] = x * map.Length + y;
                    mapIndices[i++] = x * map.Length + y + 1;
                    mapIndices[i++] = (x + 1) * map.Length + y + 1;
                    mapIndices[i++] = (x + 1) * map.Length + y;
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
                new Bounds(new Vector3((float) map.Width / 2, 0, (float) map.Length / 2), new Vector3(map.Width, 2, map.Length)),
                transform.position, 
                transform.rotation
                );

            NavMesh.AddNavMeshData(navMeshData);
            mMap = map;
        }

        public Vector3 GetWorldPosition(Vector2 flatPosition)
        {
            return GameUtils.GetPosition(flatPosition, mMap);
        }
    }
}
