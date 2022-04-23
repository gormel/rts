using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Interaction
{
    class ChooseSelectedType : MonoBehaviour
    {
        public UserInterface Interface;

        public GameObject Single;
        public GameObject Multi;

        void Update()
        {
            if (Interface == null)
                return;

            if (Single == null)
                return;
            
            if (Multi == null)
                return;

            Single.SetActive(Interface.Selected.Count == 1);
            Multi.SetActive(Interface.Selected.Count > 1);
        }
    }
}
