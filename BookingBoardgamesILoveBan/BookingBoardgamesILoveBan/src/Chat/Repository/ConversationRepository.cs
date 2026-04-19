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

        public ConversationRepository()
        {
            Subscribers = new Dictionary<int, IConversationService>();
        }

        #region Public Methods
        // GETTERS

        /// <summary>
        /// Gets all conversations for a user, including messages and last read info.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<Conversation> GetConversationsForUser(int userId)
        {
            return LoadConversationsForUserFromDB(userId);
        }

        /// <summary>
        /// Gets a single conversation by id, including messages and last read info.
        /// </summary>
        /// <param name="convId"></param>
        /// <returns></returns>
        public Conversation GetConversationById(int convId)
        {
            return LoadConversationFromDB(convId);
        }

        // CRUD HANDLERS

        /// <summary>
        /// Handles the creation of a new message, including database insertion and notifying subscribers.
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="InvalidOperationException"></exception>
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

        /// <summary>
        /// Handles a read receipt, updating the last read timestamp in the database and notifying subscribers.
        /// </summary>
        /// <param name="readReceipt"></param>
        public void HandleReadReceipt(ReadReceipt readReceipt)
        {
            UpdateLastReadInDB(readReceipt);
            NotifySubscribersAboutReadReceipt(readReceipt);
        }

        /// <summary>
        /// Handles updates to a message, such as accepting a cash agreement or finalizing a rental request.
        /// Updates the database and notifies subscribers.
        /// </summary>
        /// <param name="message"></param>
        public void HandleMessageUpdate(Message message)
        {
            if (message.TypeOfMessage == MessageType.MessageCashAgreement)
            {
                UpdateCashPaymentFromMessageUpdate((CashAgreementMessage)message);
            }
            UpdateMessageToDB(message);
            NotifySubscribersAboutMessageUpdate(message);
        }

        /// <summary>
        /// Handles the creation of a new conversation between two users. If a conversation already exists, it returns the existing conversation's id.
        /// If not, it creates a new conversation in the database, adds a welcome system message, and notifies subscribers.
        /// </summary>
        /// <param name="senderId"></param>
        /// <param name="receiverId"></param>
        /// <returns></returns>
        public int CreateConversation(int senderId, int receiverId)
        {
            Conversation newConversation = CreateConversationInDB(senderId, receiverId);
            if (newConversation.ConversationMessageList.Count == 0)
            {
                // Create welcome system message
                NotifySubscribersAboutNewConversation(newConversation);
                HandleNewMessage(new SystemMessage(-1, newConversation.ConversationId, DateTime.Now, "New conversation"));
                NotifySubscribersAboutNewConversation(newConversation);
            }
            return newConversation.ConversationId;
        }

        /// <summary>
        /// Handles the finalization of a rental request, marking it as resolved in the database and notifying subscribers.
        /// </summary>
        /// <param name="messageId"></param>
        public void HandleRentalRequestFinalization(int messageId)
        {
            try
            {
                RentalRequestMessage message = (RentalRequestMessage)GetMessageById(messageId);
                message.IsRequestResolved = true;
                message.RequestContent += "\n\nThis request has been finalized!";
                // message.Content = "This request has been finalized!";
                HandleMessageUpdate(message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"You tried to accept a message that wasnt a rental request... how?");
            }
        }

        #endregion

        #region Database Communication
        private static string appConnectionString = DatabaseBootstrap.GetAppConnection();

        // GETTERS

        /// <summary>
        /// Gets a single message by id, including all relevant info for that message type
        /// (e.g. cash agreement details, rental request details).
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
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

                    int senderId = reader.GetInt32(1);
                    int receiverId = reader.GetInt32(2);
                    DateTime timestamp = reader.GetDateTime(3);
                    string messageType = reader.GetString(4);
                    int conversationId = reader.GetInt32(5);

                    switch (messageType)
                    {
                        case "TEXT":
                            return new TextMessage(messageId, conversationId, senderId, receiverId, timestamp, reader.GetString(6));

                        case "IMAGE":
                            return new ImageMessage(messageId, conversationId, senderId, receiverId, timestamp, reader.GetString(7));

                        case "CASH_AGREEMENT":
                            bool isAcceptedByBuyer = reader.IsDBNull(13) ? false : reader.GetBoolean(13);
                            bool isAcceptedBySeller = reader.IsDBNull(12) ? false : reader.GetBoolean(12);
                            int paymentId = reader.IsDBNull(18) ? -1 : reader.GetInt32(18);
                            return new CashAgreementMessage(
                                messageId, conversationId,
                                reader.IsDBNull(10) ? -1 : reader.GetInt32(10),
                                reader.IsDBNull(11) ? -1 : reader.GetInt32(11),
                                paymentId,
                                timestamp,
                                (string)reader[8],
                                isAcceptedByBuyer && isAcceptedBySeller, // this is useless!
                                isAcceptedByBuyer,
                                isAcceptedBySeller);

                        case "RENTAL_REQUEST":
                            return new RentalRequestMessage(
                                messageId, conversationId, senderId, receiverId, timestamp,
                                (string)reader[9],
                                reader.IsDBNull(14) ? -1 : reader.GetInt32(14),
                                reader.IsDBNull(15) ? false : reader.GetBoolean(15),
                                reader.IsDBNull(16) ? false : reader.GetBoolean(16));

                        case "SYSTEM":
                            return new SystemMessage(messageId, conversationId, timestamp, reader.GetString(17));

                        default:
                            return null;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the participant user ids for a given conversation id. This is used for notifying the
        /// correct subscribers when a system message is sent, since system messages have sender and
        /// receiver id of 0 and thus we cant get the participants from the message itself.
        /// </summary>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        private int[] GetConversationParticipants(int conversationId)
        {
            var participantIds = new List<int>();
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
                        participantIds.Add(reader.GetInt32(0));
                    }
                }
                connection.Close();
            }
            return participantIds.ToArray();
        }

        /// <summary>
        /// Gets all conversations for a user, including messages and last read info.
        /// This is done by first querying the ConversationUser table
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private List<Conversation> LoadConversationsForUserFromDB(int userId)
        {
            var userConversations = new List<Conversation>();
            using (var connection = new SqlConnection(appConnectionString))
            {
                string joinQuery = @"
                SELECT C.cid 
                FROM [Conversation] C
                INNER JOIN [ConversationUser] CU ON C.cid = CU.cid
                WHERE CU.uid = @uid
            ";
                var command = new SqlCommand(joinQuery, connection);
                command.Parameters.AddWithValue("@uid", userId);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int conversationId = reader.GetInt32(0);

                        var conversation = LoadSingleConversationFromDB(conversationId, connection);
                        userConversations.Add(conversation);
                    }
                }
                connection.Close();
            }

            return userConversations;
        }

        /// <summary>
        /// Gets a single conversation by id, including messages and last read info.
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        private Conversation LoadSingleConversationFromDB(int conversationId, SqlConnection connection)
        {
            List<int> participantIds = new List<int>();
            Dictionary<int, DateTime> lastRead = new Dictionary<int, DateTime>();
            string participantsSql = "SELECT uid, LastRead FROM [ConversationUser] WHERE cid = @cid";

            using (var command = new SqlCommand(participantsSql, connection))
            {
                command.Parameters.AddWithValue("@cid", conversationId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int userId = reader.GetInt32(0);

                        DateTime lastReadByUser = reader.IsDBNull(1) ? DateTime.MinValue : reader.GetDateTime(1);
                        lastRead.Add(userId, lastReadByUser);

                        participantIds.Add(userId);
                    }
                }
            }

            List<Message> messages = LoadMessagesForConversationFromDB(conversationId);
            return new Conversation(conversationId, participantIds.ToArray(), messages, lastRead);
        }

        /// <summary>
        /// Gets all messages for a conversation, including all relevant info for each message type
        /// </summary>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        private Conversation LoadConversationFromDB(int conversationId)
        {
            Conversation ret = null;
            using (var connection = new SqlConnection(appConnectionString))
            {
                connection.Open();
                ret = LoadSingleConversationFromDB(conversationId, connection);
                connection.Close();
            }
            return ret;
        }

        /// <summary>
        /// Gets all messages for a conversation, including all relevant info for each message type.
        /// </summary>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        private List<Message> LoadMessagesForConversationFromDB(int conversationId)
        {
            // now i know why big man said to keep the db simple
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
                    while (reader.Read())
                    {
                        int messageId = reader.GetInt32(0);
                        int senderId = reader.GetInt32(1);
                        int receiverId = reader.GetInt32(2);
                        DateTime timestamp = reader.GetDateTime(3);
                        string messageType = reader.GetString(4);
                        switch (messageType)
                        {
                            case "TEXT":
                                string textContent = reader.GetString(5);
                                messages.Add(new TextMessage(messageId, conversationId, senderId, receiverId, timestamp, textContent));
                                break;
                            case "IMAGE":
                                var imageContent = reader.GetString(6);
                                messages.Add(new ImageMessage(messageId, conversationId, senderId, receiverId, timestamp, imageContent));
                                break;

                            case "CASH_AGREEMENT":
                                string cashContent = (string)reader[7];
                                int sellerId = reader.IsDBNull(9) ? -1 : reader.GetInt32(9);
                                int buyerId = reader.IsDBNull(10) ? -1 : reader.GetInt32(10);
                                bool acceptedBySeller = reader.IsDBNull(11) ? false : reader.GetBoolean(11);
                                bool acceptedByBuyer = reader.IsDBNull(12) ? false : reader.GetBoolean(12);
                                int paymentId = reader.IsDBNull(17) ? -1 : reader.GetInt32(17);
                                messages.Add(new CashAgreementMessage(
                                    messageId,
                                    conversationId,
                                    sellerId,
                                    buyerId,
                                    paymentId,
                                    timestamp,
                                    cashContent,
                                    false, // or the correct isResolved value if you have it
                                    acceptedByBuyer,
                                    acceptedBySeller));
                                break;

                            case "RENTAL_REQUEST":
                                string rentalContent = (string)reader[8];
                                int requestId = reader.IsDBNull(13) ? -1 : reader.GetInt32(13);
                                bool isResolved = reader.IsDBNull(14) ? false : reader.GetBoolean(14);
                                bool isAccepted = reader.IsDBNull(15) ? false : reader.GetBoolean(15);
                                messages.Add(new RentalRequestMessage(messageId, conversationId, senderId, receiverId, timestamp, rentalContent, requestId, isResolved, isAccepted));
                                break;

                            case "SYSTEM":
                                string systemContent = reader.GetString(16);
                                messages.Add(new SystemMessage(messageId, conversationId, timestamp, systemContent));
                                break;
                        }
                    }
                }
                connection.Close();
            }
            return messages;
        }
        // CRUD

        /// <summary>
        /// Handles the insertion of a new message into the database, including inserting into the base Message table and the relevant subtype table.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private int AddMessageToDB(Message message)
        {
            // messages created and not yet added to the database will have a dummy id (-1)
            // after a message is added to the db, its id will change to whatever the db provides
            // the id needs to be updated upwards
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
                        cashCommand.Parameters.AddWithValue("@sellerId", ((CashAgreementMessage)message).MessageSenderId); // Assuming sender is the seller
                        cashCommand.Parameters.AddWithValue("@buyerId", ((CashAgreementMessage)message).MessageReceiverId); // Assuming receiver is the buyer
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

        /// <summary>
        /// Handles the update of a message in the database.
        /// This is used for things like accepting a cash agreement or finalizing a rental request, where we
        /// want to keep the same message but just update some of its fields.
        /// </summary>
        /// <param name="message"></param>
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

        /// <summary>
        /// Handles the creation of a new conversation between two users. If a conversation already exists, it returns the existing conversation.
        /// </summary>
        /// <param name="senderId"></param>
        /// <param name="receiverId"></param>
        /// <returns></returns>
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
                Debug.WriteLine("\n\n\n\n\n\n" + result + "\n\n\n\n\n\n\n");
                if (result != null)
                {
                    return LoadSingleConversationFromDB((int)result, connection);
                }

                string insertConversationQuery = @"INSERT INTO Conversation
                                               OUTPUT INSERTED.cid
                                               DEFAULT VALUES;";
                var insertCommand = new SqlCommand(insertConversationQuery, connection);
                int newConversationId = (int)insertCommand.ExecuteScalar();
                insertCommand.ExecuteScalar();

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
        // READ RECEIPTS

        /// <summary>
        /// Handles the update of the last read timestamp for a user in a conversation when a read receipt is received.
        /// </summary>
        /// <param name="readReceipt"></param>
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

        /// <summary>
        /// Handles the creation of a new cash agreement message in response to an accepted rental request.
        /// This includes creating the message, inserting it into the database, and notifying subscribers.
        /// </summary>
        /// <param name="messageIdOfParentRentalRequestMessage"></param>
        /// <param name="paymentId"></param>
        public void CreateCashAgreementMessage(int messageIdOfParentRentalRequestMessage, int paymentId)
        {
            try
            {
                RentalRequestMessage parentMessage = (RentalRequestMessage)GetMessageById(messageIdOfParentRentalRequestMessage);
                CashAgreementMessage cashAgreementMessage = new CashAgreementMessage(
                    -1,
                    parentMessage.ConversationId,
                    parentMessage.MessageReceiverId, // seller is the one who receives the rental request
                    parentMessage.MessageSenderId,   // buyer is the one who sent the rental request
                    paymentId,
                    DateTime.Now,
                    // we might want different content?
                    $"Cash agreement for request: {parentMessage.RentalRequestId}");
                HandleNewMessage(cashAgreementMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"You tried to create a cash agreement message for a message that wasnt a rental request... how?");
            }
        }

        /// <summary>
        /// Handles the creation of a new system message announcing the finalization of a cash agreement,
        /// including inserting it into the database and notifying subscribers.
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="legalDocumentFilePath"></param>
        public void CreateSystemMessageForCashAgreementFinalization(int conversationId, string legalDocumentFilePath)
        {
            SystemMessage systemMessage = new SystemMessage(
                -1,
                conversationId,
                DateTime.Now,
                $"The cash agreement has been finalized! Here is your receipt: {legalDocumentFilePath}");
            HandleNewMessage(systemMessage);
        }

        /// <summary>
        /// Handles the update of a cash agreement message when either the buyer or seller accepts the agreement.
        /// </summary>
        /// <param name="message"></param>
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

        /// <summary>
        /// Subscribes a user to message updates. This means that whenever a new message is sent in a
        /// conversation that the user is a participant of, or a message that the user sent or received
        /// is updated, the user's MessageObserver will be notified with the new message info.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="observer"></param>
        public void Subscribe(int userId, IConversationService observer)
        {
            if (!Subscribers.ContainsKey(userId))
            {
                Subscribers.Add(userId, observer);
            }
        }

        /// <summary>
        /// Unsubscribes a user from message updates.
        /// This is used when a user logs out, so that they no longer receive updates about messages.
        /// </summary>
        /// <param name="userId"></param>
        public void Unsubscribe(int userId)
        {
            if (Subscribers.ContainsKey(userId))
            {
                Subscribers.Remove(userId);
            }
        }

        /// <summary>
        /// Notifies all relevant subscribers about a new message. This includes subscribers for both the sender and receiver of the message,
        /// </summary>
        /// <param name="message"></param>
        public void NotifySubscribersAboutMessage(Message message)
        {
            int[] participants;
            if (message.TypeOfMessage == MessageType.MessageSystem)
            {// because system messages are "sent and received by user 0" we need to hit the db :/
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

        /// <summary>
        /// Notifies all relevant subscribers about an updated message. This includes subscribers for both the sender and receiver of the message,
        /// </summary>
        /// <param name="message"></param>
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

        /// <summary>
        /// Notifies all relevant subscribers about a new conversation.
        /// This is used to trigger the creation of a new chat tab in the UI when a new conversation is created.
        /// </summary>
        /// <param name="conversation"></param>
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

        /// <summary>
        /// Notifies all relevant subscribers about a read receipt, so that they can update the UI
        /// to reflect the new last read timestamp for the user who sent the read receipt.
        /// </summary>
        /// <param name="readReceipt"></param>
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
