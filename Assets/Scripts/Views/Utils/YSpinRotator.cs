using UnityEngine;

namespace Assets.Views.Utils
{
    class YSpinRotator : MonoBehaviour
    {
        public GameObject RotationTarget;
        public float RotationSpeed;

        private void Update()
        {
            var euler = RotationTarget.transform.localEulerAngles;
            RotationTarget.transform.localEulerAngles = new Vector3(euler.x, euler.y + Time.deltaTime * RotationSpeed, euler.z);
        }
    }
}