using System;
using System.Collections.Generic;
using System.Linq;
using BookingBoardgamesILoveBan.Src.Chat.DTO;
using BookingBoardgamesILoveBan.Src.Chat.Model;
using BookingBoardgamesILoveBan.Src.Chat.Repository;
using BookingBoardgamesILoveBan.Src.Chat.Service;
using BookingBoardgamesILoveBan.Src.Enum;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using BookingBoardgamesILoveBan.Src.Model;
using Moq;
using Xunit;

namespace BookingBoardgamesILoveBan.Tests.Chat
{
    public class ConversationServiceTests
    {
        private readonly Mock<IConversationRepository> repoMock;
        private readonly Mock<IUserRepository> userServiceMock;
        private readonly ConversationService service;

        public ConversationServiceTests()
        {
            repoMock = new Mock<IConversationRepository>();
            userServiceMock = new Mock<IUserRepository>();

            userServiceMock
                .Setup(u => u.GetById(It.IsAny<int>()))
                .Returns((int id) => new User(
                    id,
                    "user" + id,
                    "display",
                    "RO",
                    "Sibiu",
                    "street",
                    "1",
                    string.Empty,
                    0));

            service = new ConversationService(
                repoMock.Object,
                1,
                userServiceMock.Object);
        }

        [Fact]
        public void FetchConversations_ReturnsEmptyList_WhenRepoEmpty()
        {
            repoMock.Setup(r => r.GetConversationsForUser(It.IsAny<int>()))
                     .Returns(new List<Conversation>());

            var result = service.FetchConversations();

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
                });

            repoMock.Setup(r => r.GetConversationsForUser(1))
                     .Returns(new List<Conversation> { conv });

            var result = service.FetchConversations();

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
                type: MessageType.MessageText,
                imageUrl: string.Empty,
                isResolved: false,
                isAccepted: false,
                isAcceptedByBuyer: false,
                isAcceptedBySeller: false,
                paymentId: -1,
                requestId: -1);

            repoMock.Setup(r => r.HandleNewMessage(It.IsAny<Message>()));

            service.SendMessage(dto);

            repoMock.Verify(r => r.HandleNewMessage(It.IsAny<Message>()), Times.Once);
        }

        [Fact]
        public void UpdateMessage_CallsRepository()
        {
            var dto = CreateTextDTO();

            repoMock.Setup(r => r.HandleMessageUpdate(It.IsAny<Message>()));

            service.UpdateMessage(dto);

            repoMock.Verify(r => r.HandleMessageUpdate(It.IsAny<Message>()), Times.Once);
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
                });

            repoMock.Setup(r => r.HandleReadReceipt(It.IsAny<ReadReceipt>()));

            service.SendReadReceipt(convDto);

            repoMock.Verify(r => r.HandleReadReceipt(It.IsAny<ReadReceipt>()), Times.Once);
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
                });

            ReadReceipt? captured = null;

            repoMock
                .Setup(r => r.HandleReadReceipt(It.IsAny<ReadReceipt>()))
                .Callback<ReadReceipt>(r => captured = r);

            service.SendReadReceipt(convDto);

            Assert.NotNull(captured);
            Assert.Equal(1, captured!.messageReaderId);
            Assert.Equal(2, captured.messageReceiverId);
        }

        [Fact]
        public void MessageToDTO_TextMessage_Works()
        {
            var msg = new TextMessage(1, 1, 1, 2, DateTime.Now, "hello");

            var dto = service.MessageToMessageDTO(msg);

            Assert.Equal(MessageType.MessageText, dto.type);
            Assert.Equal("hello", dto.content);
        }

        [Fact]
        public void MessageDTOToMessage_Text_Works()
        {
            var dto = CreateTextDTO();

            var msg = service.MessageDTOToMessage(dto);

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
                type: MessageType.MessageText,
                imageUrl: string.Empty,
                isResolved: false,
                isAccepted: false,
                isAcceptedByBuyer: false,
                isAcceptedBySeller: false,
                paymentId: -1,
                requestId: -1);
        }

        [Fact]
        public void OnMessageReceived_TriggersEvent()
        {
            var msg = new TextMessage(1, 1, 1, 2, DateTime.Now, "hi");

            bool called = false;

            service.ActionMessageProcessed += (dto, name) => called = true;

            service.OnMessageReceived(msg);

            Assert.True(called);
        }

        [Fact]
        public void OnConversationReceived_TriggersEvent()
        {
            var conv = new Conversation(
                1,
                new int[] { 1, 2 },
                new List<Message>(),
                new Dictionary<int, DateTime> { { 1, DateTime.Now }, { 2, DateTime.Now } });

            bool called = false;

            service.ActionConversationProcessed += (dto, name) => called = true;

            service.OnConversationReceived(conv);

            Assert.True(called);
        }

        [Fact]
        public void OnReadReceiptReceived_TriggersEvent()
        {
            var rr = new ReadReceipt(1, 1, 2, DateTime.Now);

            bool called = false;

            service.ActionReadReceiptProcessed += dto => called = true;

            service.OnReadReceiptReceived(rr);

            Assert.True(called);
        }

        [Fact]
        public void OnMessageUpdateReceived_TriggersEvent()
        {
            var msg = new TextMessage(1, 1, 1, 2, DateTime.Now, "hi");

            bool called = false;

            service.ActionMessageUpdateProcessed += (dto, name) => called = true;

            service.OnMessageUpdateReceived(msg);

            Assert.True(called);
        }
        [Fact]
        public void OnCardPaymentSelected_CallsFinalizeOnly()
        {
            repoMock
                .Setup(r => r.HandleRentalRequestFinalization(It.IsAny<int>()));

            service.OnCardPaymentSelected(10);

            repoMock.Verify(r =>
                r.HandleRentalRequestFinalization(10),
                Times.Once);
        }

        [Fact]
        public void OnCashPaymentSelected_CallsFinalizeAndCashAgreement()
        {
            repoMock
                .Setup(r => r.HandleRentalRequestFinalization(It.IsAny<int>()));

            repoMock
                .Setup(r => r.CreateCashAgreementMessage(It.IsAny<int>(), It.IsAny<int>()));

            service.OnCashPaymentSelected(10, 99);

            repoMock.Verify(r =>
                r.HandleRentalRequestFinalization(10),
                Times.Once);

            repoMock.Verify(r =>
                r.CreateCashAgreementMessage(10, 99),
                Times.Once);
        }

        [Fact]
        public void GetOtherUserName_ReturnsUnknownUser_WhenNull()
        {
            userServiceMock
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
                });

            var result = service.GetOtherUserNameByConversationDTO(dto);

            Assert.Equal("Unknown User", result);
        }

        [Fact]
        public void MessageToDTO_ImageMessage_Works()
        {
            var msg = new ImageMessage(1, 1, 1, 2, DateTime.Now, "img.png");

            var dto = service.MessageToMessageDTO(msg);

            Assert.Equal(MessageType.MessageImage, dto.type);
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
                false);

            var dto = service.MessageToMessageDTO(msg);

            Assert.Equal(MessageType.MessageCashAgreement, dto.type);
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
                true);

            var dto = service.MessageToMessageDTO(msg);

            Assert.Equal(MessageType.MessageRentalRequest, dto.type);
            Assert.Equal(99, dto.requestId);
        }

        [Fact]
        public void MessageToDTO_SystemMessage_Works()
        {
            var msg = new SystemMessage(1, 1, DateTime.Now, "system");

            var dto = service.MessageToMessageDTO(msg);

            Assert.Equal(MessageType.MessageSystem, dto.type);
            Assert.Equal("system", dto.content);
        }
    }
}