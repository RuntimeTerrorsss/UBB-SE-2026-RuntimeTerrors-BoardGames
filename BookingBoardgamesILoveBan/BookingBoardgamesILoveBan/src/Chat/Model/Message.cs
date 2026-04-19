using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.Chat.DTO;
using BookingBoardgamesILoveBan.Src.Enum;
using Windows.ApplicationModel.Activation;

namespace BookingBoardgamesILoveBan.Src.Model
{
    public abstract class Message
    {
        public int MessageId { get; set; }
        public int ConversationId { get; set; }
        public int MessageSenderId { get; set; }
        public int MessageReceiverId { get; set; }
        public DateTime MessageSentTime { get; set; }
        public string MessageContentAsString { get; set; }
        public MessageType TypeOfMessage { get; set; }
        public Message(int id, int conversationId, int senderId, int receiverId, DateTime sentAt, string contentAsString, MessageType type)
        {
            MessageId = id;
            ConversationId = conversationId;
            MessageSenderId = senderId;
            MessageReceiverId = receiverId;
            MessageSentTime = sentAt;
            MessageContentAsString = contentAsString;
            TypeOfMessage = type;
        }
    }
}
