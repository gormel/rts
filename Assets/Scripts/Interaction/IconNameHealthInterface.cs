using Assets.Views.Base;
using UnityEngine;
using UnityEngine.InputSystem;
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
        public RectTransform ProgressBar;
        
        public UserInterface Interface { get; set; }
        public SelectableView Owner { get; set; }
        
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

        public void SetProgress(float progress)
        {
            var progressParent = ProgressBar.parent as RectTransform;
            if (progressParent == null)
                return;

            if (progress >= 1)
                progress = 0;
            
            ProgressBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, progressParent.rect.width * progress);
        }

        public void OnClick()
        {
            if (Interface == null)
                return;

            if (Keyboard.current.shiftKey.isPressed)
            {
                Interface.SelectionManager.RemoveSelection(new[] { Owner });
            }
            else
            {
                Interface.SelectionManager.Select(new[] { Owner });
            }
        }
    }
}