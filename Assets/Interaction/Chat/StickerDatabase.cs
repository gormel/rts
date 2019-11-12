using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Interaction.Chat
{
    class StickerDatabase : MonoBehaviour
    {
        public Sprite[] Stickers;

        public Sprite GetSticker(int id)
        {
            if (id < 0 || id >= Stickers.Length)
                return null;

            return Stickers[id];
        }
    }
}
