using BookingBoardgamesILoveBan.Src.Chat.DTO;
using BookingBoardgamesILoveBan.Src.Chat.Model;
using BookingBoardgamesILoveBan.Src.Chat.Repository;
using BookingBoardgamesILoveBan.Src.Chat.Service;
using BookingBoardgamesILoveBan.Src.Enum;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using BookingBoardgamesILoveBan.Src.Model;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BookingBoardgamesILoveBan.Tests.Chat
{
    public class ConversationServiceTests
    {
        private readonly Mock<IConversationRepository> _repoMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly ConversationService _service;

        public ConversationServiceTests()
        {
            _repoMock = new Mock<IConversationRepository>();
            _userServiceMock = new Mock<IUserService>();

            _userServiceMock
                .Setup(u => u.GetById(It.IsAny<int>()))
                .Returns((int id) => new User(
                    id,
                    "user" + id,
                    "display",
                    "RO",
                    "Sibiu",
                    "street",
                    "1",
                    "",
                    0
                ));

            _service = new ConversationService(
                _repoMock.Object,
                _userServiceMock.Object,
                1
            );
        }

        [Fact]
        public void FetchConversations_ReturnsEmptyList_WhenRepoEmpty()
        {
            _repoMock.Setup(r => r.GetConversationsForUser(It.IsAny<int>()))
                     .Returns(new List<Conversation>());

            var result = _service.FetchConversations();

            Assert.Empty(result);
        }

        [Fact]
        public void FetchConversations_ReturnsMappedConversations()
        {
            var conv = new Conversation(
                1,
                new int[] { 1, 2 },
                new List<Message>(),
                new Dictionary<int, DateTime>
                {
                    { 1, DateTime.Now },
                    { 2, DateTime.Now }
                }
            );

            _repoMock.Setup(r => r.GetConversationsForUser(1))
                     .Returns(new List<Conversation> { conv });

            var result = _service.FetchConversations();

            Assert.Single(result);
            Assert.Equal(1, result.First().Id);
        }

        [Fact]
        public void SendMessage_CallsRepository()
        {
            var dto = new MessageDTO(
                id: 1,
                conversationId: 1,
                senderId: 1,
                receiverId: 2,
                sentAt: DateTime.Now,
                content: "hello",
                type: MessageType.Text,
                imageUrl: "",
                isResolved: false,
                isAccepted: false,
                isAcceptedByBuyer: false,
                isAcceptedBySeller: false,
                paymentId: -1,
                requestId: -1
            );

            _repoMock.Setup(r => r.HandleNewMessage(It.IsAny<Message>()));

            _service.SendMessage(dto);

            _repoMock.Verify(r => r.HandleNewMessage(It.IsAny<Message>()), Times.Once);
        }

        [Fact]
        public void UpdateMessage_CallsRepository()
        {
            var dto = CreateTextDTO();

            _repoMock.Setup(r => r.HandleMessageUpdate(It.IsAny<Message>()));

            _service.UpdateMessage(dto);

            _repoMock.Verify(r => r.HandleMessageUpdate(It.IsAny<Message>()), Times.Once);
        }

        [Fact]
        public void SendReadReceipt_CallsRepository()
        {
            var convDto = new ConversationDTO(
                1,
                new int[] { 1, 2 },
                new List<MessageDTO>(),
                new Dictionary<int, DateTime>
                {
                    { 1, DateTime.Now },
                    { 2, DateTime.Now }
                }
            );

            _repoMock.Setup(r => r.HandleReadReceipt(It.IsAny<ReadReceipt>()));

            _service.SendReadReceipt(convDto);

            _repoMock.Verify(r => r.HandleReadReceipt(It.IsAny<ReadReceipt>()), Times.Once);
        }

        [Fact]
        public void SendReadReceipt_SelectsOtherParticipantCorrectly()
        {
            var convDto = new ConversationDTO(
                1,
                new int[] { 1, 2 },
                new List<MessageDTO>(),
                new Dictionary<int, DateTime>
                {
                    { 1, DateTime.Now },
                    { 2, DateTime.Now }
                }
            );

            ReadReceipt? captured = null;

            _repoMock
                .Setup(r => r.HandleReadReceipt(It.IsAny<ReadReceipt>()))
                .Callback<ReadReceipt>(r => captured = r);

            _service.SendReadReceipt(convDto);

            Assert.NotNull(captured);
            Assert.Equal(1, captured!.readerId);
            Assert.Equal(2, captured.receiverId);
        }

        [Fact]
        public void MessageToDTO_TextMessage_Works()
        {
            var msg = new TextMessage(1, 1, 1, 2, DateTime.Now, "hello");

            var dto = _service.MessageToMessageDTO(msg);

            Assert.Equal(MessageType.Text, dto.type);
            Assert.Equal("hello", dto.content);
        }

        [Fact]
        public void MessageDTOToMessage_Text_Works()
        {
            var dto = CreateTextDTO();

            var msg = _service.MessageDTOToMessage(dto);

            Assert.IsType<TextMessage>(msg);
        }

        private MessageDTO CreateTextDTO()
        {
            return new MessageDTO(
                id: 1,
                conversationId: 1,
                senderId: 1,
                receiverId: 2,
                sentAt: DateTime.Now,
                content: "hello",
                type: MessageType.Text,
                imageUrl: "",
                isResolved: false,
                isAccepted: false,
                isAcceptedByBuyer: false,
                isAcceptedBySeller: false,
                paymentId: -1,
                requestId: -1
            );
        }

        [Fact]
        public void OnMessageReceived_TriggersEvent()
        {
            var msg = new TextMessage(1, 1, 1, 2, DateTime.Now, "hi");

            bool called = false;

            _service.MessageProcessed += (dto, name) => called = true;

            _service.OnMessageReceived(msg);

            Assert.True(called);
        }

        [Fact]
        public void OnConversationReceived_TriggersEvent()
        {
            var conv = new Conversation(
                1,
                new int[] { 1, 2 },
                new List<Message>(),
                new Dictionary<int, DateTime> { { 1, DateTime.Now }, { 2, DateTime.Now } }
            );

            bool called = false;

            _service.ConversationProcessed += (dto, name) => called = true;

            _service.OnConversationReceived(conv);

            Assert.True(called);
        }

        [Fact]
        public void OnReadReceiptReceived_TriggersEvent()
        {
            var rr = new ReadReceipt(1, 1, 2, DateTime.Now);

            bool called = false;

            _service.ReadReceiptProcessed += dto => called = true;

            _service.OnReadReceiptReceived(rr);

            Assert.True(called);
        }

        [Fact]
        public void OnMessageUpdateReceived_TriggersEvent()
        {
            var msg = new TextMessage(1, 1, 1, 2, DateTime.Now, "hi");

            bool called = false;

            _service.MessageUpdateProcessed += (dto, name) => called = true;

            _service.OnMessageUpdateReceived(msg);

            Assert.True(called);
        }
        [Fact]
        public void OnCardPaymentSelected_CallsFinalizeOnly()
        {
            _repoMock
                .Setup(r => r.HandleRentalRequestFinalization(It.IsAny<int>()));

            _service.OnCardPaymentSelected(10);

            _repoMock.Verify(r =>
                r.HandleRentalRequestFinalization(10),
                Times.Once);
        }


        [Fact]
        public void OnCashPaymentSelected_CallsFinalizeAndCashAgreement()
        {
            _repoMock
                .Setup(r => r.HandleRentalRequestFinalization(It.IsAny<int>()));

            _repoMock
                .Setup(r => r.CreateCashAgreementMessage(It.IsAny<int>(), It.IsAny<int>()));

            _service.OnCashPaymentSelected(10, 99);

            _repoMock.Verify(r =>
                r.HandleRentalRequestFinalization(10),
                Times.Once);

            _repoMock.Verify(r =>
                r.CreateCashAgreementMessage(10, 99),
                Times.Once);
        }

        [Fact]
        public void GetOtherUserName_ReturnsUnknownUser_WhenNull()
        {
            _userServiceMock
                .Setup(u => u.GetById(It.IsAny<int>()))
                .Returns((User)null);

            var dto = new ConversationDTO(
                1,
                new int[] { 1, 2 },
                new List<MessageDTO>(),
                new Dictionary<int, DateTime>
                {
                    { 1, DateTime.Now },
                    { 2, DateTime.Now }
                }
            );

            var result = _service.GetOtherUserNameByConversationDTO(dto);

            Assert.Equal("Unknown User", result);
        }

        [Fact]
        public void MessageToDTO_ImageMessage_Works()
        {
            var msg = new ImageMessage(1, 1, 1, 2, DateTime.Now, "img.png");

            var dto = _service.MessageToMessageDTO(msg);

            Assert.Equal(MessageType.Image, dto.type);
            Assert.Equal("img.png", dto.imageUrl);
        }

        [Fact]
        public void MessageToDTO_CashAgreement_Works()
        {
            var msg = new CashAgreementMessage(
                1, 1, 1, 2,
                55,
                DateTime.Now,
                "cash",
                false,
                true,
                false
            );

            var dto = _service.MessageToMessageDTO(msg);

            Assert.Equal(MessageType.CashAgreement, dto.type);
            Assert.Equal(55, dto.paymentId);
        }

        [Fact]
        public void MessageToDTO_RentalRequest_Works()
        {
            var msg = new RentalRequestMessage(
                1, 1, 1, 2,
                DateTime.Now,
                "rent",
                99,
                false,
                true
            );

            var dto = _service.MessageToMessageDTO(msg);

            Assert.Equal(MessageType.RentalRequest, dto.type);
            Assert.Equal(99, dto.requestId);
        }

        [Fact]
        public void MessageToDTO_SystemMessage_Works()
        {
            var msg = new SystemMessage(1, 1, DateTime.Now, "system");

            var dto = _service.MessageToMessageDTO(msg);

            Assert.Equal(MessageType.System, dto.type);
            Assert.Equal("system", dto.content);
        }

        [Fact]
        public void GetOtherUserNameByMessageDTO_ReturnsCorrectUser()
        {
            var dto = new MessageDTO(
                1, 1, 1, 2, DateTime.Now, "hi",
                MessageType.Text, "", false, false, false, false, -1, -1
            );

            var result = _service.GetOtherUserNameByMessageDTO(dto);

            Assert.Equal("user2", result);
        }

        [Fact]
        public void MessageDTOToMessage_Image_Works()
        {
            var dto = CreateTextDTO() with { type = MessageType.Image, imageUrl = "img.png" };

            var msg = _service.MessageDTOToMessage(dto);

            Assert.IsType<ImageMessage>(msg);
        }

        [Fact]
        public void MessageDTOToMessage_Rental_Works()
        {
            var dto = CreateTextDTO() with { type = MessageType.RentalRequest };

            var msg = _service.MessageDTOToMessage(dto);

            Assert.IsType<RentalRequestMessage>(msg);
        }

        [Fact]
        public void MessageDTOToMessage_Cash_Works()
        {
            var dto = CreateTextDTO() with { type = MessageType.CashAgreement };

            var msg = _service.MessageDTOToMessage(dto);

            Assert.IsType<CashAgreementMessage>(msg);
        }

        [Fact]
        public void MessageDTOToMessage_System_Works()
        {
            var dto = CreateTextDTO() with { type = MessageType.System};

            var msg = _service.MessageDTOToMessage(dto);

            Assert.IsType<SystemMessage>(msg);
        }

        [Fact]
        public void ConversationToConversationDTO_MapsCorrectly()
        {
            var conv = new Conversation(
                1,
                new[] { 1, 2 },
                new List<Message> { new TextMessage(1, 1, 1, 2, DateTime.Now, "hi") },
                new Dictionary<int, DateTime>
                {
                    { 1, DateTime.Now },
                    { 2, DateTime.Now }
                }
            );

            var dto = _service.ConversationToConversationDTO(conv);

            Assert.Equal(1, dto.Id);
            Assert.Single(dto.MessageList);
        }

        [Fact]
        public void ReadReceiptToReadReceiptDTO_MapsCorrectly()
        {
            var rr = new ReadReceipt(1, 1, 2, DateTime.Now);

            var dto = _service.ReadReceiptToReadReceiptDTO(rr);

            Assert.Equal(1, dto.conversationId);
            Assert.Equal(1, dto.readerId);
            Assert.Equal(2, dto.receiverId);
        }

        [Fact]
        public void FetchConversations_MultipleConversations()
        {
            var convs = new List<Conversation>
            {
                new Conversation(1, new[] {1,2}, new List<Message>(), new()),
                new Conversation(2, new[] {1,3}, new List<Message>(), new())
            };

            _repoMock.Setup(r => r.GetConversationsForUser(1)).Returns(convs);

            var result = _service.FetchConversations();

            Assert.Equal(2, result.Count);
        }
    }
}