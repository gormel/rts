using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Interaction.InterfaceUtils
{
    class RemoveAfterSeconds : MonoBehaviour
    {
        public float AliveSeconds;

        void Start()
        {
            StartCoroutine(StartCountdown());
        }

        IEnumerator StartCountdown()
        {
            yield return new WaitForSeconds(AliveSeconds);
            Destroy(gameObject);
        }
    }
}
