using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Interaction.InterfaceUtils;
using UnityEngine;

namespace Assets.Interaction.Chat
{
    class ChatViewInterface : MonoBehaviour
    {
        public Root Root;
        public float MessageLiveSeconds;
        public GameObject MessagePrefab;
        private int mLastMessageId;
        public StickerDatabase StickerDatabase;

        void Awake()
        {
            Root.ChatMessageRecived += RootOnChatMessageRecived;
        }

        private void RootOnChatMessageRecived(string nickname, int stickerID)
        {
            var messageInst = Instantiate(MessagePrefab);
            messageInst.transform.parent = transform;
            messageInst.GetComponent<RemoveAfterSeconds>().AliveSeconds = MessageLiveSeconds;
            var messageComponent = messageInst.GetComponent<ChatMessage>();
            messageComponent.Nickname = nickname;
            messageComponent.Sticker = StickerDatabase.GetSticker(stickerID);
        }
    }
}
