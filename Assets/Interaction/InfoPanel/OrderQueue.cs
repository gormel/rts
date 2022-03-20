using System.Linq;
using Assets.Core.GameObjects.Base;
using Assets.Views.Base;
using UnityEngine;

namespace Assets.Interaction.InfoPanel
{
    class OrderQueue : MonoBehaviour
    {
        public UserInterface Interface;
        
        public GameObject IconPrefab;
        public GameObject IconsRoot;

        public void Update()
        {
            var selected = Interface.FetchSelectedInfo<IFactoryBuildingInfo>().FirstOrDefault();
            if (selected != null)
            {
                while (selected.Queued > IconsRoot.transform.childCount)
                {
                    var icon = Instantiate(IconPrefab);
                    icon.transform.parent = IconsRoot.transform;
                }
                
                while (selected.Queued < IconsRoot.transform.childCount)
                {
                    var child = IconsRoot.transform.GetChild(0);
                    child.transform.parent = null;
                    Destroy(child.gameObject);
                }
            }
        }
    }
}