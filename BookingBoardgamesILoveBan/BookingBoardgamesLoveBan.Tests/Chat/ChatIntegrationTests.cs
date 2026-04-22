using BookingBoardgamesILoveBan.Src.Chat.DTO;
using BookingBoardgamesILoveBan.Src.Chat.Repository;
using BookingBoardgamesILoveBan.Src.Chat.Service;
using BookingBoardgamesILoveBan.Src.Enum;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Repository;
using Microsoft.Data.SqlClient;
using System;
using System.Linq;
using Xunit;

namespace BookingBoardgamesLoveBan.Tests.Chat
{
    public class ChatIntegrationTests
    {
        private readonly string connectionString;

        public ChatIntegrationTests()
        {
            DatabaseBootstrap.Initialize();
            connectionString = DatabaseBootstrap.GetAppConnection();
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
        public void SendMessage_ValidPipeline_SuccessfullySavesToDatabase()
        {
            ConversationRepository conversationRepository = new ConversationRepository();
            UserRepository userRepository = new UserRepository();

            int senderIdentifier = 5;
            int receiverIdentifier = 2;
            int unassignedMessageIdentifier = -1;
            string messageContent = "Integration test message via concrete pipeline";

            ConversationService conversationService = new ConversationService(conversationRepository, senderIdentifier, userRepository);
            int conversationIdentifier = conversationRepository.CreateConversation(senderIdentifier, receiverIdentifier);

            MessageDataTransferObject messageDataTransferObject = new MessageDataTransferObject(
                unassignedMessageIdentifier,
                conversationIdentifier,
                senderIdentifier,
                receiverIdentifier,
                DateTime.Now,
                messageContent,
                MessageType.MessageText,
                string.Empty,
                false, false, false, false,
                unassignedMessageIdentifier, unassignedMessageIdentifier
            );

            try
            {
                conversationService.SendMessage(messageDataTransferObject);

                var retrievedConversations = conversationService.FetchConversations();
                var targetConversation = retrievedConversations.FirstOrDefault(conversationItem => conversationItem.Id == conversationIdentifier);

                Assert.NotNull(targetConversation);
                Assert.Contains(targetConversation.MessageList, messageItem => messageItem.content == messageContent);
            }
            finally
            {
                CleanupConversation(conversationIdentifier);
            }
        }

        [Fact]
        public void SendReadReceipt_ValidPipeline_UpdatesDatabaseReadTimeProperly()
        {
            ConversationRepository conversationRepository = new ConversationRepository();
            UserRepository userRepository = new UserRepository();

            int readerIdentifier = 5;
            int receiverIdentifier = 2;
            int timeBufferSeconds = -2;

            ConversationService conversationService = new ConversationService(conversationRepository, readerIdentifier, userRepository);
            int conversationIdentifier = conversationRepository.CreateConversation(readerIdentifier, receiverIdentifier);

            try
            {
                var initialConversations = conversationService.FetchConversations();
                var targetConversation = initialConversations.FirstOrDefault(conversationItem => conversationItem.Id == conversationIdentifier);

                DateTime timeBeforeReceipt = DateTime.Now.AddSeconds(timeBufferSeconds);

                conversationService.SendReadReceipt(targetConversation);

                var updatedConversations = conversationService.FetchConversations();
                var updatedTargetConversation = updatedConversations.FirstOrDefault(conversationItem => conversationItem.Id == conversationIdentifier);

                Assert.True(updatedTargetConversation.LastRead[readerIdentifier] >= timeBeforeReceipt);
            }
            finally
            {
                CleanupConversation(conversationIdentifier);
            }
        }

        [Fact]
        public void UpdateMessage_RentalRequest_SuccessfullyUpdatesInDatabase()
        {
            ConversationRepository conversationRepository = new ConversationRepository();
            UserRepository userRepository = new UserRepository();

            int senderIdentifier = 5;
            int receiverIdentifier = 2;
            int unassignedMessageIdentifier = -1;
            int validDatabaseRequestIdentifier = 5;
            string rentalMessageContent = "Can I rent this board game?";

            ConversationService conversationService = new ConversationService(conversationRepository, senderIdentifier, userRepository);
            int conversationIdentifier = conversationRepository.CreateConversation(senderIdentifier, receiverIdentifier);

            MessageDataTransferObject rentalRequestDataTransferObject = new MessageDataTransferObject(
                unassignedMessageIdentifier,
                conversationIdentifier,
                senderIdentifier,
                receiverIdentifier,
                DateTime.Now,
                rentalMessageContent,
                MessageType.MessageRentalRequest,
                string.Empty,
                false, false, false, false,
                validDatabaseRequestIdentifier, unassignedMessageIdentifier
            );

            try
            {
                conversationService.SendMessage(rentalRequestDataTransferObject);

                var retrievedConversations = conversationService.FetchConversations();
                var targetConversation = retrievedConversations.FirstOrDefault(conversationItem => conversationItem.Id == conversationIdentifier);
                var storedRentalMessage = targetConversation.MessageList.FirstOrDefault(messageItem => messageItem.type == MessageType.MessageRentalRequest);

                var acceptedRentalMessage = storedRentalMessage with { isAccepted = true };

                conversationService.UpdateMessage(acceptedRentalMessage);

                var finalConversations = conversationService.FetchConversations();
                var finalTargetConversation = finalConversations.FirstOrDefault(conversationItem => conversationItem.Id == conversationIdentifier);
                var finalStoredRentalMessage = finalTargetConversation.MessageList.FirstOrDefault(messageItem => messageItem.type == MessageType.MessageRentalRequest);

                Assert.True(finalStoredRentalMessage.isAccepted);
            }
            finally
            {
                CleanupConversation(conversationIdentifier);
            }
        }
    }
}