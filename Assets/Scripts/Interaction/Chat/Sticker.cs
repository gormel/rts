using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Interaction.Chat
{
    class Sticker : MonoBehaviour
    {
        public event Action<int> Send;
        public Image StickerTarget;
        public int StickerID;

        public void OnClick()
        {
            Send?.Invoke(StickerID);
        }
    }
}
