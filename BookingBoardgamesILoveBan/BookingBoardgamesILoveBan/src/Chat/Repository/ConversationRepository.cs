using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.Chat.Model;
using BookingBoardgamesILoveBan.Src.Chat.Service;
using BookingBoardgamesILoveBan.Src.Enum;
using BookingBoardgamesILoveBan.Src.Model;
using Microsoft.Data.SqlClient;

namespace BookingBoardgamesILoveBan.Src.Chat.Repository
{
    public class ConversationRepository : IConversationRepository
    {
        private Dictionary<int, IConversationService> Subscribers { get; set; }
        private static string appConnectionString;

        public ConversationRepository()
        {
            Subscribers = new Dictionary<int, IConversationService>();
            appConnectionString = DatabaseBootstrap.GetAppConnection();
        }

        #region Public Methods

        public List<Conversation> GetConversationsForUser(int userId)
        {
            return LoadConversationsForUserFromDB(userId);
        }

        public Conversation GetConversationById(int conversationId)
        {
            return LoadConversationFromDB(conversationId);
        }

        public void HandleNewMessage(Message message)
        {
            Debug.WriteLine(message.MessageId);
            var conversation = GetConversationById(message.ConversationId);
            if (conversation != null)
            {
                conversation.ConversationMessageList.Add(message);
                message.MessageId = AddMessageToDB(message);
                NotifySubscribersAboutMessage(message);
            }
            else
            {
                throw new InvalidOperationException("Conversation not found.");
            }
        }

        public void HandleReadReceipt(ReadReceipt readReceipt)
        {
            UpdateLastReadInDB(readReceipt);
            NotifySubscribersAboutReadReceipt(readReceipt);
        }

        public void HandleMessageUpdate(Message message)
        {
            if (message is CashAgreementMessage cashAgreementMessage)
            {
                if (cashAgreementMessage.IsCashAgreementAcceptedByBuyer &&
                    cashAgreementMessage.IsCashAgreementAcceptedBySeller)
                {
                    UpdateCashPaymentFromMessageUpdate(cashAgreementMessage);
                }
            }
            UpdateMessageToDB(message);
            NotifySubscribersAboutMessageUpdate(message);
        }

        public int CreateConversation(int senderId, int receiverId)
        {
            Conversation newConversation = CreateConversationInDB(senderId, receiverId);
            if (newConversation.ConversationMessageList.Count == 0)
            {
                int unassignedMessageIdentifier = -1;
                NotifySubscribersAboutNewConversation(newConversation);
                HandleNewMessage(new SystemMessage(unassignedMessageIdentifier, newConversation.ConversationId, DateTime.Now, "New conversation"));
                NotifySubscribersAboutNewConversation(newConversation);
            }
            return newConversation.ConversationId;
        }

        public void HandleRentalRequestFinalization(int messageId)
        {
            try
            {
                RentalRequestMessage message = (RentalRequestMessage)GetMessageById(messageId);
                message.IsRequestResolved = true;
                message.RequestContent += "\n\nThis request has been finalized!";
                HandleMessageUpdate(message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"You tried to accept a message that wasnt a rental request... how? {ex.Message}");
            }
        }

        #endregion

        #region Database Communication

        private Message GetMessageById(int messageId)
        {
            using (var connection = new SqlConnection(appConnectionString))
            {
                string query = @"SELECT
                    m.mid, m.senderId, m.receiverId, m.sentAt, m.messageType, m.ConversationId,
                    tm.content AS textContent, im.content AS imageContent, cam.content AS cashContent, 
                    rrm.content AS rentalContent, cam.sellerId, cam.buyerId, cam.acceptedBySeller,  
                    cam.acceptedByBuyer, rrm.requestId, rrm.isResolved, rrm.isAccepted, sysm.content,
                    cam.PaymentId
                    FROM [Message] m
                    LEFT JOIN TextMessage tm ON tm.mid = m.mid
                    LEFT JOIN ImageMessage im ON im.mid = m.mid
                    LEFT JOIN CashAgreementMessage cam ON cam.mid = m.mid
                    LEFT JOIN RentalRequestMessage rrm ON rrm.mid = m.mid
                    LEFT JOIN SystemMessage sysm ON sysm.mid = m.mid
                    WHERE m.mid = @mid;";

                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@mid", messageId);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    int senderIdColumnIndex = 1;
                    int receiverIdColumnIndex = 2;
                    int timestampColumnIndex = 3;
                    int messageTypeColumnIndex = 4;
                    int conversationIdColumnIndex = 5;
                    int textContentColumnIndex = 6;
                    int imageContentColumnIndex = 7;
                    int cashContentColumnIndex = 8;
                    int rentalContentColumnIndex = 9;
                    int sellerIdColumnIndex = 10;
                    int buyerIdColumnIndex = 11;
                    int acceptedBySellerColumnIndex = 12;
                    int acceptedByBuyerColumnIndex = 13;
                    int requestIdColumnIndex = 14;
                    int isResolvedColumnIndex = 15;
                    int isAcceptedColumnIndex = 16;
                    int systemContentColumnIndex = 17;
                    int paymentIdColumnIndex = 18;

                    int unassignedIdentifier = -1;

                    int senderId = reader.GetInt32(senderIdColumnIndex);
                    int receiverId = reader.GetInt32(receiverIdColumnIndex);
                    DateTime timestamp = reader.GetDateTime(timestampColumnIndex);
                    string messageType = reader.GetString(messageTypeColumnIndex);
                    int conversationId = reader.GetInt32(conversationIdColumnIndex);

                    switch (messageType)
                    {
                        case "TEXT":
                            return new TextMessage(messageId, conversationId, senderId, receiverId, timestamp, reader.GetString(textContentColumnIndex));

                        case "IMAGE":
                            return new ImageMessage(messageId, conversationId, senderId, receiverId, timestamp, reader.GetString(imageContentColumnIndex));

                        case "CASH_AGREEMENT":
                            bool isAcceptedByBuyer = reader.IsDBNull(acceptedByBuyerColumnIndex) ? false : reader.GetBoolean(acceptedByBuyerColumnIndex);
                            bool isAcceptedBySeller = reader.IsDBNull(acceptedBySellerColumnIndex) ? false : reader.GetBoolean(acceptedBySellerColumnIndex);
                            int paymentId = reader.IsDBNull(paymentIdColumnIndex) ? unassignedIdentifier : reader.GetInt32(paymentIdColumnIndex);

                            return new CashAgreementMessage(
                                messageId, conversationId,
                                reader.IsDBNull(sellerIdColumnIndex) ? unassignedIdentifier : reader.GetInt32(sellerIdColumnIndex),
                                reader.IsDBNull(buyerIdColumnIndex) ? unassignedIdentifier : reader.GetInt32(buyerIdColumnIndex),
                                paymentId,
                                timestamp,
                                (string)reader[cashContentColumnIndex],
                                isAcceptedByBuyer && isAcceptedBySeller,
                                isAcceptedByBuyer,
                                isAcceptedBySeller);

                        case "RENTAL_REQUEST":
                            return new RentalRequestMessage(
                                messageId, conversationId, senderId, receiverId, timestamp,
                                (string)reader[rentalContentColumnIndex],
                                reader.IsDBNull(requestIdColumnIndex) ? unassignedIdentifier : reader.GetInt32(requestIdColumnIndex),
                                reader.IsDBNull(isResolvedColumnIndex) ? false : reader.GetBoolean(isResolvedColumnIndex),
                                reader.IsDBNull(isAcceptedColumnIndex) ? false : reader.GetBoolean(isAcceptedColumnIndex));

                        case "SYSTEM":
                            return new SystemMessage(messageId, conversationId, timestamp, reader.GetString(systemContentColumnIndex));

                        default:
                            return null;
                    }
                }
            }
        }

        private int[] GetConversationParticipants(int conversationId)
        {
            var participantIds = new List<int>();
            int userIdColumnIndex = 0;

            using (var connection = new SqlConnection(appConnectionString))
            {
                string query = "SELECT uid FROM [ConversationUser] WHERE cid = @cid";
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@cid", conversationId);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        participantIds.Add(reader.GetInt32(userIdColumnIndex));
                    }
                }
                connection.Close();
            }
            return participantIds.ToArray();
        }

        private List<Conversation> LoadConversationsForUserFromDB(int userId)
        {
            var userConversations = new List<Conversation>();
            int conversationIdColumnIndex = 0;

            using (var connection = new SqlConnection(appConnectionString))
            {
                string joinQuery = @"
                SELECT C.cid 
                FROM [Conversation] C
                INNER JOIN [ConversationUser] CU ON C.cid = CU.cid
                WHERE CU.uid = @uid";

                var command = new SqlCommand(joinQuery, connection);
                command.Parameters.AddWithValue("@uid", userId);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int conversationId = reader.GetInt32(conversationIdColumnIndex);
                        var conversation = LoadSingleConversationFromDB(conversationId, connection);
                        userConversations.Add(conversation);
                    }
                }
                connection.Close();
            }

            return userConversations;
        }

        private Conversation LoadSingleConversationFromDB(int conversationId, SqlConnection connection)
        {
            List<int> participantIds = new List<int>();
            Dictionary<int, DateTime> lastRead = new Dictionary<int, DateTime>();
            string participantsSql = "SELECT uid, LastRead FROM [ConversationUser] WHERE cid = @cid";

            int userIdColumnIndex = 0;
            int lastReadColumnIndex = 1;

            using (var command = new SqlCommand(participantsSql, connection))
            {
                command.Parameters.AddWithValue("@cid", conversationId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int userId = reader.GetInt32(userIdColumnIndex);
                        DateTime lastReadByUser = reader.IsDBNull(lastReadColumnIndex) ? DateTime.MinValue : reader.GetDateTime(lastReadColumnIndex);
                        lastRead.Add(userId, lastReadByUser);
                        participantIds.Add(userId);
                    }
                }
            }

            List<Message> messages = LoadMessagesForConversationFromDB(conversationId);
            return new Conversation(conversationId, participantIds.ToArray(), messages, lastRead);
        }

        private Conversation LoadConversationFromDB(int conversationId)
        {
            Conversation retrievedConversation = null;
            using (var connection = new SqlConnection(appConnectionString))
            {
                connection.Open();
                retrievedConversation = LoadSingleConversationFromDB(conversationId, connection);
                connection.Close();
            }
            return retrievedConversation;
        }

        private List<Message> LoadMessagesForConversationFromDB(int conversationId)
        {
            var messages = new List<Message>();
            using (var connection = new SqlConnection(appConnectionString))
            {
                string query = @"SELECT
                    m.mid, m.senderId, m.receiverId, m.sentAt, m.messageType, tm.content AS textContent, im.content AS imageContent, cam.content AS cashContent, rrm.content AS rentalContent, cam.sellerId, cam.buyerId, cam.acceptedBySeller, cam.acceptedByBuyer, rrm.requestId, rrm.isResolved, rrm.isAccepted, sysm.content, cam.PaymentId
                    FROM [Message] m
                    LEFT JOIN TextMessage tm ON tm.mid = m.mid
                    LEFT JOIN ImageMessage im ON im.mid = m.mid
                    LEFT JOIN CashAgreementMessage cam ON cam.mid = m.mid
                    LEFT JOIN RentalRequestMessage rrm ON rrm.mid = m.mid
                    LEFT JOIN SystemMessage sysm ON sysm.mid = m.mid
                    WHERE m.ConversationId = @cid;";
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@cid", conversationId);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    int messageIdColumnIndex = 0;
                    int senderIdColumnIndex = 1;
                    int receiverIdColumnIndex = 2;
                    int timestampColumnIndex = 3;
                    int messageTypeColumnIndex = 4;
                    int textContentColumnIndex = 5;
                    int imageContentColumnIndex = 6;
                    int cashContentColumnIndex = 7;
                    int rentalContentColumnIndex = 8;
                    int sellerIdColumnIndex = 9;
                    int buyerIdColumnIndex = 10;
                    int acceptedBySellerColumnIndex = 11;
                    int acceptedByBuyerColumnIndex = 12;
                    int requestIdColumnIndex = 13;
                    int isResolvedColumnIndex = 14;
                    int isAcceptedColumnIndex = 15;
                    int systemContentColumnIndex = 16;
                    int paymentIdColumnIndex = 17;
                    int unassignedIdentifier = -1;

                    while (reader.Read())
                    {
                        int messageId = reader.GetInt32(messageIdColumnIndex);
                        int senderId = reader.GetInt32(senderIdColumnIndex);
                        int receiverId = reader.GetInt32(receiverIdColumnIndex);
                        DateTime timestamp = reader.GetDateTime(timestampColumnIndex);
                        string messageType = reader.GetString(messageTypeColumnIndex);

                        switch (messageType)
                        {
                            case "TEXT":
                                string textContent = reader.GetString(textContentColumnIndex);
                                messages.Add(new TextMessage(messageId, conversationId, senderId, receiverId, timestamp, textContent));
                                break;

                            case "IMAGE":
                                var imageContent = reader.GetString(imageContentColumnIndex);
                                messages.Add(new ImageMessage(messageId, conversationId, senderId, receiverId, timestamp, imageContent));
                                break;

                            case "CASH_AGREEMENT":
                                string cashContent = reader[cashContentColumnIndex] as string;
                                int sellerId = reader.IsDBNull(sellerIdColumnIndex) ? unassignedIdentifier : reader.GetInt32(sellerIdColumnIndex);
                                int buyerId = reader.IsDBNull(buyerIdColumnIndex) ? unassignedIdentifier : reader.GetInt32(buyerIdColumnIndex);
                                bool acceptedBySeller = reader.IsDBNull(acceptedBySellerColumnIndex) ? false : reader.GetBoolean(acceptedBySellerColumnIndex);
                                bool acceptedByBuyer = reader.IsDBNull(acceptedByBuyerColumnIndex) ? false : reader.GetBoolean(acceptedByBuyerColumnIndex);
                                int paymentId = reader.IsDBNull(paymentIdColumnIndex) ? unassignedIdentifier : reader.GetInt32(paymentIdColumnIndex);
                                messages.Add(new CashAgreementMessage(
                                    messageId,
                                    conversationId,
                                    sellerId,
                                    buyerId,
                                    paymentId,
                                    timestamp,
                                    cashContent,
                                    false,
                                    acceptedByBuyer,
                                    acceptedBySeller));
                                break;

                            case "RENTAL_REQUEST":
                                string rentalContent = reader[rentalContentColumnIndex] as string;
                                int requestId = reader.IsDBNull(requestIdColumnIndex) ? unassignedIdentifier : reader.GetInt32(requestIdColumnIndex);
                                bool isResolved = reader.IsDBNull(isResolvedColumnIndex) ? false : reader.GetBoolean(isResolvedColumnIndex);
                                bool isAccepted = reader.IsDBNull(isAcceptedColumnIndex) ? false : reader.GetBoolean(isAcceptedColumnIndex);
                                messages.Add(new RentalRequestMessage(messageId, conversationId, senderId, receiverId, timestamp, rentalContent, requestId, isResolved, isAccepted));
                                break;

                            case "SYSTEM":
                                string systemContent = reader.GetString(systemContentColumnIndex);
                                messages.Add(new SystemMessage(messageId, conversationId, timestamp, systemContent));
                                break;
                        }
                    }
                }
                connection.Close();
            }
            return messages;
        }

        private int AddMessageToDB(Message message)
        {
            using (var connection = new SqlConnection(appConnectionString))
            {
                string query = @"
                INSERT INTO Message (conversationId, senderId, receiverId, sentAt, messageType)
                OUTPUT INSERTED.mid
                VALUES (@conversationId, @senderId, @receiverId, @sentAt, @messageType);";

                var command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@conversationId", message.ConversationId);
                command.Parameters.AddWithValue("@senderId", message.MessageSenderId);
                command.Parameters.AddWithValue("@receiverId", message.MessageReceiverId);
                command.Parameters.AddWithValue("@sentAt", message.MessageSentTime);
                command.Parameters.AddWithValue("@messageType", MessageTypeExtensions.MessageTypeToString(message.TypeOfMessage));
                connection.Open();
                int newId = (int)command.ExecuteScalar();

                switch (message.TypeOfMessage)
                {
                    case MessageType.MessageText:
                        string textInsert = "INSERT INTO TextMessage (mid, content) VALUES (@mid, @content)";
                        var textCommand = new SqlCommand(textInsert, connection);
                        textCommand.Parameters.AddWithValue("@mid", newId);
                        textCommand.Parameters.AddWithValue("@content", ((TextMessage)message).TextMessageContent);
                        textCommand.ExecuteNonQuery();
                        break;
                    case MessageType.MessageImage:
                        string imageInsert = "INSERT INTO ImageMessage (mid, content) VALUES (@mid, @content)";
                        var imageCommand = new SqlCommand(imageInsert, connection);
                        imageCommand.Parameters.AddWithValue("@mid", newId);
                        imageCommand.Parameters.AddWithValue("@content", ((ImageMessage)message).MessageImageUrl);
                        imageCommand.ExecuteNonQuery();
                        break;
                    case MessageType.MessageCashAgreement:
                        string cashInsert = "INSERT INTO CashAgreementMessage (mid, content, sellerId, buyerId, acceptedBySeller, acceptedByBuyer, PaymentId) VALUES (@mid, @content, @sellerId, @buyerId, @acceptedBySeller, @acceptedByBuyer, @paymentId)";
                        var cashCommand = new SqlCommand(cashInsert, connection);
                        cashCommand.Parameters.AddWithValue("@mid", newId);
                        cashCommand.Parameters.AddWithValue("@content", ((CashAgreementMessage)message).MessageContentAsString);
                        cashCommand.Parameters.AddWithValue("@sellerId", ((CashAgreementMessage)message).MessageSenderId);
                        cashCommand.Parameters.AddWithValue("@buyerId", ((CashAgreementMessage)message).MessageReceiverId);
                        cashCommand.Parameters.AddWithValue("@acceptedBySeller", ((CashAgreementMessage)message).IsCashAgreementAcceptedBySeller);
                        cashCommand.Parameters.AddWithValue("@acceptedByBuyer", ((CashAgreementMessage)message).IsCashAgreementAcceptedByBuyer);
                        cashCommand.Parameters.AddWithValue("@paymentId", ((CashAgreementMessage)message).CashPaymentId);
                        cashCommand.ExecuteNonQuery();
                        break;
                    case MessageType.MessageRentalRequest:
                        string rentalInsert = "INSERT INTO RentalRequestMessage (mid, content, requestId, isResolved, isAccepted) VALUES (@mid, @content, @requestId, @isResolved, @isAccepted)";
                        var rentalCommand = new SqlCommand(rentalInsert, connection);
                        rentalCommand.Parameters.AddWithValue("@mid", newId);
                        rentalCommand.Parameters.AddWithValue("@content", ((RentalRequestMessage)message).MessageContentAsString);
                        rentalCommand.Parameters.AddWithValue("@requestId", ((RentalRequestMessage)message).RentalRequestId);
                        rentalCommand.Parameters.AddWithValue("@isResolved", ((RentalRequestMessage)message).IsRequestResolved);
                        rentalCommand.Parameters.AddWithValue("@isAccepted", ((RentalRequestMessage)message).IsRequestAccepted);
                        rentalCommand.ExecuteNonQuery();
                        break;
                    case MessageType.MessageSystem:
                        string systemInsert = "INSERT INTO SystemMessage (mid, content) VALUES (@mid, @content)";
                        var systemCommand = new SqlCommand(systemInsert, connection);
                        systemCommand.Parameters.AddWithValue("@mid", newId);
                        systemCommand.Parameters.AddWithValue("@content", ((SystemMessage)message).MessageContentAsString);
                        systemCommand.ExecuteNonQuery();
                        break;
                }
                connection.Close();
                return newId;
            }
        }

        private void UpdateMessageToDB(Message message)
        {
            using (var connection = new SqlConnection(appConnectionString))
            {
                string query = @"
                UPDATE Message
                SET senderId = @senderId, receiverId = @receiverId, sentAt = @sentAt, messageType = @messageType
                WHERE mid = @mid;";
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@mid", message.MessageId);
                command.Parameters.AddWithValue("@senderId", message.MessageSenderId);
                command.Parameters.AddWithValue("@receiverId", message.MessageReceiverId);
                command.Parameters.AddWithValue("@sentAt", message.MessageSentTime);
                command.Parameters.AddWithValue("@messageType", MessageTypeExtensions.MessageTypeToString(message.TypeOfMessage));
                connection.Open();
                command.ExecuteNonQuery();

                if (message.TypeOfMessage == MessageType.MessageCashAgreement)
                {
                    string cashUpdate = "UPDATE CashAgreementMessage SET acceptedBySeller = @acceptedBySeller, acceptedByBuyer = @acceptedByBuyer, PaymentId = @paymentId WHERE mid = @mid";
                    var cashCommand = new SqlCommand(cashUpdate, connection);
                    cashCommand.Parameters.AddWithValue("@mid", message.MessageId);
                    cashCommand.Parameters.AddWithValue("@acceptedBySeller", ((CashAgreementMessage)message).IsCashAgreementAcceptedBySeller);
                    cashCommand.Parameters.AddWithValue("@acceptedByBuyer", ((CashAgreementMessage)message).IsCashAgreementAcceptedByBuyer);
                    cashCommand.Parameters.AddWithValue("@paymentId", ((CashAgreementMessage)message).CashPaymentId);
                    cashCommand.ExecuteNonQuery();
                }
                else if (message.TypeOfMessage == MessageType.MessageRentalRequest)
                {
                    string rentalUpdate = "UPDATE RentalRequestMessage SET isResolved = @isResolved, isAccepted = @isAccepted WHERE mid = @mid";
                    var rentalCommand = new SqlCommand(rentalUpdate, connection);
                    rentalCommand.Parameters.AddWithValue("@mid", message.MessageId);
                    rentalCommand.Parameters.AddWithValue("@isResolved", ((RentalRequestMessage)message).IsRequestResolved);
                    rentalCommand.Parameters.AddWithValue("@isAccepted", ((RentalRequestMessage)message).IsRequestAccepted);
                    rentalCommand.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        private Conversation CreateConversationInDB(int senderId, int receiverId)
        {
            using (var connection = new SqlConnection(appConnectionString))
            {
                string query = @"
                SELECT TOP 1 cid
                FROM ConversationUser
                WHERE uid IN (@user1, @user2)
                GROUP BY cid
                HAVING COUNT(DISTINCT uid) = 2;";
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@user1", senderId);
                command.Parameters.AddWithValue("@user2", receiverId);

                connection.Open();
                object result = command.ExecuteScalar();

                if (result != null)
                {
                    return LoadSingleConversationFromDB((int)result, connection);
                }

                string insertConversationQuery = @"INSERT INTO Conversation
                                               OUTPUT INSERTED.cid
                                               DEFAULT VALUES;";
                var insertCommand = new SqlCommand(insertConversationQuery, connection);
                int newConversationId = (int)insertCommand.ExecuteScalar();

                var insertConversationUsersQuery = @"INSERT INTO ConversationUser (cid, uid, LastRead) VALUES (@cid, @uid, @lastRead)";
                var insertUserCommand = new SqlCommand(insertConversationUsersQuery, connection);
                insertUserCommand.Parameters.AddWithValue("@cid", newConversationId);
                insertUserCommand.Parameters.AddWithValue("@uid", senderId);
                insertUserCommand.Parameters.AddWithValue("@lastRead", DateTime.Now);
                insertUserCommand.ExecuteNonQuery();

                insertUserCommand.Parameters["@uid"].Value = receiverId;
                insertUserCommand.Parameters["@lastRead"].Value = DateTime.Now;
                insertUserCommand.ExecuteNonQuery();

                connection.Close();
                return new Conversation(newConversationId, new int[] { senderId, receiverId }, new List<Message>(), new Dictionary<int, DateTime> { { senderId, DateTime.Now }, { receiverId, DateTime.Now } });
            }
        }

        private void UpdateLastReadInDB(ReadReceipt readReceipt)
        {
            using (var connection = new SqlConnection(appConnectionString))
            {
                string query = @"UPDATE ConversationUser SET LastRead = @lastRead WHERE cid = @cid AND uid = @uid";
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@lastRead", readReceipt.timeStamp);
                command.Parameters.AddWithValue("@cid", readReceipt.conversationId);
                command.Parameters.AddWithValue("@uid", readReceipt.messageReaderId);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public void CreateCashAgreementMessage(int messageIdOfParentRentalRequestMessage, int paymentId)
        {
            try
            {
                int unassignedMessageIdentifier = -1;
                RentalRequestMessage parentMessage = (RentalRequestMessage)GetMessageById(messageIdOfParentRentalRequestMessage);
                CashAgreementMessage cashAgreementMessage = new CashAgreementMessage(
                    unassignedMessageIdentifier,
                    parentMessage.ConversationId,
                    parentMessage.MessageReceiverId,
                    parentMessage.MessageSenderId,
                    paymentId,
                    DateTime.Now,
                    $"Cash agreement for request: {parentMessage.RentalRequestId}");
                HandleNewMessage(cashAgreementMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"You tried to create a cash agreement message for a message that wasnt a rental request... how? {ex.Message}");
            }
        }

        public void CreateSystemMessageForCashAgreementFinalization(int conversationId, string legalDocumentFilePath)
        {
            int unassignedMessageIdentifier = -1;
            SystemMessage systemMessage = new SystemMessage(
                unassignedMessageIdentifier,
                conversationId,
                DateTime.Now,
                $"The cash agreement has been finalized! Here is your receipt: {legalDocumentFilePath}");
            HandleNewMessage(systemMessage);
        }

        private void UpdateCashPaymentFromMessageUpdate(CashAgreementMessage message)
        {
            int paymentId = message.CashPaymentId;
            if (!App.CashPaymentService.IsDeliveryConfirmed(paymentId) && message.IsCashAgreementAcceptedByBuyer)
            {
                App.CashPaymentService.ConfirmDelivery(paymentId);
            }
            if (!App.CashPaymentService.IsPaymentConfirmed(paymentId) && message.IsCashAgreementAcceptedBySeller)
            {
                App.CashPaymentService.ConfirmPayment(paymentId);
            }
            if (App.CashPaymentService.IsAllConfirmed(paymentId))
            {
                CreateSystemMessageForCashAgreementFinalization(message.ConversationId, App.CashPaymentService.GetReceipt(paymentId));
            }
        }

        #endregion

        #region ObserverStuff

        public void Subscribe(int userId, IConversationService observer)
        {
            if (!Subscribers.ContainsKey(userId))
            {
                Subscribers.Add(userId, observer);
            }
        }

        public void Unsubscribe(int userId)
        {
            if (Subscribers.ContainsKey(userId))
            {
                Subscribers.Remove(userId);
            }
        }

        public void NotifySubscribersAboutMessage(Message message)
        {
            int[] participants;
            if (message.TypeOfMessage == MessageType.MessageSystem)
            {
                participants = GetConversationParticipants(message.ConversationId);
            }
            else
            {
                participants = new[] { message.MessageSenderId, message.MessageReceiverId };
            }
            foreach (var participant in participants)
            {
                if (Subscribers.ContainsKey(participant))
                {
                    Subscribers[participant].OnMessageReceived(message);
                }
            }
        }

        public void NotifySubscribersAboutMessageUpdate(Message message)
        {
            var participants = new[] { message.MessageSenderId, message.MessageReceiverId };
            foreach (var participant in participants)
            {
                if (Subscribers.ContainsKey(participant))
                {
                    Subscribers[participant].OnMessageUpdateReceived(message);
                }
            }
        }

        public void NotifySubscribersAboutNewConversation(Conversation conversation)
        {
            foreach (var participant in conversation.ConversationParticipantIds)
            {
                if (Subscribers.ContainsKey(participant))
                {
                    Subscribers[participant].OnConversationReceived(conversation);
                }
            }
        }

        public void NotifySubscribersAboutReadReceipt(ReadReceipt readReceipt)
        {
            var participants = new[] { readReceipt.messageReaderId, readReceipt.messageReceiverId };
            foreach (var participant in participants)
            {
                if (Subscribers.ContainsKey(participant))
                {
                    Subscribers[participant].OnReadReceiptReceived(readReceipt);
                }
            }
        }
        #endregion
    }
}