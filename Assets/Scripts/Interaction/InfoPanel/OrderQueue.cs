using System.Linq;
using Assets.Core.GameObjects.Base;
using Assets.Views.Base;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Interaction.InfoPanel
{
    class OrderQueue : MonoBehaviour
    {
        public UserInterface Interface;
        
        public GameObject IconPrefab;
        public GameObject IconsRoot;

        public void Update()
        {
            var selected = Interface.FetchSelectedInfo<IQueueOrdersInfo>().FirstOrDefault();
            if (selected != null)
            {
                while (selected.Queued > IconsRoot.transform.childCount)
                {
                    var icon = Instantiate(IconPrefab);
                    
                    var iconButton = icon.GetComponent<Button>();
                    if (iconButton != null)
                        iconButton.onClick.AddListener(() => OnIconClick(icon));
                    
                    icon.transform.parent = IconsRoot.transform;
                }
                
                while (selected.Queued < IconsRoot.transform.childCount)
                {
                    var child = IconsRoot.transform.GetChild(0);
                    
                    var iconButton = child.GetComponent<Button>();
                    if (iconButton != null)
                        iconButton.onClick.RemoveAllListeners();
                    
                    child.transform.parent = null;
                    Destroy(child.gameObject);
                }
            }
        }

        private void OnIconClick(GameObject icon)
        {
            var indx = icon.transform.GetSiblingIndex();
            var selected = Interface.FetchSelectedOrders<IQueueOrdersOrders>().FirstOrDefault();
            if (selected != null)
                selected.CancelOrderAt(indx);
        }
    }
}