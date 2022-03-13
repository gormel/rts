using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Views.Utils
{
    class ExternalUpdater : MonoBehaviour
    {
        private readonly Dictionary<Guid, Action> mRegistredUpdates = new Dictionary<Guid, Action>();
        
        public void Register(Guid id, Action action)
        {
            mRegistredUpdates[id] = action;
        }

        public void Free(Guid id)
        {
            mRegistredUpdates.Remove(id);
        }

        private void Update()
        {
            foreach (var registredUpdatesValue in mRegistredUpdates.Values)
            {
                registredUpdatesValue();
            }
        }
    }
}