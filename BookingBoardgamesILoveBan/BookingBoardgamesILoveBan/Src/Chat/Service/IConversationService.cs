using System;
using System.Collections.Generic;
using BookingBoardgamesILoveBan.Src.Chat.DTO;
using BookingBoardgamesILoveBan.Src.Chat.Model;
using BookingBoardgamesILoveBan.Src.Model;

namespace BookingBoardgamesILoveBan.Src.Chat.Service
{
    public interface IConversationService
    {
        void OnMessageReceived(Message message);
        void OnMessageUpdateReceived(Message message);
        void OnConversationReceived(Conversation conversation);
        void OnReadReceiptReceived(ReadReceipt readReceipt);
        List<ConversationDataTransferObject> FetchConversations();
        string GetOtherUserNameByConversationDTO(ConversationDataTransferObject conversation);
        void UpdateMessage(MessageDataTransferObject message);
        void SendMessage(MessageDataTransferObject message);

        event Action<MessageDataTransferObject, string> ActionMessageProcessed;
        event Action<ConversationDataTransferObject, string> ActionConversationProcessed;
        event Action<ReadReceiptDataTransferObject> ActionReadReceiptProcessed;
        event Action<MessageDataTransferObject, string> ActionMessageUpdateProcessed;
    }
}