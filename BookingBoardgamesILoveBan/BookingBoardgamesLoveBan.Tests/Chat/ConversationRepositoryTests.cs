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

        private int CreateTemporaryTestUser(string userSuffix)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                string insertQuery = @"
                    INSERT INTO [User] (DisplayName, UserName, Balance, Country, City, Street, StreetNumber)
                    VALUES (@DisplayName, @UserName, 0, 'RO', 'City', 'Street', '1');
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                using (SqlCommand sqlCommand = new SqlCommand(insertQuery, sqlConnection))
                {
                    sqlCommand.Parameters.AddWithValue("@DisplayName", "TestUser_" + userSuffix);
                    sqlCommand.Parameters.AddWithValue("@UserName", "usr_" + Guid.NewGuid().ToString().Substring(0, 8));
                    return (int)sqlCommand.ExecuteScalar();
                }
            }
        }

        private void CleanupTemporaryTestUser(int userIdentifier)
        {
            if (userIdentifier <= 0) return;
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    sqlConnection.Open();
                    using (SqlCommand sqlCommand = new SqlCommand("DELETE FROM [User] WHERE uid = @UserId", sqlConnection))
                    {
                        sqlCommand.Parameters.AddWithValue("@UserId", userIdentifier);
                        sqlCommand.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }

        private void CleanupConversation(int conversationIdentifier)
        {
            if (conversationIdentifier <= 0) return;
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    sqlConnection.Open();
                    SqlCommand sqlCommand = new SqlCommand(@"
                        DELETE FROM TextMessage WHERE mid IN (SELECT mid FROM Message WHERE ConversationId = @conversationId);
                        DELETE FROM ImageMessage WHERE mid IN (SELECT mid FROM Message WHERE ConversationId = @conversationId);
                        DELETE FROM CashAgreementMessage WHERE mid IN (SELECT mid FROM Message WHERE ConversationId = @conversationId);
                        DELETE FROM RentalRequestMessage WHERE mid IN (SELECT mid FROM Message WHERE ConversationId = @conversationId);
                        DELETE FROM SystemMessage WHERE mid IN (SELECT mid FROM Message WHERE ConversationId = @conversationId);
                        DELETE FROM Message WHERE ConversationId = @conversationId;
                        DELETE FROM ConversationUser WHERE cid = @conversationId;
                        DELETE FROM Conversation WHERE cid = @conversationId;
                    ", sqlConnection);

                    sqlCommand.Parameters.AddWithValue("@conversationId", conversationIdentifier);
                    sqlCommand.ExecuteNonQuery();
                }
            }
            catch { }
        }

        [Fact]
        public void HandleNewMessage_ValidImageMessage_PersistsToDatabase()
        {
            int firstUserIdentifier = -1;
            int secondUserIdentifier = -1;
            int conversationIdentifier = -1;
            int defaultMessageIdentifier = -1;
            string testImageFileName = "img.png";

            try
            {
                firstUserIdentifier = CreateTemporaryTestUser("A");
                secondUserIdentifier = CreateTemporaryTestUser("B");
                ConversationRepository conversationRepository = new ConversationRepository();

                conversationIdentifier = conversationRepository.CreateConversation(firstUserIdentifier, secondUserIdentifier);
                ImageMessage imageMessage = new ImageMessage(defaultMessageIdentifier, conversationIdentifier, firstUserIdentifier, secondUserIdentifier, DateTime.Now, testImageFileName);

                conversationRepository.HandleNewMessage(imageMessage);
                Conversation retrievedConversation = conversationRepository.GetConversationById(conversationIdentifier);

                Assert.Contains(retrievedConversation.ConversationMessageList, messageItem => messageItem is ImageMessage);
            }
            finally
            {
                CleanupConversation(conversationIdentifier);
                CleanupTemporaryTestUser(firstUserIdentifier);
                CleanupTemporaryTestUser(secondUserIdentifier);
            }
        }

        [Fact]
        public void HandleMessageUpdate_ValidRentalRequest_UpdatesDatabaseStatus()
        {
            int firstUserIdentifier = -1;
            int secondUserIdentifier = -1;
            int conversationIdentifier = -1;
            int defaultMessageIdentifier = -1;
            int rentalRequestIdentifier = 1;
            string rentalMessageContent = "rent";

            try
            {
                firstUserIdentifier = CreateTemporaryTestUser("A");
                secondUserIdentifier = CreateTemporaryTestUser("B");
                ConversationRepository conversationRepository = new ConversationRepository();

                conversationIdentifier = conversationRepository.CreateConversation(firstUserIdentifier, secondUserIdentifier);
                RentalRequestMessage rentalRequestMessage = new RentalRequestMessage(
                    defaultMessageIdentifier, conversationIdentifier, firstUserIdentifier, secondUserIdentifier,
                    DateTime.Now, rentalMessageContent, rentalRequestIdentifier, false, false);

                conversationRepository.HandleNewMessage(rentalRequestMessage);

                RentalRequestMessage storedMessage = conversationRepository.GetConversationById(conversationIdentifier)
                    .ConversationMessageList.OfType<RentalRequestMessage>().First();

                storedMessage.IsRequestAccepted = true;
                conversationRepository.HandleMessageUpdate(storedMessage);

                RentalRequestMessage updatedMessage = conversationRepository.GetConversationById(conversationIdentifier)
                    .ConversationMessageList.OfType<RentalRequestMessage>().First();

                Assert.True(updatedMessage.IsRequestAccepted);
            }
            finally
            {
                CleanupConversation(conversationIdentifier);
                CleanupTemporaryTestUser(firstUserIdentifier);
                CleanupTemporaryTestUser(secondUserIdentifier);
            }
        }

        [Fact]
        public void HandleMessageUpdate_BothPartiesAcceptCash_CreatesSystemMessage()
        {
            int firstUserIdentifier = -1;
            int secondUserIdentifier = -1;
            int conversationIdentifier = -1;
            int defaultMessageIdentifier = -1;
            int cashPaymentIdentifier = 1;
            string cashMessageContent = "cash";

            try
            {
                firstUserIdentifier = CreateTemporaryTestUser("A");
                secondUserIdentifier = CreateTemporaryTestUser("B");
                ConversationRepository conversationRepository = new ConversationRepository();

                conversationIdentifier = conversationRepository.CreateConversation(firstUserIdentifier, secondUserIdentifier);
                CashAgreementMessage cashMessage = new CashAgreementMessage(
                    defaultMessageIdentifier, conversationIdentifier, firstUserIdentifier, secondUserIdentifier,
                    cashPaymentIdentifier, DateTime.Now, cashMessageContent, false, false, false);

                conversationRepository.HandleNewMessage(cashMessage);

                cashMessage.IsCashAgreementAcceptedByBuyer = true;
                cashMessage.IsCashAgreementAcceptedBySeller = true;
                conversationRepository.HandleMessageUpdate(cashMessage);

                Conversation retrievedConversation = conversationRepository.GetConversationById(conversationIdentifier);

                Assert.Contains(retrievedConversation.ConversationMessageList, messageItem => messageItem is SystemMessage && messageItem.MessageContentAsString != "New conversation");
            }
            finally
            {
                CleanupConversation(conversationIdentifier);
                CleanupTemporaryTestUser(firstUserIdentifier);
                CleanupTemporaryTestUser(secondUserIdentifier);
            }
        }

        [Fact]
        public void GetConversationsForUser_ExistingConversations_ReturnsPopulatedList()
        {
            int firstUserIdentifier = -1;
            int secondUserIdentifier = -1;
            int conversationIdentifier = -1;

            try
            {
                firstUserIdentifier = CreateTemporaryTestUser("A");
                secondUserIdentifier = CreateTemporaryTestUser("B");
                ConversationRepository conversationRepository = new ConversationRepository();

                conversationIdentifier = conversationRepository.CreateConversation(firstUserIdentifier, secondUserIdentifier);
                var conversationList = conversationRepository.GetConversationsForUser(firstUserIdentifier);

                Assert.Contains(conversationList, conversationItem => conversationItem.ConversationId == conversationIdentifier);
            }
            finally
            {
                CleanupConversation(conversationIdentifier);
                CleanupTemporaryTestUser(firstUserIdentifier);
                CleanupTemporaryTestUser(secondUserIdentifier);
            }
        }

        [Fact]
        public void Subscribe_ValidObserver_ReceivesMessageNotification()
        {
            int firstUserIdentifier = -1;
            int secondUserIdentifier = -1;
            int conversationIdentifier = -1;
            int defaultMessageIdentifier = -1;
            string textContent = "hi";

            try
            {
                firstUserIdentifier = CreateTemporaryTestUser("A");
                secondUserIdentifier = CreateTemporaryTestUser("B");
                ConversationRepository conversationRepository = new ConversationRepository();
                Mock<IConversationService> conversationServiceMock = new Mock<IConversationService>();

                conversationRepository.Subscribe(firstUserIdentifier, conversationServiceMock.Object);
                conversationIdentifier = conversationRepository.CreateConversation(firstUserIdentifier, secondUserIdentifier);

                TextMessage textMessage = new TextMessage(defaultMessageIdentifier, conversationIdentifier, firstUserIdentifier, secondUserIdentifier, DateTime.Now, textContent);
                conversationRepository.HandleNewMessage(textMessage);

                conversationServiceMock.Verify(serviceMock => serviceMock.OnMessageReceived(It.IsAny<Message>()), Times.AtLeastOnce);
            }
            finally
            {
                CleanupConversation(conversationIdentifier);
                CleanupTemporaryTestUser(firstUserIdentifier);
                CleanupTemporaryTestUser(secondUserIdentifier);
            }
        }

        [Fact]
        public void HandleNewMessage_ValidTextMessage_AddsMessageToConversation()
        {
            int firstUserIdentifier = -1;
            int secondUserIdentifier = -1;
            int conversationIdentifier = -1;
            int defaultMessageIdentifier = -1;
            string textContent = "hello";

            try
            {
                firstUserIdentifier = CreateTemporaryTestUser("A");
                secondUserIdentifier = CreateTemporaryTestUser("B");
                ConversationRepository conversationRepository = new ConversationRepository();

                conversationIdentifier = conversationRepository.CreateConversation(firstUserIdentifier, secondUserIdentifier);
                TextMessage textMessage = new TextMessage(defaultMessageIdentifier, conversationIdentifier, firstUserIdentifier, secondUserIdentifier, DateTime.Now, textContent);

                conversationRepository.HandleNewMessage(textMessage);
                Conversation retrievedConversation = conversationRepository.GetConversationById(conversationIdentifier);

                Assert.Contains(retrievedConversation.ConversationMessageList, messageItem => messageItem.MessageContentAsString == textContent);
            }
            finally
            {
                CleanupConversation(conversationIdentifier);
                CleanupTemporaryTestUser(firstUserIdentifier);
                CleanupTemporaryTestUser(secondUserIdentifier);
            }
        }

        [Fact]
        public void HandleReadReceipt_ValidReceipt_UpdatesLastReadTime()
        {
            int firstUserIdentifier = -1;
            int secondUserIdentifier = -1;
            int conversationIdentifier = -1;

            try
            {
                firstUserIdentifier = CreateTemporaryTestUser("A");
                secondUserIdentifier = CreateTemporaryTestUser("B");
                ConversationRepository conversationRepository = new ConversationRepository();

                conversationIdentifier = conversationRepository.CreateConversation(firstUserIdentifier, secondUserIdentifier);
                ReadReceipt readReceipt = new ReadReceipt(conversationIdentifier, firstUserIdentifier, secondUserIdentifier, DateTime.UtcNow);

                conversationRepository.HandleReadReceipt(readReceipt);
                Conversation retrievedConversation = conversationRepository.GetConversationById(conversationIdentifier);

                Assert.True(retrievedConversation.LastMessageReadTime.ContainsKey(firstUserIdentifier));
            }
            finally
            {
                CleanupConversation(conversationIdentifier);
                CleanupTemporaryTestUser(firstUserIdentifier);
                CleanupTemporaryTestUser(secondUserIdentifier);
            }
        }

        [Fact]
        public void HandleRentalRequestFinalization_InvalidIdentifier_ExecutesWithoutError()
        {
            ConversationRepository conversationRepository = new ConversationRepository();
            int invalidMessageIdentifier = -999;

            Exception executionException = Record.Exception(() => conversationRepository.HandleRentalRequestFinalization(invalidMessageIdentifier));

            Assert.Null(executionException);
        }

        [Fact]
        public void Unsubscribe_ValidUser_RemovesServiceNotification()
        {
            int firstUserIdentifier = -1;
            int secondUserIdentifier = -1;
            int conversationIdentifier = -1;
            int defaultMessageIdentifier = -1;

            try
            {
                firstUserIdentifier = CreateTemporaryTestUser("A");
                secondUserIdentifier = CreateTemporaryTestUser("B");
                ConversationRepository conversationRepository = new ConversationRepository();
                Mock<IConversationService> conversationServiceMock = new Mock<IConversationService>();

                conversationRepository.Subscribe(firstUserIdentifier, conversationServiceMock.Object);
                conversationRepository.Unsubscribe(firstUserIdentifier);

                conversationIdentifier = conversationRepository.CreateConversation(firstUserIdentifier, secondUserIdentifier);
                TextMessage textMessage = new TextMessage(defaultMessageIdentifier, conversationIdentifier, firstUserIdentifier, secondUserIdentifier, DateTime.Now, "x");

                conversationRepository.HandleNewMessage(textMessage);

                conversationServiceMock.Verify(serviceMock => serviceMock.OnMessageReceived(It.IsAny<Message>()), Times.Never);
            }
            finally
            {
                CleanupConversation(conversationIdentifier);
                CleanupTemporaryTestUser(firstUserIdentifier);
                CleanupTemporaryTestUser(secondUserIdentifier);
            }
        }

        [Fact]
        public void CreateSystemMessageForCashAgreementFinalization_ValidInput_AddsSystemMessage()
        {
            int firstUserIdentifier = -1;
            int secondUserIdentifier = -1;
            int conversationIdentifier = -1;
            string documentPath = "file.pdf";

            try
            {
                firstUserIdentifier = CreateTemporaryTestUser("A");
                secondUserIdentifier = CreateTemporaryTestUser("B");
                ConversationRepository conversationRepository = new ConversationRepository();

                conversationIdentifier = conversationRepository.CreateConversation(firstUserIdentifier, secondUserIdentifier);
                conversationRepository.CreateSystemMessageForCashAgreementFinalization(conversationIdentifier, documentPath);

                Conversation retrievedConversation = conversationRepository.GetConversationById(conversationIdentifier);

                Assert.Contains(retrievedConversation.ConversationMessageList, messageItem => messageItem.TypeOfMessage == MessageType.MessageSystem && messageItem.MessageContentAsString != "New conversation");
            }
            finally
            {
                CleanupConversation(conversationIdentifier);
                CleanupTemporaryTestUser(firstUserIdentifier);
                CleanupTemporaryTestUser(secondUserIdentifier);
            }
        }

        [Fact]
        public void GetConversationById_InvalidIdentifier_ReturnsNonNullObject()
        {
            ConversationRepository conversationRepository = new ConversationRepository();
            int invalidConversationIdentifier = -999;

            var retrievedConversation = conversationRepository.GetConversationById(invalidConversationIdentifier);

            Assert.NotNull(retrievedConversation);
        }

        [Fact]
        public void CreateConversation_UsersAlreadyHaveConversation_ReturnsExistingIdentifier()
        {
            int firstUserIdentifier = -1;
            int secondUserIdentifier = -1;
            int firstConversationIdentifier = -1;
            int secondConversationIdentifier = -1;

            try
            {
                firstUserIdentifier = CreateTemporaryTestUser("A");
                secondUserIdentifier = CreateTemporaryTestUser("B");
                ConversationRepository conversationRepository = new ConversationRepository();

                firstConversationIdentifier = conversationRepository.CreateConversation(firstUserIdentifier, secondUserIdentifier);
                secondConversationIdentifier = conversationRepository.CreateConversation(firstUserIdentifier, secondUserIdentifier);

                Assert.Equal(firstConversationIdentifier, secondConversationIdentifier);
            }
            finally
            {
                CleanupConversation(firstConversationIdentifier);
                CleanupTemporaryTestUser(firstUserIdentifier);
                CleanupTemporaryTestUser(secondUserIdentifier);
            }
        }

        [Fact]
        public void CreateSystemMessageForCashAgreementFinalization_SubscribedUsers_NotifiesBothParticipants()
        {
            int firstUserIdentifier = -1;
            int secondUserIdentifier = -1;
            int conversationIdentifier = -1;
            string documentPath = "file.pdf";

            try
            {
                firstUserIdentifier = CreateTemporaryTestUser("A");
                secondUserIdentifier = CreateTemporaryTestUser("B");
                ConversationRepository conversationRepository = new ConversationRepository();
                Mock<IConversationService> conversationServiceMock = new Mock<IConversationService>();

                conversationRepository.Subscribe(firstUserIdentifier, conversationServiceMock.Object);
                conversationRepository.Subscribe(secondUserIdentifier, conversationServiceMock.Object);

                conversationIdentifier = conversationRepository.CreateConversation(firstUserIdentifier, secondUserIdentifier);
                conversationRepository.CreateSystemMessageForCashAgreementFinalization(conversationIdentifier, documentPath);

                conversationServiceMock.Verify(serviceMock => serviceMock.OnMessageReceived(It.IsAny<Message>()), Times.AtLeastOnce);
            }
            finally
            {
                CleanupConversation(conversationIdentifier);
                CleanupTemporaryTestUser(firstUserIdentifier);
                CleanupTemporaryTestUser(secondUserIdentifier);
            }
        }

        [Fact]
        public void Unsubscribe_InvalidUserIdentifier_ExecutesWithoutError()
        {
            ConversationRepository conversationRepository = new ConversationRepository();
            int invalidUserIdentifier = 99999;

            Exception executionException = Record.Exception(() => conversationRepository.Unsubscribe(invalidUserIdentifier));

            Assert.Null(executionException);
        }

        [Fact]
        public void HandleMessageUpdate_CashAgreementNotFullyAccepted_DoesNotCreateSystemMessage()
        {
            int firstUserIdentifier = -1;
            int secondUserIdentifier = -1;
            int conversationIdentifier = -1;
            int defaultMessageIdentifier = -1;
            int cashPaymentIdentifier = 1;

            try
            {
                firstUserIdentifier = CreateTemporaryTestUser("A");
                secondUserIdentifier = CreateTemporaryTestUser("B");
                ConversationRepository conversationRepository = new ConversationRepository();

                conversationIdentifier = conversationRepository.CreateConversation(firstUserIdentifier, secondUserIdentifier);
                CashAgreementMessage cashMessage = new CashAgreementMessage(
                    defaultMessageIdentifier, conversationIdentifier, firstUserIdentifier, secondUserIdentifier,
                    cashPaymentIdentifier, DateTime.Now, "cash", false, false, false);

                conversationRepository.HandleNewMessage(cashMessage);
                conversationRepository.HandleMessageUpdate(cashMessage);

                Conversation retrievedConversation = conversationRepository.GetConversationById(conversationIdentifier);

                Assert.DoesNotContain(retrievedConversation.ConversationMessageList, messageItem => messageItem is SystemMessage && messageItem.MessageContentAsString != "New conversation");
            }
            finally
            {
                CleanupConversation(conversationIdentifier);
                CleanupTemporaryTestUser(firstUserIdentifier);
                CleanupTemporaryTestUser(secondUserIdentifier);
            }
        }
    }
}