using UnityEngine;

namespace Assets.Interaction
{
    sealed class BuildCursorColorer : MonoBehaviour
    {
        public GameObject Cursor;
        private MeshRenderer mRenderer;

        public bool Valid { get; set; }

        public Color ValidColor;
        public Color InvalidColor;

        void Start()
        {
            mRenderer = Cursor.GetComponent<MeshRenderer>();
        }

        void Update()
        {
            mRenderer.material.color = Valid ? ValidColor : InvalidColor;
        }
    }
}