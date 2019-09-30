using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Map;
using Assets.Utils;
using Assets.Views.Base;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Assets.Views
{
    class MapView : MonoBehaviour
    {
        public const int AgentTypeID = 0;

        public GameObject ChildContainer;
        public GameObject ObjectsContainer;

        public GameObject FogOfWar;
        public Camera FogOfWarCamera;
        public RenderTexture FogOfWarTexture;

        public GameObject TreePrefab;
        public GameObject CrystalPrefab;

        private IMapData mMapData;

        private static void ApplyMesh(MeshFilter filter, List<Vector3> verts, int[] indices, List<Vector2> uv)
        {
            filter.mesh.SetVertices(verts);
            filter.mesh.SetIndices(indices, MeshTopology.Quads, 0);
            filter.mesh.SetUVs(0, uv);
            filter.mesh.RecalculateNormals();
            filter.mesh.RecalculateTangents();
            filter.mesh.RecalculateBounds();
        }

        public void LoadMap(IMapData mapData, bool generateNavMesh)
        {
            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
                throw new Exception("MeshFilter on map not found.");

            if (meshFilter.mesh == null)
                meshFilter.mesh = new Mesh();

            var fowMeshFilter = FogOfWar.GetComponent<MeshFilter>();
            if (fowMeshFilter.mesh == null)
                fowMeshFilter.mesh = new Mesh();

            var fowVertices = new List<Vector3>();
            var mapVertices = new List<Vector3>();
            var mapUVs = new List<Vector2>();
            var fowUVs = new List<Vector2>();
            var i = 0;
            var mapIndices = new int[(mapData.Width - 1) * (mapData.Length - 1) * 4];
            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Length; y++)
                {
                    fowVertices.Add(GameUtils.GetPosition(new Vector2(x, y), mapData));
                    mapVertices.Add(GameUtils.GetPosition(new Vector2(x, y), mapData));
                    mapUVs.Add(new Vector2(x, y) / 4);
                    fowUVs.Add(new Vector2((float)x / mapData.Width, (float)y / mapData.Length));

                    if (y == mapData.Length - 1 || x == mapData.Width - 1)
                        continue;

                    mapIndices[i++] = x * mapData.Length + y;
                    mapIndices[i++] = x * mapData.Length + y + 1;
                    mapIndices[i++] = (x + 1) * mapData.Length + y + 1;
                    mapIndices[i++] = (x + 1) * mapData.Length + y;
                }
            }

            ApplyMesh(meshFilter, mapVertices, mapIndices, mapUVs);
            ApplyMesh(fowMeshFilter, fowVertices, mapIndices, fowUVs);

            var mapCollider = GetComponent<MeshCollider>();
            if (mapCollider != null)
                mapCollider.sharedMesh = meshFilter.mesh;

            var fowCollider = FogOfWar.GetComponent<MeshCollider>();
            if (fowCollider != null)
                fowCollider.sharedMesh = fowMeshFilter.mesh;

            gameObject.isStatic = true;

            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Length; y++)
                {
                    var pos = GameUtils.GetPosition(new Vector2(x + 0.5f, y + 0.5f), mapData);
                    GameObject inst = null;
                    switch (mapData.GetMapObjectAt(x, y))
                    {
                        case MapObject.Tree:
                            inst = Instantiate(TreePrefab);
                            break;
                        case MapObject.Crystal:
                            inst = Instantiate(CrystalPrefab);
                            break;
                    }

                    if (inst == null)
                        continue;

                    inst.transform.parent = ObjectsContainer.transform;
                    inst.transform.localPosition = pos;
                }
            }

            if (generateNavMesh)
            {
                var settings = NavMesh.CreateSettings();
                settings.agentTypeID = AgentTypeID;
                settings.agentRadius = 0;
                settings.agentSlope = 60;

                var source = new NavMeshBuildSource();
                source.shape = NavMeshBuildSourceShape.Mesh;
                source.sourceObject = meshFilter.mesh;
                source.transform = transform.localToWorldMatrix;

                var navMeshData = NavMeshBuilder.BuildNavMeshData(settings, new List<NavMeshBuildSource> { source }, new Bounds(new Vector3((float)mapData.Width / 2, 0, (float)mapData.Length / 2), new Vector3(mapData.Width, 2, mapData.Length)), transform.position, transform.rotation);

                NavMesh.RemoveAllNavMeshData();
                NavMesh.AddNavMeshData(navMeshData);
            }
            mMapData = mapData;
            FogOfWarTexture.width = mapData.Width;
            FogOfWarTexture.height = mapData.Length;
            FogOfWarCamera.orthographicSize = Mathf.Max(mapData.Width - 1, mapData.Length - 1) / 2f;
            FogOfWarCamera.transform.localPosition = new Vector3((mapData.Width - 1) / 2f, 10, (mapData.Length - 1) / 2f);
        }

        public bool IsAreaFree(Vector2 position, Vector2 size)
        {
            if (!mMapData.GetIsAreaFree(position, size))
                return false;

            var bounds = new Rect(position, size);

            for (int i = 0; i < ChildContainer.transform.childCount; i++)
            {
                var child = ChildContainer.transform.GetChild(i).gameObject;
                var boundsOwner = child.GetComponent<IFlatBoundsOwner>();

                if (bounds.Overlaps(boundsOwner.FlatBounds))
                    return false;
            }
            //foreach child in childcontainer, check box collisions
            return true;
        }

        public Vector3 GetWorldPosition(Vector2 flatPosition)
        {
            return GameUtils.GetPosition(flatPosition, mMapData);
        }
    }
}
