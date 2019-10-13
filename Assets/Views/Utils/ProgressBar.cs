using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Views.Utils
{
    class ProgressBar : MonoBehaviour
    {
        public float Progress { get; set; }
        private MeshRenderer mRenderer;

        void Start()
        {
            mRenderer = GetComponent<MeshRenderer>();
        }

        void Update()
        {
            var mat = mRenderer.material;
            mat.SetFloat("_Value", Progress);
        }
    }
}
