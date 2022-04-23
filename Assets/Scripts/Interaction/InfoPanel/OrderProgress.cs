using System.Linq;
using Assets.Core.GameObjects.Base;
using UnityEngine;

namespace Assets.Interaction.InfoPanel
{
    class OrderProgress : MonoBehaviour
    {
        public UserInterface Interface;
        public Progress ProgressBar;

        public void Update()
        {
            var selected = Interface.FetchSelectedInfo<IQueueOrdersInfo>().FirstOrDefault();
            if (selected != null)
            {
                ProgressBar.ProgressValue = selected.Queued > 0 ? selected.Progress : 0;
            }
        }
    }
}