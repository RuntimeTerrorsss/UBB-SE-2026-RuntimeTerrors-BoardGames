using System;
using System.Collections.Generic;
using BookingBoardgamesILoveBan.Src.Chat.Model;
using BookingBoardgamesILoveBan.Src.Chat.Service;
using BookingBoardgamesILoveBan.Src.Model;

namespace BookingBoardgamesILoveBan.Src.Chat.Repository
{
    public interface IConversationRepository
    {
        List<Conversation> GetConversationsForUser(int userId);
        Conversation GetConversationById(int conversationId);

        void HandleNewMessage(Message message);
        void HandleMessageUpdate(Message message);
        void HandleReadReceipt(ReadReceipt readReceipt);

        int CreateConversation(int senderId, int receiverId);

        void HandleRentalRequestFinalization(int messageId);

        void CreateCashAgreementMessage(int messageIdOfParentRentalRequestMessage, int paymentId);

        void Subscribe(int userId, IConversationService observer);
        void Unsubscribe(int userId);
        void NotifySubscribersAboutMessage(Message message);
        void NotifySubscribersAboutMessageUpdate(Message message);
        void NotifySubscribersAboutNewConversation(Conversation conversation);
        void NotifySubscribersAboutReadReceipt(ReadReceipt readReceipt);
    }
}