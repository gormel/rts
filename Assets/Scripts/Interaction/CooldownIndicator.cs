using UnityEngine;
using UnityEngine.UI;

namespace Assets.Interaction
{
    class CooldownIndicator : MonoBehaviour
    {
        public Image IndicatorImage;

        public void SetProgress(float progress)
        {
            IndicatorImage.fillAmount = progress;
        }
    }
}