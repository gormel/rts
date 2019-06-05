using UnityEngine;
using UnityEngine.UI;

namespace Assets.Interaction
{
    class IconNameHealthInterface : MonoBehaviour
    {
        public Image IconImage;
        public Text NameText;
        public Text HealthText;
        public RectTransform GreenHealthBar;
        public RectTransform RedHealthBar;

        public void SetIcon(Sprite icon)
        {
            IconImage.sprite = icon;
        }

        public void SetName(string name)
        {
            NameText.text = name;
        }

        public void SetHealth(float health, float maxHealth)
        {
            HealthText.text = $"{health:######}/{maxHealth:#####}";
            if (maxHealth == 0)
            {
                GreenHealthBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, RedHealthBar.rect.width);
            }
            else
            {
                var percent = health / maxHealth;
                GreenHealthBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, RedHealthBar.rect.width * percent);
            }
        }
    }
}