using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Utils;
using UnityEngine;

namespace Assets.Interaction.Chat
{
    class Stickers : MonoBehaviour
    {
        public StickerDatabase Database;
        public GameObject StickerPrefab;
        public Root Root;

        void Start()
        {
            for (int i = 0; i < Database.Stickers.Length; i++)
            {
                var stickerInst = Instantiate(StickerPrefab);
                var sticker = stickerInst.GetComponent<Sticker>();
                sticker.StickerID = i;
                sticker.StickerTarget.sprite = Database.GetSticker(i);
                sticker.Send += StickerOnSend;
                stickerInst.transform.parent = transform;
            }
        }

        private void StickerOnSend(int stickerID)
        {
            Root.SendChatMessage(GameUtils.Nickname, stickerID);
            gameObject.SetActive(false);
        }
    }
}
