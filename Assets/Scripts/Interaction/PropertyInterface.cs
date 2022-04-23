using UnityEngine;
using UnityEngine.UI;

namespace Assets.Interaction
{
    class PropertyInterface : MonoBehaviour
    {
        public Text NameText;
        public Text ValueText;

        public void SetName(string name)
        {
            NameText.text = name;
        }

        public void SetText(string text)
        {
            ValueText.text = text;
        }
    }
}