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
        private IUserService userService;
        private int UserId { get; set; }

        public event Action<MessageDTO, string> MessageProcessed;
        public event Action<ConversationDTO, string> ConversationProcessed;
        public event Action<ReadReceiptDTO> ReadReceiptProcessed;
        public event Action<MessageDTO, string> MessageUpdateProcessed;

        public ConversationService(IConversationRepository conversationRepo, int userIdInput) : this(conversationRepo, userIdInput, App.UserService)
        {
        }

        public ConversationService(IConversationRepository conversationRepo, int userIdInput, IUserService userService)
        {
            UserId = userIdInput;
            ConversationRepository = conversationRepo;
            this.userService = userService;

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
            var user = userService.GetById(conversation.Participants[0] == UserId ? conversation.Participants[1] : conversation.Participants[0]);
            return user?.Username ?? "Unknown User";
        }

        /// <summary>
        /// Helper method to get the username of the other participant in a conversation, given a MessageDTO.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public string GetOtherUserNameByMessageDTO(MessageDTO message)
        {
            return userService.GetById(message.senderId == UserId ? message.receiverId : message.senderId).Username ?? "Unknown User";
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
                conversation.Participants.First(id => id != UserId),
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
            MessageProcessed?.Invoke(messageDTO, userName);
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
            ConversationProcessed?.Invoke(conversationDTO, userName);
        }

        /// <summary>
        /// Callback method invoked by the repository when a read receipt is received for this user.
        /// Translates the ReadReceipt to a ReadReceiptDTO,
        /// </summary>
        /// <param name="readReceipt"></param>
        public void OnReadReceiptReceived(ReadReceipt readReceipt)
        {
            ReadReceiptProcessed?.Invoke(ReadReceiptToReadReceiptDTO(readReceipt));
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
            MessageUpdateProcessed?.Invoke(messageDTO, userName);
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
            Message toReturn = messageDto.type switch
            {
                MessageType.Text => new TextMessage(
                    id: messageDto.id,
                    conversationId: messageDto.conversationId,
                    senderId: messageDto.senderId,
                    receiverId: messageDto.receiverId,
                    sentAt: messageDto.sentAt,
                    content: messageDto.content),
                MessageType.Image => new ImageMessage(
                    id: messageDto.id,
                    conversationId: messageDto.conversationId,
                    senderId: messageDto.senderId,
                    receiverId: messageDto.receiverId,
                    sentAt: messageDto.sentAt,
                    imageUrl: messageDto.imageUrl),
                MessageType.RentalRequest => new RentalRequestMessage(
                    id: messageDto.id,
                    conversationId: messageDto.conversationId,
                    senderId: messageDto.senderId,
                    receiverId: messageDto.receiverId,
                    sentAt: messageDto.sentAt,
                    content: messageDto.content,
                    requestId: messageDto.requestId,
                    isResolved: messageDto.isResolved,
                    isAccepted: messageDto.isAccepted),
                MessageType.CashAgreement => new CashAgreementMessage(
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
                MessageType.System => new SystemMessage(
                    id: messageDto.id,
                    conversationId: messageDto.conversationId,
                    sentAt: messageDto.sentAt,
                    content: messageDto.content),
            };
            return toReturn;
        }

        /// <summary>
        /// Helper method to translate a Message object to a MessageDTO. Uses pattern matching to determine the specific type of the
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public MessageDTO MessageToMessageDTO(Message message)
        {
            MessageDTO toReturn = new MessageDTO(
                id: message.Id,
                conversationId: message.ConversationId,
                senderId: message.SenderId,
                receiverId: message.ReceiverId,
                sentAt: message.SentAt,
                content: message.ContentAsString,
                type: message.Type,
                imageUrl: message is ImageMessage img ? img.ImageUrl : string.Empty,
                isResolved: message is RentalRequestMessage brm ? brm.IsResolved
                          : message is CashAgreementMessage cam ? cam.IsResolved
                          : false,
                isAccepted: message is RentalRequestMessage brm2 ? brm2.IsAccepted : false,
                isAcceptedByBuyer: message is CashAgreementMessage cam3 ? cam3.IsAcceptedByBuyer : false,
                isAcceptedBySeller: message is CashAgreementMessage cam4 ? cam4.IsAcceptedBySeller : false,
                paymentId: message is CashAgreementMessage cam5 ? cam5.PaymentId : -1,
                requestId: message is RentalRequestMessage brm3 ? brm3.RequestId : -1);
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
            var messageDTOs = conversation.MessageList.Select(mess => MessageToMessageDTO(mess)).ToList();
            return new ConversationDTO(
                conversationId: conversation.Id,
                participants: conversation.ParticipantIds,
                messages: messageDTOs,
                lastRead: conversation.LastRead);
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
                readReceipt.readerId,
                readReceipt.receiverId,
                readReceipt.timeStamp);
        }
        #endregion
    }
}
