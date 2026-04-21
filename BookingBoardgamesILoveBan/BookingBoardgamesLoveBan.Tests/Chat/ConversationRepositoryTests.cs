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

        public ConversationRepositoryIntegrationTests()
        {
            DatabaseBootstrap.Initialize();
            connectionString = DatabaseBootstrap.GetAppConnection();
        }

        [Fact]
        public void HandleNewMessage_ImageMessage_Persists()
        {
            var repository = new ConversationRepository();

            int conversationId = repository.CreateConversation(1, 2);

            var message = new ImageMessage(-1, conversationId, 1, 2, DateTime.Now, "img.png");

            repository.HandleNewMessage(message);

            var conversation = repository.GetConversationById(conversationId);

            Assert.Contains(conversation.ConversationMessageList,
                message => message is ImageMessage);

            CleanupConversation(conversationId);
        }

        [Fact]
        public void HandleMessageUpdate_RentalRequest_UpdatesDB()
        {
            var repository = new ConversationRepository();

            int conversationId = repository.CreateConversation(1, 2);

            var message = new RentalRequestMessage(
                -1, conversationId, 1, 2, DateTime.Now,
                "rent", 1, false, false);

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
        public void CashAgreement_CreatesSystemMessage()
        {
            var repository = new ConversationRepository();

            int conversationId = repository.CreateConversation(1, 2);

            var message = new CashAgreementMessage(
                -1, conversationId, 1, 2, 1,
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
        public void GetConversationsForUser_ReturnsData()
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

                Assert.Contains(list, c => c.ConversationId == conversationersationId);
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
                    catch (Exception ex)
                    {
                    }
                }
            }
        }

        [Fact]
        public void Observer_ReceivesMessageNotification()
        {
            var repository = new ConversationRepository();

            var mock = new Mock<IConversationService>();
            repository.Subscribe(1, mock.Object);

            int conversationId = repository.CreateConversation(1, 2);

            repository.HandleNewMessage(
                new TextMessage(-1, conversationId, 1, 2, DateTime.Now, "hi"));

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
        public void HandleNewMessage_ValidConversation_AddsMessage()
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
        public void HandleReadReceipt_UpdatesDatabase()
        {
            var repository = new ConversationRepository();

            int conversationId = repository.CreateConversation(1, 2);

            repository.HandleReadReceipt(new ReadReceipt(conversationId, 1, 2, DateTime.UtcNow));

            var conversation = repository.GetConversationById(conversationId);

            Assert.True(conversation.LastMessageReadTime.Count > 0);
        }

        [Fact]
        public void HandleRentalRequestFinalization_InvalidMessage_DoesNotCrash()
        {
            var repository = new ConversationRepository();

            repository.HandleRentalRequestFinalization(-999);
        }

        [Fact]
        public void SubscribeAndNotify_Works()
        {
            var repository = new ConversationRepository();

            var mock = new Mock<IConversationService>();

            repository.Subscribe(1, mock.Object);

            int conversationId = repository.CreateConversation(1, 2);

            var message = new TextMessage(-1, conversationId, 1, 2, DateTime.Now, "hi");

            repository.HandleNewMessage(message);

            mock.Verify(x => x.OnMessageReceived(It.IsAny<Message>()), Times.Once);
        }

        [Fact]
        public void Unsubscribe_RemovesSubscriber()
        {
            var repository = new ConversationRepository();

            var mock = new Mock<IConversationService>();

            repository.Subscribe(1, mock.Object);
            repository.Unsubscribe(1);

            int conversationId = repository.CreateConversation(1, 2);

            repository.HandleNewMessage(
                new TextMessage(-1, conversationId, 1, 2, DateTime.Now, "x"));

            mock.Verify(x => x.OnMessageReceived(It.IsAny<Message>()), Times.Never);
        }

        [Fact]
        public void SystemMessage_IsProcessed()
        {
            var repository = new ConversationRepository();

            int conversationId = repository.CreateConversation(1, 2);

            repository.CreateSystemMessageForCashAgreementFinalization(conversationId, "file.pdf");

            var conversation = repository.GetConversationById(conversationId);

            Assert.Contains(conversation.ConversationMessageList,
                message => message.TypeOfMessage == MessageType.MessageSystem);
        }

        [Fact]
        public void GetMessageById_Invalid_NotReturnsNull()
        {
            var repository = new ConversationRepository();

            var result = repository.GetConversationById(-999);

            Assert.NotNull(result);
        }

        [Fact]
        public void CreateConversation_ReusesExistingConversation()
        {
            var repository = new ConversationRepository();

            int id1 = repository.CreateConversation(1, 2);
            int id2 = repository.CreateConversation(1, 2);

            Assert.Equal(id1, id2);
        }

        [Fact]
        public void SystemMessage_UsesParticipantLookupBranch()
        {
            var repository = new ConversationRepository();

            var mock = new Mock<IConversationService>();

            repository.Subscribe(1, mock.Object);
            repository.Subscribe(2, mock.Object);

            int conversationId = repository.CreateConversation(1, 2);

            repository.CreateSystemMessageForCashAgreementFinalization(conversationId, "file.pdf");

            mock.Verify(x => x.OnMessageReceived(It.IsAny<Message>()), Times.AtLeastOnce);

            CleanupConversation(conversationId);
        }

        [Fact]
        public void Unsubscribe_NonExistingUser_DoesNotThrow()
        {
            var repository = new ConversationRepository();
            repository.Unsubscribe(99999);
        }

        [Fact]
        public void CashPaymentUpdate_NoConditionsMet_DoesNothing()
        {
            var repository = new ConversationRepository();

            int conversationId = repository.CreateConversation(1, 2);

            var message = new CashAgreementMessage(
                -1, conversationId, 1, 2, 1,
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