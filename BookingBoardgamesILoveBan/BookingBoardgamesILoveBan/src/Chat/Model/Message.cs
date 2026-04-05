using BookingBoardgamesILoveBan.src.Enum;
using BookingBoardgamesILoveBan.src.Chat.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;

namespace BookingBoardgamesILoveBan.src.Model
{
    public abstract class Message
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public DateTime SentAt { get; set; }
        public string ContentAsString { get; set; }
        public MessageType Type { get; set; }


        public Message(int id, int conversationId, int senderId, int receiverId, DateTime sentAt, string contentAsString, MessageType type)
        { 
            Id = id;
            ConversationId = conversationId;
            SenderId = senderId;
            ReceiverId = receiverId;
            SentAt = sentAt;
            ContentAsString = contentAsString; 
            Type = type;
        }
    }
}
