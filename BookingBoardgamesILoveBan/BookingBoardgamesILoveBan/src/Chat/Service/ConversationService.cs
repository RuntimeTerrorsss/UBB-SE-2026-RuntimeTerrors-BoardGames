using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public event Action<MessageDTO, string> ActionMessageProcessed;
        public event Action<ConversationDTO, string> ActionConversationProcessed;
        public event Action<ReadReceiptDTO> ActionReadReceiptProcessed;
        public event Action<MessageDTO, string> ActionMessageUpdateProcessed;

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

        /// <summary>
        /// Fetches all conversations for the user and translates them to ConversationDTOs.
        /// Should only be called once on app startup, as after that conversations and messages
        /// will be pushed by the repository through the OnConversationReceived and OnMessageReceived callbacks.
        /// </summary>
        /// <returns></returns>
        public List<ConversationDTO> FetchConversations()
        {
            List<ConversationDTO> conversationList = new List<ConversationDTO>();

            foreach (var conversation in ConversationRepository.GetConversationsForUser(UserId))
            {
                conversationList.Add(ConversationToConversationDTO(conversation));
            }
            return conversationList;
        }

        /// <summary>
        /// Helper method to get the username of the other participant in a conversation, given a ConversationDTO.
        /// </summary>
        /// <param name="conversation"></param>
        /// <returns></returns>
        public string GetOtherUserNameByConversationDTO(ConversationDTO conversation)
        {
            var user = userRepository.GetById(conversation.Participants[0] == UserId ? conversation.Participants[1] : conversation.Participants[0]);
            return user?.Username ?? "Unknown User";
        }

        /// <summary>
        /// Helper method to get the username of the other participant in a conversation, given a MessageDTO.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public string GetOtherUserNameByMessageDTO(MessageDTO message)
        {
            return userRepository.GetById(message.senderId == UserId ? message.receiverId : message.senderId).Username ?? "Unknown User";
        }

        /// <summary>
        /// Sends a message by translating the MessageDTO to a Message and passing it to the repository's HandleNewMessage
        /// method, which will take care of saving it to the database and pushing it to the other participant.
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(MessageDTO message)
        {
            Debug.WriteLine("im sedingin a message: " + message.imageUrl);
            ConversationRepository.HandleNewMessage(MessageDTOToMessage(message));
        }

        /// <summary>
        /// Sends a message update by translating the MessageDTO to a Message and passing it to the repository's HandleMessageUpdate
        /// </summary>
        /// <param name="message"></param>
        public void UpdateMessage(MessageDTO message)
        {
            ConversationRepository.HandleMessageUpdate(MessageDTOToMessage(message));
        }

        /// <summary>
        /// Sends a read receipt by creating a new ReadReceipt object and passing it to the repository's HandleReadReceipt method,
        /// which will update the conversation's last read timestamp for the user and push the update to the other participant.
        /// </summary>
        /// <param name="conversation"></param>
        public void SendReadReceipt(ConversationDTO conversation)
        {
            ConversationRepository.HandleReadReceipt(new ReadReceipt(
                conversation.Id,
                UserId,
                conversation.Participants.First(p => p != UserId),
                DateTime.Now));
        }

        /// <summary>
        /// Handles the selection of the card payment option for a rental request by finalizing the rental request in the
        /// repository, which will update the corresponding RentalRequestMessage and push the update to the other participant.
        /// </summary>
        /// <param name="messageId"></param>
        public void OnCardPaymentSelected(int messageId)
        {
            FinalizeRentalRequest(messageId);
        }

        /// <summary>
        /// Handles the selection of the cash payment option for a rental request by finalizing the rental request in the
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="paymentId"></param>
        public void OnCashPaymentSelected(int messageId, int paymentId)
        {
            FinalizeRentalRequest(messageId);
            SendCashAgreementMessage(messageId, paymentId);
        }

        /// <summary>
        /// Helper method to finalize a rental request by calling the repository's HandleRentalRequestFinalization method,
        /// which will update the corresponding RentalRequestMessage and push the update to the other participant.
        /// </summary>
        /// <param name="messageId"></param>
        private void FinalizeRentalRequest(int messageId)
        {
            ConversationRepository.HandleRentalRequestFinalization(messageId);
        }

        /// <summary>
        /// Helper method to send a cash agreement message by calling the repository's CreateCashAgreementMessage method,
        /// which will create a new CashAgreementMessage linked to the original RentalRequestMessage and push it to the other
        /// participant.
        /// </summary>
        /// <param name="messageIdOfParentRentalRequestMessage"></param>
        /// <param name="paymentId"></param>
        private void SendCashAgreementMessage(int messageIdOfParentRentalRequestMessage, int paymentId)
        {
            ConversationRepository.CreateCashAgreementMessage(messageIdOfParentRentalRequestMessage, paymentId);
        }

        /// <summary>
        /// Callback method invoked by the repository when a new message is
        /// received for this user. Translates the Message to a MessageDTO,
        /// </summary>
        /// <param name="message"></param>
        public void OnMessageReceived(Message message)
        {
            MessageDTO messageDTO = MessageToMessageDTO(message);
            string userName = GetOtherUserNameByMessageDTO(messageDTO);
            ActionMessageProcessed?.Invoke(messageDTO, userName);
        }

        /// <summary>
        /// Callback method invoked by the repository when a new conversation is created for this user.
        /// Translates the Conversation to a ConversationDTO,
        /// </summary>
        /// <param name="conversation"></param>
        public void OnConversationReceived(Conversation conversation)
        {
            ConversationDTO conversationDTO = ConversationToConversationDTO(conversation);
            string userName = GetOtherUserNameByConversationDTO(conversationDTO);
            ActionConversationProcessed?.Invoke(conversationDTO, userName);
        }

        /// <summary>
        /// Callback method invoked by the repository when a read receipt is received for this user.
        /// Translates the ReadReceipt to a ReadReceiptDTO,
        /// </summary>
        /// <param name="readReceipt"></param>
        public void OnReadReceiptReceived(ReadReceipt readReceipt)
        {
            ActionReadReceiptProcessed?.Invoke(ReadReceiptToReadReceiptDTO(readReceipt));
        }

        /// <summary>
        /// Callback method invoked by the repository when a message update is received for this user
        /// (e.g. rental request finalization, cash agreement acceptance).
        /// </summary>
        /// <param name="message"></param>
        public void OnMessageUpdateReceived(Message message)
        {
            MessageDTO messageDTO = MessageToMessageDTO(message);
            string userName = GetOtherUserNameByMessageDTO(messageDTO);
            ActionMessageUpdateProcessed?.Invoke(messageDTO, userName);
        }

        #region domain-to-dto translations

        /// <summary>
        /// Helper method to translate a Message object to a MessageDTO.
        /// Uses pattern matching to determine the specific type of the
        /// message and populate the relevant fields in the DTO accordingly.
        /// </summary>
        /// <param name="messageDto"></param>
        /// <returns></returns>
        public Message MessageDTOToMessage(MessageDTO messageDto)
        {
            Message messageToReturn = messageDto.type switch
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
            return messageToReturn;
        }

        /// <summary>
        /// Helper method to translate a Message object to a MessageDTO. Uses pattern matching to determine the specific type of the
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public MessageDTO MessageToMessageDTO(Message message)
        {
            MessageDTO toReturn = new MessageDTO(
                id: message.MessageId,
                conversationId: message.ConversationId,
                senderId: message.MessageSenderId,
                receiverId: message.MessageReceiverId,
                sentAt: message.MessageSentTime,
                content: message.MessageContentAsString,
                type: message.TypeOfMessage,
                imageUrl: message is ImageMessage img ? img.MessageImageUrl : string.Empty,
                isResolved: message is RentalRequestMessage brm ? brm.IsRequestResolved
                          : message is CashAgreementMessage cam ? cam.IsCashAgreementResolved
                          : false,
                isAccepted: message is RentalRequestMessage brm2 ? brm2.IsRequestAccepted : false,
                isAcceptedByBuyer: message is CashAgreementMessage cam3 ? cam3.IsCashAgreementAcceptedByBuyer : false,
                isAcceptedBySeller: message is CashAgreementMessage cam4 ? cam4.IsCashAgreementAcceptedBySeller : false,
                paymentId: message is CashAgreementMessage cam5 ? cam5.CashPaymentId : -1,
                requestId: message is RentalRequestMessage brm3 ? brm3.RentalRequestId : -1);
            return toReturn;
        }

        /// <summary>
        /// Helper method to translate a Conversation object to a ConversationDTO.
        /// Translates the list of Message objects in the conversation
        /// </summary>
        /// <param name="conversation"></param>
        /// <returns></returns>
        public ConversationDTO ConversationToConversationDTO(Conversation conversation)
        {
            var messageDTOs = conversation.ConversationMessageList.Select(mess => MessageToMessageDTO(mess)).ToList();
            return new ConversationDTO(
                conversationId: conversation.ConversationId,
                participants: conversation.ConversationParticipantIds,
                messages: messageDTOs,
                lastRead: conversation.LastMessageReadTime);
        }

        /// <summary>
        /// Helper method to translate a ReadReceipt object to a ReadReceiptDTO.
        /// </summary>
        /// <param name="readReceipt"></param>
        /// <returns></returns>
        public ReadReceiptDTO ReadReceiptToReadReceiptDTO(ReadReceipt readReceipt)
        {
            return new ReadReceiptDTO(
                readReceipt.conversationId,
                readReceipt.messageReaderId,
                readReceipt.messageReceiverId,
                readReceipt.timeStamp);
        }
        #endregion
    }
}
