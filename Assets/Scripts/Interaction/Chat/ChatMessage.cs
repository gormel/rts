using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Interaction.Chat
{
    class ChatMessage : MonoBehaviour
    {
        public string Nickname;
        public Sprite Sticker;

        public TextMeshProUGUI NicknameTarget;
        public Image StickerTarget;

        void Update()
        {
            if (NicknameTarget != null)
                NicknameTarget.text = Nickname;

            if (StickerTarget != null)
                StickerTarget.sprite = Sticker;
        }
    }
}
