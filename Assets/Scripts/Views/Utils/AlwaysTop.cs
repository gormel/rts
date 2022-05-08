using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Views.Utils
{
    class AlwaysTop : MonoBehaviour
    {
        void Update()
        {
            transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.up);
        }
    }
}
