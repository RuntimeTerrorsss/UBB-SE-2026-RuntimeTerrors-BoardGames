using BookingBoardgamesILoveBan.Src.Chat.Model;
using BookingBoardgamesILoveBan.Src.Chat.Repository;
using BookingBoardgamesILoveBan.Src.Enum;
using BookingBoardgamesILoveBan.Src.Model;
using Microsoft.Data.SqlClient;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BookingBoardgamesILoveBan.Tests.Chat
{
    public class ConversationRepositoryTests
    {
        private readonly ConversationRepository _repo;
        private string connectionString = "Server=localhost\\MSSQLSERVER02;Database=ChatTestDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";

        public ConversationRepositoryTests()
        {
            _repo = new ConversationRepository(true);
        }

        private void ClearDatabase()
        {
            using var conn = new SqlConnection(connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                DELETE FROM ConversationUser;
                DELETE FROM SystemMessage;
                DELETE FROM CashAgreementMessage;
                DELETE FROM RentalRequestMessage;
                DELETE FROM ImageMessage;
                DELETE FROM TextMessage;
                DELETE FROM Message;
                DELETE FROM Conversation;
            ";

            cmd.ExecuteNonQuery();
        }

        [Fact]
        public void GetConversationsForUser_Returns_List()
        {
            ClearDatabase();
            var result = _repo.GetConversationsForUser(1);

            Assert.NotNull(result);
        }

        [Fact]
        public void CreateConversation_Creates_Or_Returns_Existing()
        {
            ClearDatabase();
            int sender = 1;
            int receiver = 2;

            var id1 = _repo.CreateConversation(sender, receiver);
            var id2 = _repo.CreateConversation(sender, receiver);

            Assert.Equal(id1, id2);
        }

        [Fact]
        public void HandleNewMessage_Inserts_TextMessage()
        {
            ClearDatabase();
            var convId = _repo.CreateConversation(1, 2);

            var message = new TextMessage(
                -1,
                convId,
                1,
                2,
                DateTime.Now,
                "integration test message"
            );

            _repo.HandleNewMessage(message);

            var conversation = _repo.GetConversation(convId);

            Assert.Contains(conversation.MessageList,
                m => m.ContentAsString == "integration test message");
        }

        [Fact]
        public async Task HandleReadReceipt_Updates_LastRead()
        {
            
            ClearDatabase();
           
            var convId = _repo.CreateConversation(1, 2, true);

            var receipt = new ReadReceipt(
                convId,
                readerId: 1,
                receiverId: 2,
                DateTime.UtcNow
            );

            _repo.HandleReadReceipt(receipt);

            var conv = _repo.GetConversation(convId);

            Assert.NotEmpty(conv.LastRead);
        }

        [Fact]
        public void HandleMessageUpdate_Updates_Message()
        {
            ClearDatabase();
            var convId = _repo.CreateConversation(1, 2);

            var message = new RentalRequestMessage(
                -1,
                convId,
                1,
                2,
                DateTime.Now,
                "rent",
                requestId: 99,
                isResolved: false,
                isAccepted: false
            );

            _repo.HandleNewMessage(message);

            var stored = _repo.GetConversation(convId)
                              .MessageList
                              .OfType<RentalRequestMessage>()
                              .First();

            stored.IsResolved = true;

            _repo.HandleMessageUpdate(stored);

            var updated = _repo.GetConversation(convId)
                               .MessageList
                               .OfType<RentalRequestMessage>()
                               .First();

            Assert.True(updated.IsResolved);
        }

        [Fact]
        public void HandleRentalRequestFinalization_Sets_Resolved_And_Updates()
        {
            ClearDatabase();
            var convId = _repo.CreateConversation(1, 2);

            var message = new RentalRequestMessage(
                -1,
                convId,
                1,
                2,
                DateTime.Now,
                "rent",
                requestId: 55,
                isResolved: false,
                isAccepted: true
            );

            _repo.HandleNewMessage(message);

            var stored = _repo.GetConversation(convId)
                              .MessageList
                              .OfType<RentalRequestMessage>()
                              .First();

            _repo.HandleRentalRequestFinalization(stored.Id);

            var conv = _repo.GetConversation(convId);
            var msg = conv.MessageList.First(m => m.Id == stored.Id);

            Assert.True(((RentalRequestMessage)msg).IsResolved);
        }

        [Fact]
        public void CreateCashAgreementMessage_Creates_Message()
        {
            ClearDatabase();
            var convId = _repo.CreateConversation(1, 2);

            var rental = new RentalRequestMessage(
                -1,
                convId,
                1,
                2,
                DateTime.Now,
                "rent",
                123,
                false,
                true
            );

            _repo.HandleNewMessage(rental);

            var stored = _repo.GetConversation(convId)
                              .MessageList
                              .OfType<RentalRequestMessage>()
                              .First();

            _repo.CreateCashAgreementMessage(stored.Id, 999);

            var conv = _repo.GetConversation(convId);

            Assert.Contains(conv.MessageList,
                m => m.Type == MessageType.CashAgreement);
        }

        [Fact]
        public void SystemMessage_Is_Inserted_When_All_Cash_Confirmed()
        {
            ClearDatabase();
            var convId = _repo.CreateConversation(1, 2, true);

            var rental = new RentalRequestMessage(
                -1,
                convId,
                1,
                2,
                DateTime.Now,
                "rent",
                10,
                false,
                true
            );

            _repo.HandleNewMessage(rental);

            var stored = _repo.GetConversation(convId)
                              .MessageList
                              .OfType<RentalRequestMessage>()
                              .First();

            _repo.CreateCashAgreementMessage(stored.Id, 500);

            var cash = _repo.GetConversation(convId)
                            .MessageList
                            .OfType<CashAgreementMessage>()
                            .First();

            cash.IsAcceptedByBuyer = true;
            cash.IsAcceptedBySeller = true;

            var conv = _repo.GetConversation(convId);

            Assert.Contains(conv.MessageList,
                m => m.Type == MessageType.System);
        }
    }
}