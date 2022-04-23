using System;
using System.Linq;
using Assets.Core.GameObjects.Final;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Interaction.InfoPanel
{
    class MiningCampInfoPanel : MonoBehaviour
    {
        public UserInterface Interface;
        public GameObject IconsRoot;
        public GameObject IconPrefab;
        public Text MiningSpeedText;

        public void Update()
        {
            var selected = Interface.FetchSelectedInfo<IMinigCampInfo>().FirstOrDefault();
            if (selected != null)
            {
                while (selected.WorkerCount > IconsRoot.transform.childCount)
                {
                    var icon = Instantiate(IconPrefab);
                    icon.transform.parent = IconsRoot.transform;
                }
                
                while (selected.WorkerCount < IconsRoot.transform.childCount)
                {
                    var child = IconsRoot.transform.GetChild(0);
                    child.transform.parent = null;
                    Destroy(child.gameObject);
                }

                MiningSpeedText.text = selected.MiningSpeed.ToString();
            }
        }
    }
}