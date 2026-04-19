using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.Chat.DTO;
using BookingBoardgamesILoveBan.Src.Chat.Model;
using BookingBoardgamesILoveBan.Src.Model;

namespace BookingBoardgamesILoveBan.Src.Chat.Service
{
    public interface IConversationService
    {
        public void OnMessageReceived(Message message);
        public void OnMessageUpdateReceived(Message message);
        public void OnConversationReceived(Conversation conversation);
        public void OnReadReceiptReceived(ReadReceipt readReceipt);
        public List<ConversationDTO> FetchConversations();
        public string GetOtherUserNameByConversationDTO(ConversationDTO conversation);
        public void UpdateMessage(MessageDTO message);
        public void SendMessage(MessageDTO message);

        event Action<MessageDTO, string> ActionMessageProcessed;
        event Action<ConversationDTO, string> ActionConversationProcessed;
        event Action<ReadReceiptDTO> ActionReadReceiptProcessed;
        event Action<MessageDTO, string> ActionMessageUpdateProcessed;
    }
}
