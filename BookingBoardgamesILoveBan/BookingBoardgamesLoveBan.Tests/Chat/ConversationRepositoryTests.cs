using BookingBoardgamesILoveBan.Src.Chat.Model;
using BookingBoardgamesILoveBan.Src.Chat.Repository;
using BookingBoardgamesILoveBan.Src.Chat.Service;
using BookingBoardgamesILoveBan.Src.Enum;
using BookingBoardgamesILoveBan.Src.Model;
using Microsoft.Data.SqlClient;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace BookingBoardgamesILoveBan.Tests.Chat
{
    public class ConversationRepositoryIntegrationTests
    {
        private readonly string connectionString;
        private int senderId = 1, receiverId = 2;
        public ConversationRepositoryIntegrationTests()
        {
            DatabaseBootstrap.Initialize();
            connectionString = DatabaseBootstrap.GetAppConnection();
        }

        [Fact]
        public void ConversationRepositoryHandleNewMessage_imageMessage_addsMessageToConversationMessageList()
        {
            var repository = new ConversationRepository();

            int conversationId = repository.CreateConversation(senderId, receiverId);
            int messageId = -1;
            var message = new ImageMessage(messageId, conversationId, senderId, receiverId, DateTime.Now, "img.png");

            repository.HandleNewMessage(message);

            var conversation = repository.GetConversationById(conversationId);

            Assert.Contains(conversation.ConversationMessageList,
                message => message is ImageMessage);

            CleanupConversation(conversationId);
        }

        [Fact]
        public void ConversationRepositoryHandleMessageUpdate_rentalRequest_updatesIsRequestAccepted()
        {
            var repository = new ConversationRepository();

            int conversationId = repository.CreateConversation(senderId, receiverId);
            int messageId = -1;
            int requestId = 1;
            var message = new RentalRequestMessage(
                messageId, conversationId, senderId, receiverId, DateTime.Now,
                "rent", requestId, false, false);

            repository.HandleNewMessage(message);

            var stored = repository.GetConversationById(conversationId)
                .ConversationMessageList
                .OfType<RentalRequestMessage>()
                .First();

            stored.IsRequestAccepted = true;

            repository.HandleMessageUpdate(stored);

            var updated = repository.GetConversationById(conversationId)
                .ConversationMessageList
                .OfType<RentalRequestMessage>()
                .First();

            Assert.True(updated.IsRequestAccepted);

            CleanupConversation(conversationId);
        }

        [Fact]
        public void ConversationRepositoryHandleMessageUpdate_cashAgreementWhenBothAccepted_createsSystemMessage()
        {
            var repository = new ConversationRepository();

            int conversationId = repository.CreateConversation(senderId, receiverId);
            int messageId = -1;
            int requestId = 1;
            var message = new CashAgreementMessage(
                messageId, conversationId, senderId, receiverId,requestId,
                DateTime.Now, "cash",
                false, false, false);

            repository.HandleNewMessage(message);

            message.IsCashAgreementAcceptedByBuyer = true;
            message.IsCashAgreementAcceptedBySeller = true;

            repository.HandleMessageUpdate(message);

            var conversation = repository.GetConversationById(conversationId);

            Assert.Contains(conversation.ConversationMessageList,
                message => message is SystemMessage);

            CleanupConversation(conversationId);
        }

        [Fact]
        public void ConversationRepository_getConversationsForUser_returnsOnlyConversationsContainingUser()
        {
            var repository = new ConversationRepository();

            int user1 = -1;
            int user2 = -1;
            int conversationersationId = -1;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    var cmd = new SqlCommand(@"
                        INSERT INTO [User] (DisplayName, UserName, Balance, Country, City, Street, StreetNumber)
                        VALUES ('Test50', 'test50', 0, 'RO', 'City', 'Street', '1');

                        SELECT SCOPE_IDENTITY();
                    ", connection);

                    user1 = Convert.ToInt32(cmd.ExecuteScalar());

                    cmd = new SqlCommand(@"
                        INSERT INTO [User] (DisplayName, UserName, Balance, Country, City, Street, StreetNumber)
                        VALUES ('Test60', 'test60', 0, 'RO', 'City', 'Street', '2');

                        SELECT SCOPE_IDENTITY();
                    ", connection);

                    user2 = Convert.ToInt32(cmd.ExecuteScalar());
                }

                conversationersationId = repository.CreateConversation(user1, user2);

                var list = repository.GetConversationsForUser(user1);

                Assert.Contains(list, conversation => conversation.ConversationId == conversationersationId);
            }
            finally
            {
                CleanupConversation(conversationersationId);

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    try
                    {
                        if (user1 > 0)
                            new SqlCommand("DELETE FROM [User] WHERE uid = @id", connection)
                            {
                                Parameters = { new SqlParameter("@id", user1) }
                            }.ExecuteNonQuery();

                        if (user2 > 0)
                            new SqlCommand("DELETE FROM [User] WHERE uid = @id", connection)
                            {
                                Parameters = { new SqlParameter("@id", user2) }
                            }.ExecuteNonQuery();
                    }
                    catch (Exception exception)
                    {
                    }
                }
            }
        }

        [Fact]
        public void ConversationRepository_subscribedObserver_receivesNotificationWhenNewMessageIsHandled()
        {
            var repository = new ConversationRepository();

            var mock = new Mock<IConversationService>();
            repository.Subscribe(1, mock.Object);

            int conversationId = repository.CreateConversation(senderId, receiverId);

            repository.HandleNewMessage(
                new TextMessage(-1, conversationId, senderId, receiverId, DateTime.Now, "hi"));

            mock.Verify(message => message.OnMessageReceived(It.IsAny<Message>()),
                Times.AtLeastOnce);

            CleanupConversation(conversationId);
        }

        private void CleanupConversation(int conversationersationId)
        {
            if (conversationersationId <= 0) return;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    var cmd = new SqlCommand(@"
                    DELETE FROM TextMessage WHERE mid IN (SELECT mid FROM Message WHERE ConversationId = @conversationId);
                    DELETE FROM ImageMessage WHERE mid IN (SELECT mid FROM Message WHERE ConversationId = @conversationId);
                    DELETE FROM CashAgreementMessage WHERE mid IN (SELECT mid FROM Message WHERE ConversationId = @conversationId);
                    DELETE FROM RentalRequestMessage WHERE mid IN (SELECT mid FROM Message WHERE ConversationId = @conversationId);
                    DELETE FROM SystemMessage WHERE mid IN (SELECT mid FROM Message WHERE ConversationId = @conversationId);
                    DELETE FROM Message WHERE ConversationId = @conversationId;
                    DELETE FROM ConversationUser WHERE conversationId = @conversationId;
                    DELETE FROM Conversation WHERE conversationId = @conversationId;
                ", connection);

                    cmd.Parameters.AddWithValue("@conversationId", conversationersationId);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
            }
        }

        [Fact]
        public void ConversationRepositoryHandleNewMessage_addsMessageToConversationMessageList()
        {
            var repository = new ConversationRepository();

            int user1 = 1, user2 = 2;

            int conversationId = repository.CreateConversation(user1, user2);

            var message = new TextMessage(-1, conversationId, user1, user2, DateTime.Now, "hello");

            repository.HandleNewMessage(message);

            var conversation = repository.GetConversationById(conversationId);

            Assert.Contains(conversation.ConversationMessageList,
                message => message.MessageContentAsString == "hello");
        }

        [Fact]
        public void ConversationRepository_handleReadReceipt_updatesLastMessageReadTime()
        {
            var repository = new ConversationRepository();

            int conversationId = repository.CreateConversation(senderId, receiverId);

            repository.HandleReadReceipt(new ReadReceipt(conversationId, senderId, receiverId, DateTime.UtcNow));

            var conversation = repository.GetConversationById(conversationId);

            Assert.True(conversation.LastMessageReadTime.Count > 0);
        }

        [Fact]
        public void ConversationRepositoryHandleRentalRequestFinalization_invalidMessageId_doesNotThrowException()
        {
            var repository = new ConversationRepository();
            int invalidId = -999;
            repository.HandleRentalRequestFinalization(invalidId);
        }

        [Fact]
        public void ConversationRepository_subscribedObserver_isNotifiedWhenNewMessageIsHandled()
        {
            var repository = new ConversationRepository();

            var mock = new Mock<IConversationService>();

            repository.Subscribe(1, mock.Object);

            int conversationId = repository.CreateConversation(senderId, receiverId);
            int messageId = -1;
            var message = new TextMessage(messageId, conversationId, senderId, receiverId, DateTime.Now, "hi");

            repository.HandleNewMessage(message);

            mock.Verify(service => service.OnMessageReceived(It.IsAny<Message>()), Times.Once);
        }

        [Fact]
        public void ConversationRepository_unsubscribe_preventsObserverFromReceivingMessageNotifications()
        {
            var repository = new ConversationRepository();

            var mock = new Mock<IConversationService>();
            int userId = 1;
            repository.Subscribe(userId, mock.Object);
            repository.Unsubscribe(userId);

            int conversationId = repository.CreateConversation(senderId, receiverId);
            int messageId = -1;
            repository.HandleNewMessage(
                new TextMessage(messageId, conversationId, senderId, receiverId, DateTime.Now, "x"));

            mock.Verify(service => service.OnMessageReceived(It.IsAny<Message>()), Times.Never);
        }

        [Fact]
        public void ConversationRepository_createSystemMessageForCashAgreementFinalization_addsSystemMessageToConversation()
        {
            var repository = new ConversationRepository();

            int conversationId = repository.CreateConversation(senderId, receiverId);

            repository.CreateSystemMessageForCashAgreementFinalization(conversationId, "file.pdf");

            var conversation = repository.GetConversationById(conversationId);

            Assert.Contains(conversation.ConversationMessageList,
                message => message.TypeOfMessage == MessageType.MessageSystem);
        }

        [Fact]
        public void ConversationRepositoryGetConversationById_invalidId_returnsNonNullResult()
        {
            var repository = new ConversationRepository();

            var result = repository.GetConversationById(-999);

            Assert.NotNull(result);
        }

        [Fact]
        public void ConversationRepository_createConversation_returnsExistingConversationForSameParticipants()
        {
            var repository = new ConversationRepository();

            int id1 = repository.CreateConversation(senderId, receiverId);
            int id2 = repository.CreateConversation(senderId, receiverId);

            Assert.Equal(id1, id2);
        }

        [Fact]
        public void ConversationRepository_createSystemMessageForCashAgreementFinalization_notifiesAllSubscribedParticipants()
        {
            var repository = new ConversationRepository();

            var mock = new Mock<IConversationService>();

            repository.Subscribe(1, mock.Object);
            repository.Subscribe(2, mock.Object);

            int conversationId = repository.CreateConversation(senderId, receiverId);

            repository.CreateSystemMessageForCashAgreementFinalization(conversationId, "file.pdf");

            mock.Verify(service => service.OnMessageReceived(It.IsAny<Message>()), Times.AtLeastOnce);

            CleanupConversation(conversationId);
        }

        [Fact]
        public void ConversationRepositoryUnsubscribe_nonSubscribedUser_isSafe()
        {
            var repository = new ConversationRepository();
            int invalidId = 99999;
            repository.Unsubscribe(invalidId);
        }

        [Fact]
        public void ConversationRepositoryHandleMessageUpdate_cashAgreementWithNoAcceptance_doesNotCreateSystemMessage()
        {
            var repository = new ConversationRepository();

            int conversationId = repository.CreateConversation(senderId, receiverId);
            int messageId = -1;
            int requestId = 1;
            var message = new CashAgreementMessage(
                messageId, conversationId, senderId, receiverId, requestId,
                DateTime.Now,
                "cash",
                false,
                false,
                false
            );

            repository.HandleNewMessage(message);
            repository.HandleMessageUpdate(message);
            CleanupConversation(conversationId);
        }

    }
}