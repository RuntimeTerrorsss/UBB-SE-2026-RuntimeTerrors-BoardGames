using BookingBoardgamesILoveBan.src.Chat.DTO;
using BookingBoardgamesILoveBan.src.Chat.Model;
using BookingBoardgamesILoveBan.src.Chat.Repository;
using BookingBoardgamesILoveBan.src.Enum;
using BookingBoardgamesILoveBan.src.Mocks.UserMock;
using BookingBoardgamesILoveBan.src.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardgamesILoveBan.src.Chat.Service
{
    public class ConversationService : MessageObserver
    {
        private ConversationRepository _conversationRepository { get; set; }
        private UserService _userService = App.UserService;
        private int _userId { get; set; }
        public int UserId
        {
            get => _userId;
        }

        public event Action<MessageDTO, string> MessageProcessed;
        public event Action<ConversationDTO, string> ConversationProcessed;
        public event Action<ReadReceiptDTO> ReadReceiptProcessed;
        public event Action<MessageDTO, string> MessageUpdateProcessed;

        public ConversationService(ConversationRepository conversationRepository, int userId)
        {
            _userId = userId;
            _conversationRepository = conversationRepository;
            _conversationRepository.Subscribe(userId, this);
        }

        /// <summary>
        /// Fetches all conversations for the user and translates them to ConversationDTOs. 
        /// Should only be called once on app startup, as after that conversations and messages
        /// will be pushed by the repository through the OnConversationReceived and OnMessageReceived callbacks.
        /// </summary>
        /// <returns></returns>
        public List<ConversationDTO> FetchConversations()
        {
            List<ConversationDTO> convList = new List<ConversationDTO>();

            foreach (var conv in _conversationRepository.GetConversationsForUser(_userId))
            {
                convList.Add(ConversationToConversationDTO(conv));
            }
            return convList;
        }

        /// <summary>
        /// Helper method to get the username of the other participant in a conversation, given a ConversationDTO.
        /// </summary>
        /// <param name="conversation"></param>
        /// <returns></returns>
        public string GetOtherUserNameByConversationDTO(ConversationDTO conversation)
        {
            return _userService.GetById(conversation.Participants[0] == _userId ? conversation.Participants[1] : conversation.Participants[0]).Username ?? "Unknown User";
        }

        /// <summary>
        /// Helper method to get the username of the other participant in a conversation, given a MessageDTO.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public string GetOtherUserNameByMessageDTO(MessageDTO message)
        {
            return _userService.GetById(message.SenderId == _userId ? message.ReceiverId : message.SenderId).Username ?? "Unknown User";
        }

        /// <summary>
        /// Sends a message by translating the MessageDTO to a Message and passing it to the repository's HandleNewMessage
        /// method, which will take care of saving it to the database and pushing it to the other participant.
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(MessageDTO message)
        {
            Debug.WriteLine("im sedingin a message: " + message.ImageUrl);
            _conversationRepository.HandleNewMessage(MessageDTOToMessage(message));
        }

        /// <summary>
        /// Sends a message update by translating the MessageDTO to a Message and passing it to the repository's HandleMessageUpdate
        /// </summary>
        /// <param name="message"></param>
        public void UpdateMessage(MessageDTO message)
        {
            _conversationRepository.HandleMessageUpdate(MessageDTOToMessage(message));
        }

        /// <summary>
        /// Sends a read receipt by creating a new ReadReceipt object and passing it to the repository's HandleReadReceipt method, 
        /// which will update the conversation's last read timestamp for the user and push the update to the other participant.
        /// </summary>
        /// <param name="conversation"></param>
        public void SendReadReceipt(ConversationDTO conversation)
        {
            _conversationRepository.HandleReadReceipt(new ReadReceipt
            (
                conversation.Id,
                _userId,
                conversation.Participants.First(p => p != _userId),
                DateTime.Now
            ));
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
            _conversationRepository.HandleRentalRequestFinalization(messageId);
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
            _conversationRepository.CreateCashAgreementMessage(messageIdOfParentRentalRequestMessage, paymentId);
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
            MessageUpdateProcessed?.Invoke(messageDTO,userName);
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
            Message toReturn = messageDto.Type switch
            {
                MessageType.Text => new TextMessage(
                    id: messageDto.Id,
                    conversationId: messageDto.ConversationId,
                    senderId: messageDto.SenderId,
                    receiverId: messageDto.ReceiverId,
                    sentAt: messageDto.SentAt,
                    content: messageDto.Content
                ),
                MessageType.Image => new ImageMessage(
                    id: messageDto.Id,
                    conversationId: messageDto.ConversationId,
                    senderId: messageDto.SenderId,
                    receiverId: messageDto.ReceiverId,
                    sentAt: messageDto.SentAt,
                    imageUrl: messageDto.ImageUrl
                ),
                MessageType.RentalRequest => new RentalRequestMessage(
                    id: messageDto.Id,
                    conversationId: messageDto.ConversationId,
                    senderId: messageDto.SenderId,
                    receiverId: messageDto.ReceiverId,
                    sentAt: messageDto.SentAt,
                    content: messageDto.Content,
                    requestId: messageDto.RequestId,
                    isResolved: messageDto.IsResolved,
                    isAccepted: messageDto.IsAccepted
                ),
                MessageType.CashAgreement => new CashAgreementMessage(
                    id: messageDto.Id,
                    conversationId: messageDto.ConversationId,
                    sellerId: messageDto.SenderId,
                    buyerId: messageDto.ReceiverId,
                    paymentId: messageDto.PaymentId,
                    sentAt: messageDto.SentAt,
                    content: messageDto.Content,
                    isResolved: messageDto.IsResolved,
                    isAcceptedByBuyer: messageDto.IsAcceptedByBuyer,
                    isAcceptedBySeller: messageDto.IsAcceptedBySeller
                ),
                MessageType.System => new SystemMessage(
                    id: messageDto.Id,
                    conversationId: messageDto.ConversationId,
                    sentAt: messageDto.SentAt,
                    content: messageDto.Content
                ),
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
                Id: message.Id,
                ConversationId: message.ConversationId,
                SenderId: message.SenderId,
                ReceiverId: message.ReceiverId,
                SentAt: message.SentAt,
                Content: message.ContentAsString,
                Type: message.Type,
                ImageUrl: message is ImageMessage img ? img.ImageUrl : "",
                IsResolved: message is RentalRequestMessage brm ? brm.IsResolved
                          : message is CashAgreementMessage cam ? cam.IsResolved
                          : false,
                IsAccepted: message is RentalRequestMessage brm2 ? brm2.IsAccepted : false,
                IsAcceptedByBuyer: message is CashAgreementMessage cam3 ? cam3.IsAcceptedByBuyer : false,
                IsAcceptedBySeller: message is CashAgreementMessage cam4 ? cam4.IsAcceptedBySeller : false,
                PaymentId: message is CashAgreementMessage cam5 ? cam5.PaymentId : -1,
                RequestId: message is RentalRequestMessage brm3 ? brm3.RequestId : -1
            );
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
                convId: conversation.Id,
                participants: conversation.ParticipantIds,
                messages: messageDTOs,
                lastRead: conversation.LastRead
            );
        }

        /// <summary>
        /// Helper method to translate a ReadReceipt object to a ReadReceiptDTO.
        /// </summary>
        /// <param name="readReceipt"></param>
        /// <returns></returns>
        public ReadReceiptDTO ReadReceiptToReadReceiptDTO(ReadReceipt readReceipt)
        {
            return new ReadReceiptDTO(
                readReceipt.ConversationId,
                readReceipt.ReaderId,
                readReceipt.ReceiverId,
                readReceipt.TimeStamp
            );
        }
        #endregion
    }
}
