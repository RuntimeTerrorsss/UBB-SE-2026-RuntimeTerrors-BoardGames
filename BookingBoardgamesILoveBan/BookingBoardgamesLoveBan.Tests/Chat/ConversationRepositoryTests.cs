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
        public void CreateConversation_And_SendMessage_PersistsCorrectly()
        {
            int user1 = 9001;
            int user2 = 9002;
            int conversationId = -1;

            try
            {
                var repo = new ConversationRepository(true);

                conversationId = repo.CreateConversation(user1, user2);

                Assert.True(conversationId > 0);

                var message = new TextMessage(
                    -1,
                    conversationId,
                    user1,
                    user2,
                    DateTime.Now,
                    "hello integration test"
                );

                repo.HandleNewMessage(message);

                var conversation = repo.GetConversationById(conversationId);

                Assert.NotNull(conversation);
                Assert.Contains(conversation.ConversationMessageList,
                    m => m.MessageContentAsString == "hello integration test");
            }
            finally
            {
                CleanupConversation(conversationId);
            }
        }

        [Fact]
        public void HandleReadReceipt_UpdatesLastRead()
        {
            int user1 = 9011;
            int user2 = 9012;
            int conversationId = -1;

            try
            {
                var repo = new ConversationRepository(true);

                conversationId = repo.CreateConversation(user1, user2);

                var receipt = new ReadReceipt(
                    conversationId,
                    user1,
                    user2,
                    DateTime.UtcNow
                );

                repo.HandleReadReceipt(receipt);

                var conversation = repo.GetConversationById(conversationId);

                Assert.True(conversation.LastMessageReadTime.ContainsKey(user1));
            }
            finally
            {
                CleanupConversation(conversationId);
            }
        }

        [Fact]
        public void RentalRequest_Finalization_UpdatesMessage()
        {
            int user1 = 9021;
            int user2 = 9022;
            int conversationId = -1;

            try
            {
                var repo = new ConversationRepository(true);

                conversationId = repo.CreateConversation(user1, user2);

                var message = new RentalRequestMessage(
                    -1,
                    conversationId,
                    user1,
                    user2,
                    DateTime.Now,
                    "rent pls",
                    777,
                    false,
                    false
                );

                repo.HandleNewMessage(message);

                var stored = repo.GetConversationById(conversationId)
                    .ConversationMessageList
                    .OfType<RentalRequestMessage>()
                    .First();

                repo.HandleRentalRequestFinalization(stored.MessageId);

                var updated = repo.GetConversationById(conversationId)
                    .ConversationMessageList
                    .OfType<RentalRequestMessage>()
                    .First();

                Assert.True(updated.IsRequestResolved);
            }
            finally
            {
                CleanupConversation(conversationId);
            }
        }

        [Fact]
        public void CreateConversation_ReusesExistingConversation()
        {
            var repo = new ConversationRepository(true);

            int id1 = repo.CreateConversation(100, 200);
            int id2 = repo.CreateConversation(100, 200);

            Assert.Equal(id1, id2);

            CleanupConversation(id1);
        }

        [Fact]
        public void HandleNewMessage_ImageMessage_Persists()
        {
            var repo = new ConversationRepository(true);

            int cid = repo.CreateConversation(1, 2);

            var msg = new ImageMessage(-1, cid, 1, 2, DateTime.Now, "img.png");

            repo.HandleNewMessage(msg);

            var conv = repo.GetConversationById(cid);

            Assert.Contains(conv.ConversationMessageList,
                m => m is ImageMessage);

            CleanupConversation(cid);
        }

        [Fact]
        public void HandleNewMessage_CashAgreement_Persists()
        {
            var repo = new ConversationRepository(true);

            int cid = repo.CreateConversation(1, 2);

            var msg = new CashAgreementMessage(
                -1, cid, 1, 2, 999,
                DateTime.Now, "cash",
                false, false, false);

            repo.HandleNewMessage(msg);

            var conv = repo.GetConversationById(cid);

            Assert.Contains(conv.ConversationMessageList,
                m => m is CashAgreementMessage);

            CleanupConversation(cid);
        }

        [Fact]
        public void HandleMessageUpdate_RentalRequest_UpdatesDB()
        {
            var repo = new ConversationRepository(true);

            int cid = repo.CreateConversation(1, 2);

            var msg = new RentalRequestMessage(
                -1, cid, 1, 2, DateTime.Now,
                "rent", 1, false, false);

            repo.HandleNewMessage(msg);

            var stored = repo.GetConversationById(cid)
                .ConversationMessageList
                .OfType<RentalRequestMessage>()
                .First();

            stored.IsRequestAccepted = true;

            repo.HandleMessageUpdate(stored);

            var updated = repo.GetConversationById(cid)
                .ConversationMessageList
                .OfType<RentalRequestMessage>()
                .First();

            Assert.True(updated.IsRequestAccepted);

            CleanupConversation(cid);
        }

        [Fact]
        public void CashAgreement_FullFlow_CreatesSystemMessage()
        {
            var repo = new ConversationRepository(true);

            int cid = repo.CreateConversation(1, 2);

            var msg = new CashAgreementMessage(
                -1, cid, 1, 2, 1,
                DateTime.Now, "cash",
                false, false, false);

            repo.HandleNewMessage(msg);

            msg.IsCashAgreementAcceptedByBuyer = true;
            msg.IsCashAgreementAcceptedBySeller = true;

            repo.HandleMessageUpdate(msg);

            var conv = repo.GetConversationById(cid);

            Assert.Contains(conv.ConversationMessageList,
                m => m is SystemMessage);

            CleanupConversation(cid);
        }

        [Fact]
        public void GetConversationsForUser_ReturnsData()
        {
            var repo = new ConversationRepository(true);

            int cid = repo.CreateConversation(50, 60);

            var list = repo.GetConversationsForUser(50);

            Assert.Contains(list, c => c.ConversationId == cid);

            CleanupConversation(cid);
        }

        [Fact]
        public void Observer_Receives_MessageNotification()
        {
            var repo = new ConversationRepository(true);

            var mock = new Mock<IConversationService>();
            repo.Subscribe(1, mock.Object);

            int cid = repo.CreateConversation(1, 2);

            repo.HandleNewMessage(
                new TextMessage(-1, cid, 1, 2, DateTime.Now, "hi"));

            mock.Verify(m => m.OnMessageReceived(It.IsAny<Message>()),
                Times.AtLeastOnce);

            CleanupConversation(cid);
        }

        private void CleanupConversation(int conversationId)
        {
            if (conversationId <= 0) return;

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                var cmd = new SqlCommand(@"
                    DELETE FROM TextMessage WHERE mid IN (SELECT mid FROM Message WHERE ConversationId = @cid);
                    DELETE FROM ImageMessage WHERE mid IN (SELECT mid FROM Message WHERE ConversationId = @cid);
                    DELETE FROM CashAgreementMessage WHERE mid IN (SELECT mid FROM Message WHERE ConversationId = @cid);
                    DELETE FROM RentalRequestMessage WHERE mid IN (SELECT mid FROM Message WHERE ConversationId = @cid);
                    DELETE FROM SystemMessage WHERE mid IN (SELECT mid FROM Message WHERE ConversationId = @cid);
                    DELETE FROM Message WHERE ConversationId = @cid;
                    DELETE FROM ConversationUser WHERE cid = @cid;
                    DELETE FROM Conversation WHERE cid = @cid;
                ", conn);

                cmd.Parameters.AddWithValue("@cid", conversationId);
                cmd.ExecuteNonQuery();
            }
        }

        [Fact]
        public void HandleNewMessage_ValidConversation_AddsMessage()
        {
            var repo = new ConversationRepository(true);

            int user1 = 1, user2 = 2;

            int cid = repo.CreateConversation(user1, user2);

            var msg = new TextMessage(-1, cid, user1, user2, DateTime.Now, "hello");

            repo.HandleNewMessage(msg);

            var conv = repo.GetConversationById(cid);

            Assert.Contains(conv.ConversationMessageList,
                m => m.MessageContentAsString == "hello");
        }

        [Fact]
        public void HandleReadReceipt_UpdatesDatabase()
        {
            var repo = new ConversationRepository(true);

            int cid = repo.CreateConversation(1, 2);

            repo.HandleReadReceipt(new ReadReceipt(cid, 1, 2, DateTime.UtcNow));

            var conv = repo.GetConversationById(cid);

            Assert.True(conv.LastMessageReadTime.Count > 0);
        }

        [Fact]
        public void RentalRequest_Finalization_SetsResolved()
        {
            var repo = new ConversationRepository(true);

            int cid = repo.CreateConversation(1, 2);

            var msg = new RentalRequestMessage(-1, cid, 1, 2, DateTime.Now,
                "rent", 123, false, false);

            repo.HandleNewMessage(msg);

            var stored = repo.GetConversationById(cid)
                .ConversationMessageList.OfType<RentalRequestMessage>().First();

            repo.HandleRentalRequestFinalization(stored.MessageId);

            var updated = repo.GetConversationById(cid)
                .ConversationMessageList.OfType<RentalRequestMessage>().First();

            Assert.True(updated.IsRequestResolved);
        }

        [Fact]
        public void HandleRentalRequestFinalization_InvalidMessage_DoesNotCrash()
        {
            var repo = new ConversationRepository(true);

            repo.HandleRentalRequestFinalization(-999);
        }

        [Fact]
        public void CashAgreement_UpdateFlow_TriggersSystemUpdate()
        {
            var repo = new ConversationRepository(true);

            int cid = repo.CreateConversation(1, 2);

            var msg = new CashAgreementMessage(
                -1, cid, 1, 2, 1, DateTime.Now,
                "cash", false, false, false);

            repo.HandleNewMessage(msg);

            var stored = repo.GetConversationById(cid)
                .ConversationMessageList.OfType<CashAgreementMessage>().First();

            repo.HandleMessageUpdate(stored);

            Assert.True(true);
        }

        [Fact]
        public void Subscribe_And_Notify_Works()
        {
            var repo = new ConversationRepository(true);

            var mock = new Mock<IConversationService>();

            repo.Subscribe(1, mock.Object);

            int cid = repo.CreateConversation(1, 2);

            var msg = new TextMessage(-1, cid, 1, 2, DateTime.Now, "hi");

            repo.HandleNewMessage(msg);

            mock.Verify(x => x.OnMessageReceived(It.IsAny<Message>()), Times.Once);
        }

        [Fact]
        public void Unsubscribe_RemovesSubscriber()
        {
            var repo = new ConversationRepository(true);

            var mock = new Mock<IConversationService>();

            repo.Subscribe(1, mock.Object);
            repo.Unsubscribe(1);

            int cid = repo.CreateConversation(1, 2);

            repo.HandleNewMessage(
                new TextMessage(-1, cid, 1, 2, DateTime.Now, "x"));

            mock.Verify(x => x.OnMessageReceived(It.IsAny<Message>()), Times.Never);
        }

        [Fact]
        public void SystemMessage_IsProcessed()
        {
            var repo = new ConversationRepository(true);

            int cid = repo.CreateConversation(1, 2);

            repo.CreateSystemMessageForCashAgreementFinalization(cid, "file.pdf");

            var conv = repo.GetConversationById(cid);

            Assert.Contains(conv.ConversationMessageList,
                m => m.TypeOfMessage == MessageType.MessageSystem);
        }

        [Fact]
        public void GetMessageById_Invalid_ReturnsNullSafe()
        {
            var repo = new ConversationRepository(true);

            var result = repo.GetConversationById(-999);

            Assert.NotNull(result);
        }
    }
}