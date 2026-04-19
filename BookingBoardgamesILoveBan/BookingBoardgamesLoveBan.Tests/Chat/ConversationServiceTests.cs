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
        private readonly Mock<IConversationRepository> repositoryMock;
        private readonly Mock<IUserRepository> userServiceMock;
        private readonly ConversationService service;

        public ConversationServiceTests()
        {
            repositoryMock = new Mock<IConversationRepository>();
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
                repositoryMock.Object,
                1,
                userServiceMock.Object);
        }

        [Fact]
        public void FetchConversations_ReturnsEmptyList_WhenRepoEmpty()
        {
            repositoryMock.Setup(r => r.GetConversationsForUser(It.IsAny<int>()))
                     .Returns(new List<Conversation>());

            var result = service.FetchConversations();

            Assert.Empty(result);
        }

        [Fact]
        public void FetchConversations_ReturnsMappedConversations()
        {
            var conversation= new Conversation(
                1,
                new int[] { 1, 2 },
                new List<Message>(),
                new Dictionary<int, DateTime>
                {
                    { 1, DateTime.Now },
                    { 2, DateTime.Now }
                });

            repositoryMock.Setup(r => r.GetConversationsForUser(1))
                     .Returns(new List<Conversation> { conversation });

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

            repositoryMock.Setup(r => r.HandleNewMessage(It.IsAny<Message>()));

            service.SendMessage(dto);

            repositoryMock.Verify(r => r.HandleNewMessage(It.IsAny<Message>()), Times.Once);
        }

        [Fact]
        public void UpdateMessage_CallsRepository()
        {
            var dto = CreateTextDTO();

            repositoryMock.Setup(r => r.HandleMessageUpdate(It.IsAny<Message>()));

            service.UpdateMessage(dto);

            repositoryMock.Verify(r => r.HandleMessageUpdate(It.IsAny<Message>()), Times.Once);
        }

        [Fact]
        public void SendReadReceipt_CallsRepository()
        {
            var conversationDto = new ConversationDTO(
                1,
                new int[] { 1, 2 },
                new List<MessageDTO>(),
                new Dictionary<int, DateTime>
                {
                    { 1, DateTime.Now },
                    { 2, DateTime.Now }
                });

            repositoryMock.Setup(r => r.HandleReadReceipt(It.IsAny<ReadReceipt>()));

            service.SendReadReceipt(conversationDto);

            repositoryMock.Verify(r => r.HandleReadReceipt(It.IsAny<ReadReceipt>()), Times.Once);
        }

        [Fact]
        public void SendReadReceipt_SelectsOtherParticipantCorrectly()
        {
            var conversationDto = new ConversationDTO(
                1,
                new int[] { 1, 2 },
                new List<MessageDTO>(),
                new Dictionary<int, DateTime>
                {
                    { 1, DateTime.Now },
                    { 2, DateTime.Now }
                });

            ReadReceipt? captured = null;

            repositoryMock
                .Setup(r => r.HandleReadReceipt(It.IsAny<ReadReceipt>()))
                .Callback<ReadReceipt>(r => captured = r);

            service.SendReadReceipt(conversationDto);

            Assert.Equal(1, captured!.messageReaderId);
            Assert.Equal(2, captured.messageReceiverId);
        }

        [Fact]
        public void MessageToDTO_TextMessage_Works()
        {
            var message = new TextMessage(1, 1, 1, 2, DateTime.Now, "hello");

            var dto = service.MessageToMessageDTO(message);

            Assert.Equal(MessageType.MessageText, dto.type);
            Assert.Equal("hello", dto.content);
        }

        [Fact]
        public void MessageDTOToMessage_Text_Works()
        {
            var dto = CreateTextDTO();

            var message = service.MessageDTOToMessage(dto);

            Assert.IsType<TextMessage>(message);
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
            var message = new TextMessage(1, 1, 1, 2, DateTime.Now, "hi");

            bool called = false;

            service.ActionMessageProcessed += (dto, name) => called = true;

            service.OnMessageReceived(message);

            Assert.True(called);
        }

        [Fact]
        public void OnConversationReceived_TriggersEvent()
        {
            var conversation= new Conversation(
                1,
                new int[] { 1, 2 },
                new List<Message>(),
                new Dictionary<int, DateTime> { { 1, DateTime.Now }, { 2, DateTime.Now } });

            bool called = false;

            service.ActionConversationProcessed += (dto, name) => called = true;

            service.OnConversationReceived(conversation);

            Assert.True(called);
        }

        [Fact]
        public void OnReadReceiptReceived_TriggersEvent()
        {
            var readReceipt = new ReadReceipt(1, 1, 2, DateTime.Now);

            bool called = false;

            service.ActionReadReceiptProcessed += dto => called = true;

            service.OnReadReceiptReceived(readReceipt);

            Assert.True(called);
        }

        [Fact]
        public void OnMessageUpdateReceived_TriggersEvent()
        {
            var message = new TextMessage(1, 1, 1, 2, DateTime.Now, "hi");

            bool called = false;

            service.ActionMessageUpdateProcessed += (dto, name) => called = true;

            service.OnMessageUpdateReceived(message);

            Assert.True(called);
        }
        [Fact]
        public void OnCardPaymentSelected_CallsFinalizeOnly()
        {
            repositoryMock
                .Setup(r => r.HandleRentalRequestFinalization(It.IsAny<int>()));

            service.OnCardPaymentSelected(10);

            repositoryMock.Verify(r =>
                r.HandleRentalRequestFinalization(10),
                Times.Once);
        }

        [Fact]
        public void OnCashPaymentSelected_CallsFinalizeAndCashAgreement()
        {
            repositoryMock
                .Setup(r => r.HandleRentalRequestFinalization(It.IsAny<int>()));

            repositoryMock
                .Setup(r => r.CreateCashAgreementMessage(It.IsAny<int>(), It.IsAny<int>()));

            service.OnCashPaymentSelected(10, 99);

            repositoryMock.Verify(r =>
                r.HandleRentalRequestFinalization(10),
                Times.Once);

            repositoryMock.Verify(r =>
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
            var message = new ImageMessage(1, 1, 1, 2, DateTime.Now, "img.png");

            var dto = service.MessageToMessageDTO(message);

            Assert.Equal(MessageType.MessageImage, dto.type);
            Assert.Equal("img.png", dto.imageUrl);
        }

        [Fact]
        public void MessageToDTO_CashAgreement_Works()
        {
            var message = new CashAgreementMessage(
                1, 1, 1, 2,
                55,
                DateTime.Now,
                "cash",
                false,
                true,
                false);

            var dto = service.MessageToMessageDTO(message);

            Assert.Equal(MessageType.MessageCashAgreement, dto.type);
            Assert.Equal(55, dto.paymentId);
        }

        [Fact]
        public void MessageToDTO_RentalRequest_Works()
        {
            var message = new RentalRequestMessage(
                1, 1, 1, 2,
                DateTime.Now,
                "rent",
                99,
                false,
                true);

            var dto = service.MessageToMessageDTO(message);

            Assert.Equal(MessageType.MessageRentalRequest, dto.type);
            Assert.Equal(99, dto.requestId);
        }

        [Fact]
        public void MessageToDTO_SystemMessage_Works()
        {
            var message = new SystemMessage(1, 1, DateTime.Now, "system");

            var dto = service.MessageToMessageDTO(message);

            Assert.Equal(MessageType.MessageSystem, dto.type);
            Assert.Equal("system", dto.content);
        }

        [Fact]
        public void GetOtherUserNameByMessageDTO_ReturnsCorrectUser()
        {
            var dto = new MessageDTO(
                1, 1, 1, 2, DateTime.Now, "hi",
                MessageType.MessageText, "", false, false, false, false, -1, -1
            );

            var result = service.GetOtherUserNameByMessageDTO(dto);

            Assert.Equal("user2", result);
        }

        [Fact]
        public void MessageDTOToMessage_Image_Works()
        {
            var dto = CreateTextDTO() with { type = MessageType.MessageImage, imageUrl = "img.png" };

            var message = service.MessageDTOToMessage(dto);

            Assert.IsType<ImageMessage>(message);
        }

        [Fact]
        public void MessageDTOToMessage_Rental_Works()
        {
            var dto = CreateTextDTO() with { type = MessageType.MessageRentalRequest };

            var message = service.MessageDTOToMessage(dto);

            Assert.IsType<RentalRequestMessage>(message);
        }

        [Fact]
        public void MessageDTOToMessage_Cash_Works()
        {
            var dto = CreateTextDTO() with { type = MessageType.MessageCashAgreement };

            var message = service.MessageDTOToMessage(dto);

            Assert.IsType<CashAgreementMessage>(message);
        }

        [Fact]
        public void MessageDTOToMessage_System_Works()
        {
            var dto = CreateTextDTO() with { type = MessageType.MessageSystem };

            var message = service.MessageDTOToMessage(dto);

            Assert.IsType<SystemMessage>(message);
        }

        [Fact]
        public void ConversationToConversationDTO_MapsCorrectly()
        {
            var conversation= new Conversation(
                1,
                new[] { 1, 2 },
                new List<Message> { new TextMessage(1, 1, 1, 2, DateTime.Now, "hi") },
                new Dictionary<int, DateTime>
                {
                    { 1, DateTime.Now },
                    { 2, DateTime.Now }
                }
            );

            var dto = service.ConversationToConversationDTO(conversation);

            Assert.Equal(1, dto.Id);
            Assert.Single(dto.MessageList);
        }

        [Fact]
        public void ReadReceiptToReadReceiptDTO_MapsCorrectly()
        {
            var readReceipt = new ReadReceipt(1, 1, 2, DateTime.Now);

            var dto = service.ReadReceiptToReadReceiptDTO(readReceipt);

            Assert.Equal(1, dto.conversationId);
            Assert.Equal(1, dto.readerId);
            Assert.Equal(2, dto.receiverId);
        }

        [Fact]
        public void FetchConversations_MultipleConversations()
        {
            var conversation = new List<Conversation>
            {
                new Conversation(1, new[] {1,2}, new List<Message>(), new()),
                new Conversation(2, new[] {1,3}, new List<Message>(), new())
            };

            repositoryMock.Setup(r => r.GetConversationsForUser(1)).Returns(conversation);

            var result = service.FetchConversations();

            Assert.Equal(2, result.Count);
        }
    }
}