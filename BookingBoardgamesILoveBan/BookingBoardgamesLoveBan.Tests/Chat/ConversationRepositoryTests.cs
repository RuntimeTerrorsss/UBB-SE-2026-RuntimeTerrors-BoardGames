using BookingBoardgamesILoveBan.Src.Chat.DTO;
using BookingBoardgamesILoveBan.Src.Chat.Model;
using BookingBoardgamesILoveBan.Src.Chat.Repository;
using BookingBoardgamesILoveBan.Src.Chat.Service;
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
        private readonly ConversationRepository _repository;
        private string connectionString = "Server=localhost\\MSSQLSERVER02;Database=ChatTestDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";

        public ConversationRepositoryTests()
        {
            _repository = new ConversationRepository(true);
        }

        private void ClearDatabase()
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM ConversationUser;
                DELETE FROM SystemMessage;
                DELETE FROM CashAgreementMessage;
                DELETE FROM RentalRequestMessage;
                DELETE FROM ImageMessage;
                DELETE FROM TextMessage;
                DELETE FROM Message;
                DELETE FROM Conversation;
            ";

            command.ExecuteNonQuery();
        }

        [Fact]
        public void GetConversationsForUser_Returns_List()
        {
            ClearDatabase();
            var result = _repository.GetConversationsForUser(1);

            Assert.NotNull(result);
        }

        [Fact]
        public void CreateConversation_Creates_Or_Returns_Existing()
        {
            ClearDatabase();
            int sender = 1;
            int receiver = 2;

            var id1 = _repository.CreateConversation(sender, receiver);
            var id2 = _repository.CreateConversation(sender, receiver);

            Assert.Equal(id1, id2);
        }

        [Fact]
        public void HandleNewMessage_Inserts_TextMessage()
        {
            ClearDatabase();
            var conversationId = _repository.CreateConversation(1, 2);

            var message = new TextMessage(
                -1,
                conversationId,
                1,
                2,
                DateTime.Now,
                "integration test message"
            );

            _repository.HandleNewMessage(message);

            var conversationersation = _repository.GetConversation(conversationId);

            Assert.Contains(conversationersation.MessageList,
                message => message.ContentAsString == "integration test message");
        }

        [Fact]
        public async Task HandleReadReceipt_Updates_LastRead()
        {

            ClearDatabase();

            var conversationId = _repository.CreateConversation(1, 2, true);

            var receipt = new ReadReceipt(
                conversationId,
                readerId: 1,
                receiverId: 2,
                DateTime.UtcNow
            );

            _repository.HandleReadReceipt(receipt);

            var conversation = _repository.GetConversation(conversationId);

            Assert.NotEmpty(conversation.LastRead);
        }

        [Fact]
        public void HandleMessageUpdate_Updates_Message()
        {
            ClearDatabase();
            var conversationId = _repository.CreateConversation(1, 2);

            var message = new RentalRequestMessage(
                -1,
                conversationId,
                1,
                2,
                DateTime.Now,
                "rent",
                requestId: 99,
                isResolved: false,
                isAccepted: false
            );

            _repository.HandleNewMessage(message);

            var stored = _repository.GetConversation(conversationId)
                              .MessageList
                              .OfType<RentalRequestMessage>()
                              .First();

            stored.IsResolved = true;

            _repository.HandleMessageUpdate(stored);

            var updated = _repository.GetConversation(conversationId)
                               .MessageList
                               .OfType<RentalRequestMessage>()
                               .First();

            Assert.True(updated.IsResolved);
        }

        [Fact]
        public void HandleRentalRequestFinalization_Sets_Resolved_And_Updates()
        {
            ClearDatabase();
            var conversationId = _repository.CreateConversation(1, 2);

            var message = new RentalRequestMessage(
                -1,
                conversationId,
                1,
                2,
                DateTime.Now,
                "rent",
                requestId: 55,
                isResolved: false,
                isAccepted: true
            );

            _repository.HandleNewMessage(message);

            var stored = _repository.GetConversation(conversationId)
                              .MessageList
                              .OfType<RentalRequestMessage>()
                              .First();

            _repository.HandleRentalRequestFinalization(stored.Id);

            var conversation = _repository.GetConversation(conversationId);
            var messageResult = conversation.MessageList.First(firstMessage => firstMessage.Id == stored.Id);

            Assert.True(((RentalRequestMessage)messageResult).IsResolved);
        }

        [Fact]
        public void CreateCashAgreementMessage_Creates_Message()
        {
            ClearDatabase();
            var conversationId = _repository.CreateConversation(1, 2);

            var rental = new RentalRequestMessage(
                -1,
                conversationId,
                1,
                2,
                DateTime.Now,
                "rent",
                123,
                false,
                true
            );

            _repository.HandleNewMessage(rental);

            var stored = _repository.GetConversation(conversationId)
                              .MessageList
                              .OfType<RentalRequestMessage>()
                              .First();

            _repository.CreateCashAgreementMessage(stored.Id, 999);

            var conversation = _repository.GetConversation(conversationId);

            Assert.Contains(conversation.MessageList,
                message => message.Type == MessageType.CashAgreement);
        }

        [Fact]
        public void SystemMessage_Is_Inserted_When_All_Cash_Confirmed()
        {
            ClearDatabase();
            var conversationId = _repository.CreateConversation(1, 2, true);

            var rental = new RentalRequestMessage(
                -1,
                conversationId,
                1,
                2,
                DateTime.Now,
                "rent",
                10,
                false,
                true
            );

            _repository.HandleNewMessage(rental);

            var stored = _repository.GetConversation(conversationId)
                              .MessageList
                              .OfType<RentalRequestMessage>()
                              .First();

            _repository.CreateCashAgreementMessage(stored.Id, 500);

            var cash = _repository.GetConversation(conversationId)
                            .MessageList
                            .OfType<CashAgreementMessage>()
                            .First();

            cash.IsAcceptedByBuyer = true;
            cash.IsAcceptedBySeller = true;

            var conversation = _repository.GetConversation(conversationId);

            Assert.Contains(conversation.MessageList,
                message => message.Type == MessageType.System);
        }

        [Fact]
        public void HandleNewMessage_ImageMessage_PersistsAndRetrievable()
        {
            ClearDatabase();
            var conversationId = _repository.CreateConversation(1, 2);

            var message = new ImageMessage(
                -1,
                conversationId,
                1,
                2,
                DateTime.Now,
                "img.png"
            );

            _repository.HandleNewMessage(message);

            var conversation = _repository.GetConversation(conversationId);

            var stored = conversation.MessageList
                .OfType<ImageMessage>()
                .FirstOrDefault();

            Assert.NotNull(stored);
            Assert.Equal("img.png", stored.ImageUrl);
        }

        [Fact]
        public void HandleNewMessage_CashAgreement_PersistsCorrectly()
        {
            ClearDatabase();
            var conversationId = _repository.CreateConversation(1, 2);

            var message = new CashAgreementMessage(
                -1,
                conversationId,
                1,
                2,
                123,
                DateTime.Now,
                "cash",
                false,
                false,
                false
            );

            _repository.HandleNewMessage(message);

            var conversation = _repository.GetConversation(conversationId);

            var stored = conversation.MessageList
                .OfType<CashAgreementMessage>()
                .FirstOrDefault();

            Assert.NotNull(stored);
            Assert.Equal(123, stored.PaymentId);
        }

        [Fact]
        public void HandleNewMessage_RentalRequest_PersistsCorrectly()
        {
            ClearDatabase();
            var conversationId = _repository.CreateConversation(1, 2);

            var message = new RentalRequestMessage(
                -1,
                conversationId,
                1,
                2,
                DateTime.Now,
                "rent",
                99,
                false,
                false
            );

            _repository.HandleNewMessage(message);

            var conversation = _repository.GetConversation(conversationId);

            var stored = conversation.MessageList
                .OfType<RentalRequestMessage>()
                .FirstOrDefault();

            Assert.NotNull(stored);
            Assert.Equal(99, stored.RequestId);
        }

        [Fact]
        public void HandleNewMessage_SystemMessage_NotifiesSubscribersAndStores()
        {
            ClearDatabase();
            var conversationId = _repository.CreateConversation(1, 2);

            var message = new SystemMessage(
                -1,
                conversationId,
                DateTime.Now,
                "system message"
            );

            _repository.HandleNewMessage(message);

            var conversation = _repository.GetConversation(conversationId);

            Assert.Contains(
                conversation.MessageList,
                m => m.Type == MessageType.System
            );
        }

        [Fact]
        public void GetMessageById_TextMessage_ReturnsCorrectType()
        {
            ClearDatabase();
            var conversationId = _repository.CreateConversation(1, 2);

            var message = new TextMessage(-1, conversationId, 1, 2, DateTime.Now, "hello");
            _repository.HandleNewMessage(message);

            var stored = _repository.GetConversation(conversationId)
                .MessageList.OfType<TextMessage>()
                .First();

            var result = _repository.GetConversation(conversationId)
                .MessageList.OfType<TextMessage>()
                .First();

            Assert.Equal("hello", result.ContentAsString);
        }

        [Fact]
        public void CreateConversation_CreatesUsersAndMessages()
        {
            ClearDatabase();

            var id = _repository.CreateConversation(1, 2, true);

            Conversation conversation = null;

            for (int i = 0; i < 10; i++)
            {
                conversation = _repository.GetConversation(id);
                if (conversation.MessageList.Any(m => m.Type == MessageType.System))
                    break;

                Thread.Sleep(50);
            }

            Assert.Contains(
                conversation.MessageList,
                m => m.Type == MessageType.System
            );
        }

        [Fact]
        public void NotifySubscribers_SystemMessage_UsesDbParticipants()
        {
            ClearDatabase();

            var conversationId = _repository.CreateConversation(1, 2);

            var message = new SystemMessage(-1, conversationId, DateTime.Now, "sys");

            bool called = false;

            _repository.Subscribe(1, new FakeService(() => called = true));
            _repository.Subscribe(2, new FakeService(() => called = true));

            _repository.HandleNewMessage(message);

            Assert.True(called);
        }

        public class FakeService : IConversationService
        {
            private readonly Action _onMessage;

            public event Action<MessageDTO, string> MessageProcessed;
            public event Action<ConversationDTO, string> ConversationProcessed;
            public event Action<ReadReceiptDTO> ReadReceiptProcessed;
            public event Action<MessageDTO, string> MessageUpdateProcessed;

            public FakeService(Action onMessage)
            {
                _onMessage = onMessage;
            }

            public void OnMessageReceived(Message message) => _onMessage();
            public void OnConversationReceived(Conversation conversation) { }
            public void OnReadReceiptReceived(ReadReceipt readReceipt) { }
            public void OnMessageUpdateReceived(Message message) { }

            public List<ConversationDTO> FetchConversations()
            {
                throw new NotImplementedException();
            }

            public string GetOtherUserNameByConversationDTO(ConversationDTO conversation)
            {
                throw new NotImplementedException();
            }

            public void UpdateMessage(MessageDTO message)
            {
                throw new NotImplementedException();
            }

            public void SendMessage(MessageDTO message)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void HandleReadReceipt_UpdatesDbAndReturnsCorrectState()
        {
            ClearDatabase();

            var conversationId = _repository.CreateConversation(1, 2);

            var receipt = new ReadReceipt(
                conversationId,
                1,
                2,
                DateTime.UtcNow
            );

            _repository.HandleReadReceipt(receipt);

            var conversation = _repository.GetConversation(conversationId);

            Assert.True(conversation.LastRead.ContainsKey(1));
        }
    }
}