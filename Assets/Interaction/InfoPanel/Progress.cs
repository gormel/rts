using UnityEngine;

namespace Assets.Interaction.InfoPanel
{
    class Progress : MonoBehaviour
    {
        public RectTransform ProgressBar;
        public float ProgressValue;
        
        public void Update()
        {
            var barParent = ProgressBar.transform.parent.GetComponent<RectTransform>();
            if (barParent != null)
            {
                var parentWidth = barParent.rect.width;
                ProgressBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentWidth * ProgressValue);
            }
        }
    }
}