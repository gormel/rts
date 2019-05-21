using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Interaction
{
    class SingleSelectedUnitInterface : MonoBehaviour
    {
        public UserInterface Interface;
        public IconNameHealthInterface IconNameHealth;

        void Update()
        {
            if (Interface.Selected.Count < 1)
                return;

            var selected = Interface.Selected[0];
            IconNameHealth.SetIcon(selected.Icon);
            IconNameHealth.SetName(selected.Name);
            IconNameHealth.SetHealth(selected.Health, selected.MaxHealth);
        }
    }
}
