using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Views.Utils
{
    class Shooter : MonoBehaviour
    {
        public float Percent = 0.2f;
        public float FireRate;
        public GameObject Target;

        private Coroutine mFiering;

        void OnEnable()
        {
            mFiering = StartCoroutine(Fire());
        }

        void OnDisable()
        {
            StopCoroutine(mFiering);
        }

        private IEnumerator Fire()
        {
            while (true)
            {
                if (Target == null)
                {
                    yield return null;
                    continue;
                }

                var t = 1 / FireRate;
                Target.SetActive(true);
                yield return new WaitForSeconds(t * Percent);
                Target.SetActive(false);
                yield return new WaitForSeconds(t * (1 - Percent));
            }
        }
    }
}
