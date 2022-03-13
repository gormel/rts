using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Views.Utils
{
    class RandomYRotator : MonoBehaviour
    {
        public GameObject RotationTarget;
        private void Start()
        {
            RotationTarget.transform.localEulerAngles = new Vector3(0, Random.Range(0, 360), 0);
        }
    }
}
