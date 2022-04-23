using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Views.Utils
{
    class FaceToCamera : MonoBehaviour
    {
        void Update()
        {
            var camForward = Camera.main.transform.forward;
            var camUp = Camera.main.transform.up;

            transform.rotation = Quaternion.LookRotation(camForward, camUp);
        }
    }
}
