using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BookingBoardgamesILoveBan.Src.Chat.DTO;
using BookingBoardgamesILoveBan.Src.Chat.Model;
using BookingBoardgamesILoveBan.Src.Chat.Repository;
using BookingBoardgamesILoveBan.Src.Enum;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using BookingBoardgamesILoveBan.Src.Model;

namespace BookingBoardgamesILoveBan.Src.Chat.Service
{
    public class ConversationService : IConversationService
    {
        private IConversationRepository ConversationRepository { get; set; }
        private IUserRepository userRepository;
        private int UserId { get; set; }

        public event Action<MessageDataTransferObject, string> ActionMessageProcessed;
        public event Action<ConversationDataTransferObject, string> ActionConversationProcessed;
        public event Action<ReadReceiptDataTransferObject> ActionReadReceiptProcessed;
        public event Action<MessageDataTransferObject, string> ActionMessageUpdateProcessed;

        public ConversationService(IConversationRepository conversationRepo, int userIdInput) : this(conversationRepo, userIdInput, App.UserRepository)
        {
        }

        public ConversationService(IConversationRepository conversationRepo, int userIdInput, IUserRepository userRepo)
        {
            UserId = userIdInput;
            ConversationRepository = conversationRepo;
            userRepository = userRepo;

            ConversationRepository.Subscribe(UserId, this);
        }

        public List<ConversationDataTransferObject> FetchConversations()
        {
            List<ConversationDataTransferObject> conversationList = new List<ConversationDataTransferObject>();

            foreach (var conversation in ConversationRepository.GetConversationsForUser(UserId))
            {
                conversationList.Add(ConversationToConversationDTO(conversation));
            }
            return conversationList;
        }

        public string GetOtherUserNameByConversationDTO(ConversationDataTransferObject conversation)
        {
            int firstParticipantIndex = 0;
            int secondParticipantIndex = 1;
            var user = userRepository.GetById(conversation.Participants[firstParticipantIndex] == UserId ? conversation.Participants[secondParticipantIndex] : conversation.Participants[firstParticipantIndex]);
            return user?.Username ?? "Unknown User";
        }

        public string GetOtherUserNameByMessageDTO(MessageDataTransferObject message)
        {
            return userRepository.GetById(message.senderId == UserId ? message.receiverId : message.senderId).Username ?? "Unknown User";
        }

        public void SendMessage(MessageDataTransferObject message)
        {
            ConversationRepository.HandleNewMessage(MessageDTOToMessage(message));
        }

        public void UpdateMessage(MessageDataTransferObject message)
        {
            ConversationRepository.HandleMessageUpdate(MessageDTOToMessage(message));
        }

        public void SendReadReceipt(ConversationDataTransferObject conversation)
        {
            ConversationRepository.HandleReadReceipt(new ReadReceipt(
                conversation.Id,
                UserId,
                conversation.Participants.First(participant => participant != UserId),
                DateTime.Now));
        }

        public void OnCardPaymentSelected(int messageId)
        {
            FinalizeRentalRequest(messageId);
        }

        public void OnCashPaymentSelected(int messageId, int paymentId)
        {
            FinalizeRentalRequest(messageId);
            SendCashAgreementMessage(messageId, paymentId);
        }

        private void FinalizeRentalRequest(int messageId)
        {
            ConversationRepository.HandleRentalRequestFinalization(messageId);
        }

        private void SendCashAgreementMessage(int messageIdOfParentRentalRequestMessage, int paymentId)
        {
            ConversationRepository.CreateCashAgreementMessage(messageIdOfParentRentalRequestMessage, paymentId);
        }

        public void OnMessageReceived(Message message)
        {
            MessageDataTransferObject messageDTO = MessageToMessageDTO(message);
            string userName = GetOtherUserNameByMessageDTO(messageDTO);
            ActionMessageProcessed?.Invoke(messageDTO, userName);
        }

        public void OnConversationReceived(Conversation conversation)
        {
            ConversationDataTransferObject conversationDTO = ConversationToConversationDTO(conversation);
            string userName = GetOtherUserNameByConversationDTO(conversationDTO);
            ActionConversationProcessed?.Invoke(conversationDTO, userName);
        }

        public void OnReadReceiptReceived(ReadReceipt readReceipt)
        {
            ActionReadReceiptProcessed?.Invoke(ReadReceiptToReadReceiptDTO(readReceipt));
        }

        public void OnMessageUpdateReceived(Message message)
        {
            MessageDataTransferObject messageDTO = MessageToMessageDTO(message);
            string userName = GetOtherUserNameByMessageDTO(messageDTO);
            ActionMessageUpdateProcessed?.Invoke(messageDTO, userName);
        }

        public Message MessageDTOToMessage(MessageDataTransferObject messageDto)
        {
            Message toReturn = messageDto.type switch
            {
                MessageType.MessageText => new TextMessage(
                    id: messageDto.id,
                    conversationId: messageDto.conversationId,
                    senderId: messageDto.senderId,
                    receiverId: messageDto.receiverId,
                    sentAt: messageDto.sentAt,
                    content: messageDto.content),
                MessageType.MessageImage => new ImageMessage(
                    id: messageDto.id,
                    conversationId: messageDto.conversationId,
                    senderId: messageDto.senderId,
                    receiverId: messageDto.receiverId,
                    sentAt: messageDto.sentAt,
                    imageUrl: messageDto.imageUrl),
                MessageType.MessageRentalRequest => new RentalRequestMessage(
                    id: messageDto.id,
                    conversationId: messageDto.conversationId,
                    senderId: messageDto.senderId,
                    receiverId: messageDto.receiverId,
                    sentAt: messageDto.sentAt,
                    content: messageDto.content,
                    requestId: messageDto.requestId,
                    isResolved: messageDto.isResolved,
                    isAccepted: messageDto.isAccepted),
                MessageType.MessageCashAgreement => new CashAgreementMessage(
                    id: messageDto.id,
                    conversationId: messageDto.conversationId,
                    sellerId: messageDto.senderId,
                    buyerId: messageDto.receiverId,
                    paymentId: messageDto.paymentId,
                    sentAt: messageDto.sentAt,
                    content: messageDto.content,
                    isResolved: messageDto.isResolved,
                    isAcceptedByBuyer: messageDto.isAcceptedByBuyer,
                    isAcceptedBySeller: messageDto.isAcceptedBySeller),
                MessageType.MessageSystem => new SystemMessage(
                    id: messageDto.id,
                    conversationId: messageDto.conversationId,
                    sentAt: messageDto.sentAt,
                    content: messageDto.content),
            };
            return toReturn;
        }

        public MessageDataTransferObject MessageToMessageDTO(Message message)
        {
            int defaultMissingIdentifier = -1;

            MessageDataTransferObject toReturn = new MessageDataTransferObject(
                id: message.MessageId,
                conversationId: message.ConversationId,
                senderId: message.MessageSenderId,
                receiverId: message.MessageReceiverId,
                sentAt: message.MessageSentTime,
                content: message.MessageContentAsString,
                type: message.TypeOfMessage,
                imageUrl: message is ImageMessage imageMessage ? imageMessage.MessageImageUrl : string.Empty,
                isResolved: message is RentalRequestMessage rentalResolvedMessage ? rentalResolvedMessage.IsRequestResolved
                          : message is CashAgreementMessage cashResolvedMessage ? cashResolvedMessage.IsCashAgreementResolved
                          : false,
                isAccepted: message is RentalRequestMessage rentalAcceptedMessage ? rentalAcceptedMessage.IsRequestAccepted : false,
                isAcceptedByBuyer: message is CashAgreementMessage cashBuyerMessage ? cashBuyerMessage.IsCashAgreementAcceptedByBuyer : false,
                isAcceptedBySeller: message is CashAgreementMessage cashSellerMessage ? cashSellerMessage.IsCashAgreementAcceptedBySeller : false,
                paymentId: message is CashAgreementMessage cashPaymentMessage ? cashPaymentMessage.CashPaymentId : defaultMissingIdentifier,
                requestId: message is RentalRequestMessage rentalRequestMessage ? rentalRequestMessage.RentalRequestId : defaultMissingIdentifier);
            return toReturn;
        }

        public ConversationDataTransferObject ConversationToConversationDTO(Conversation conversation)
        {
            var messageDTOs = conversation.ConversationMessageList.Select(messageItem => MessageToMessageDTO(messageItem)).ToList();
            return new ConversationDataTransferObject(
                conversationId: conversation.ConversationId,
                participants: conversation.ConversationParticipantIds,
                messages: messageDTOs,
                lastRead: conversation.LastMessageReadTime);
        }

        public ReadReceiptDataTransferObject ReadReceiptToReadReceiptDTO(ReadReceipt readReceipt)
        {
            return new ReadReceiptDataTransferObject(
                readReceipt.conversationId,
                readReceipt.messageReaderId,
                readReceipt.messageReceiverId,
                readReceipt.timeStamp);
        }
    }
}