using System;
using UnityEngine;

namespace Assets.Views.Utils
{
    public class TileStretched : MonoBehaviour
    {
        public GameObject TilePrefab;

        public GameObject BeginPosition;
        public GameObject EndPosition;
        public GameObject TileContainer;

        public float TileSize;

        public void Regenerate()
        {
            while (TileContainer.transform.childCount > 0)
            {
                var child = TileContainer.transform.GetChild(0);
                child.SetParent(null);
                Destroy(child.gameObject);
            }

            var a = BeginPosition.transform.position;
            var b = EndPosition.transform.position;
            var dist = Vector3.Distance(a, b);

            var count = Mathf.Floor(dist / TileSize);
            for (int i = 0; i < count; i++)
            {
                GenerateAt((i + 0.5f) / dist);
            }

            var reminder = dist / TileSize - count;
            if (reminder > 0.1f)
            {
                var tile = GenerateAt((count + reminder / 2) / dist);
                tile.transform.localScale = new Vector3(reminder / TileSize, 1, 1);
            }
        }

        private GameObject GenerateAt(float percent)
        {
            var a = BeginPosition.transform.position;
            var b = EndPosition.transform.position;
            var pos = Vector3.Lerp(a, b, percent);
            var tile = Instantiate(TilePrefab, TileContainer.transform, true);
            tile.transform.position = pos;
            tile.transform.rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.up, b - a));
            return tile;
        }
    }
}