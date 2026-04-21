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
        private int[] conversationParticaipantsId = { 1, 2 };
        public ConversationServiceTests()
        {
            repositoryMock = new Mock<IConversationRepository>();
            userServiceMock = new Mock<IUserRepository>();

            userServiceMock
                .Setup(user => user.GetById(It.IsAny<int>()))
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
        public void FetchConversations_RepositoryReturnsEmptyList_ReturnsEmptyList()
        {
            repositoryMock.Setup(repository => repository.GetConversationsForUser(It.IsAny<int>()))
                     .Returns(new List<Conversation>());

            var result = service.FetchConversations();

            Assert.Empty(result);
        }

        [Fact]
        public void FetchConversations_repositoryReturnsSingleConversation_returnsSingleConversationDto()
        {
            int conversationId = 1;

            var conversation= new Conversation(
                conversationId,
                conversationParticaipantsId,
                new List<Message>(),
                new Dictionary<int, DateTime>
                {
                    { conversationParticaipantsId[0], DateTime.Now },
                    { conversationParticaipantsId[1], DateTime.Now }
                });

            repositoryMock.Setup(repository => repository.GetConversationsForUser(1))
                     .Returns(new List<Conversation> { conversation });

            var result = service.FetchConversations();

            Assert.Single(result);
            Assert.Equal(1, result.First().Id);
        }

        [Fact]
        public void SendMessage_validMessageDto_callsHandleNewMessageOnce()
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

            repositoryMock.Setup(repository => repository.HandleNewMessage(It.IsAny<Message>()));

            service.SendMessage(dto);

            repositoryMock.Verify(repository => repository.HandleNewMessage(It.IsAny<Message>()), Times.Once);
        }

        [Fact]
        public void UpdateMessage_validMessageDto_callsHandleMessageUpdateOnce()
        {
            var dto = CreateTextDTO();

            repositoryMock.Setup(repository => repository.HandleMessageUpdate(It.IsAny<Message>()));

            service.UpdateMessage(dto);

            repositoryMock.Verify(repository => repository.HandleMessageUpdate(It.IsAny<Message>()), Times.Once);
        }

        [Fact]
        public void SendReadReceipt_validConversationDto_callsHandleReadReceiptOnce()
        {
            int conversationId = 1;
            
            var conversationDto = new ConversationDTO(
                conversationId,
                conversationParticaipantsId,
                new List<MessageDTO>(),
                new Dictionary<int, DateTime>
                {
                    { conversationParticaipantsId[0], DateTime.Now },
                    { conversationParticaipantsId[1], DateTime.Now }
                });

            repositoryMock.Setup(repository => repository.HandleReadReceipt(It.IsAny<ReadReceipt>()));

            service.SendReadReceipt(conversationDto);

            repositoryMock.Verify(repository => repository.HandleReadReceipt(It.IsAny<ReadReceipt>()), Times.Once);
        }

        [Fact]
        public void SendReadReceipt_conversationWithTwoParticipants_createsReadReceiptWithCorrectReaderAndReceiver()
        {
            int conversationId = 1;
            var conversationDto = new ConversationDTO(
                conversationId,
                conversationParticaipantsId,
                new List<MessageDTO>(),
                new Dictionary<int, DateTime>
                {
                    { conversationParticaipantsId[0], DateTime.Now },
                    { conversationParticaipantsId[1], DateTime.Now }
                });

            ReadReceipt? captured = null;

            repositoryMock
                .Setup(repository => repository.HandleReadReceipt(It.IsAny<ReadReceipt>()))
                .Callback<ReadReceipt>(receipt => captured = receipt);

            service.SendReadReceipt(conversationDto);

            Assert.Equal(1, captured!.messageReaderId);
            Assert.Equal(2, captured.messageReceiverId);
        }

        [Fact]
        public void MessageToMessageDTO_textMessage_returnsTextMessageType()
        {
            int id = 1, conversationId = 1;
            var message = new TextMessage(id, conversationId, conversationParticaipantsId[0], conversationParticaipantsId[1], DateTime.Now, "hello");

            var dto = service.MessageToMessageDTO(message);

            Assert.Equal(MessageType.MessageText, dto.type);
        }

        [Fact]
        public void MessageDTOToMessage_textMessageType_returnsTextMessage()
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
        public void OnMessageReceived_validMessage_invokesMessageProcessedEvent()
        {
            int id = 1, conversationId = 1;
            var message = new TextMessage(id, conversationId, conversationParticaipantsId[0], conversationParticaipantsId[1], DateTime.Now, "hi");

            bool called = false;

            service.ActionMessageProcessed += (dto, name) => called = true;

            service.OnMessageReceived(message);

            Assert.True(called);
        }

        [Fact]
        public void OnConversationReceived_validConversation_invokesConversationProcessedEvent()
        {
            int conversationId = 1;
            var conversation= new Conversation(
                conversationId,
                conversationParticaipantsId,
                new List<Message>(),
                new Dictionary<int, DateTime> {
                    { conversationParticaipantsId[0], DateTime.Now },
                    { conversationParticaipantsId[1], DateTime.Now }
                });

            bool called = false;

            service.ActionConversationProcessed += (dto, name) => called = true;

            service.OnConversationReceived(conversation);

            Assert.True(called);
        }

        [Fact]
        public void OnReadReceiptReceived_validReadReceipt_invokesReadReceiptProcessedEvent()
        {
            int conversationId = 1;
            var readReceipt = new ReadReceipt(conversationId, conversationParticaipantsId[0], conversationParticaipantsId[1], DateTime.Now);

            bool called = false;

            service.ActionReadReceiptProcessed += dto => called = true;

            service.OnReadReceiptReceived(readReceipt);

            Assert.True(called);
        }

        [Fact]
        public void OnMessageUpdateReceived_messageUpdate_triggersUpdateEvent()
        {
            int id = 1, conversationId = 1;
            var message = new TextMessage(id, conversationId, conversationParticaipantsId[0], conversationParticaipantsId[1], DateTime.Now, "hi");

            bool called = false;

            service.ActionMessageUpdateProcessed += (dto, name) => called = true;

            service.OnMessageUpdateReceived(message);

            Assert.True(called);
        }
        [Fact]
        public void OnCardPaymentSelected_ValidRentalRequest_CallsFinalizeOnly()
        {
            repositoryMock
                .Setup(repository => repository.HandleRentalRequestFinalization(It.IsAny<int>()));

            service.OnCardPaymentSelected(10);

            repositoryMock.Verify(repository =>
                repository.HandleRentalRequestFinalization(10),
                Times.Once);
        }

        [Fact]
        public void OnCashPaymentSelected_whenValidRentalRequestAndPaymentId_finalizeRentalRequestAndCreateCashAgreementMessage()
        {
            repositoryMock
                .Setup(repository => repository.HandleRentalRequestFinalization(It.IsAny<int>()));

            repositoryMock
                .Setup(repository => repository.CreateCashAgreementMessage(It.IsAny<int>(), It.IsAny<int>()));

            service.OnCashPaymentSelected(10, 99);

            repositoryMock.Verify(repository =>
                repository.HandleRentalRequestFinalization(10),
                Times.Once);

            repositoryMock.Verify(repository =>
                repository.CreateCashAgreementMessage(10, 99),
                Times.Once);
        }

        [Fact]
        public void GetOtherUserNameByConversationDTO_whenUserServiceReturnsNull_returnsUnknownUser()
        {
            userServiceMock
                .Setup(repository => repository.GetById(It.IsAny<int>()))
                .Returns((User)null);
            int conversationId = 1;
            var dto = new ConversationDTO(
                conversationId,
                conversationParticaipantsId,
                new List<MessageDTO>(),
                new Dictionary<int, DateTime>
                {
                    { conversationParticaipantsId[0], DateTime.Now },
                    { conversationParticaipantsId[1], DateTime.Now }
                });

            var result = service.GetOtherUserNameByConversationDTO(dto);

            Assert.Equal("Unknown User", result);
        }

        [Fact]
        public void MessageToMessageDTO_whenImageMessageIsProvided_returnsDtoWithImageType()
        {
            int id = 1, conversationId = 1;
            var message = new ImageMessage(id, conversationId, conversationParticaipantsId[1], conversationParticaipantsId[0], DateTime.Now, "img.png");

            var dto = service.MessageToMessageDTO(message);

            Assert.Equal(MessageType.MessageImage, dto.type);
        }

        [Fact]
        public void MessageToMessageDTO_whenCashAgreementMessageIsProvided_returnsDtoWithCashAgreementTypeAndPaymentId()
        {
            int id = 1, conversationId = 1;
            int paymentId = 55;
            var message = new CashAgreementMessage(
                id, conversationId, conversationParticaipantsId[0], conversationParticaipantsId[1],
                paymentId,
                DateTime.Now,
                "cash",
                false,
                true,
                false);

            var dto = service.MessageToMessageDTO(message);

            Assert.Equal(MessageType.MessageCashAgreement, dto.type);
            Assert.Equal(paymentId, dto.paymentId);
        }

        [Fact]
        public void MessageToMessageDTO_whenRentalRequestMessageIsProvided_returnsDtoWithRentalRequestTypeAndRequestId()
        {
            int id = 1, conversationId = 1;
            var message = new RentalRequestMessage(
                id, conversationId, conversationParticaipantsId[0], conversationParticaipantsId[1],
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
        public void MessageToMessageDTO_whenSystemMessageIsProvided_returnsDtoWithSystemTypeAndContent()
        {
            var message = new SystemMessage(1, 1, DateTime.Now, "system");

            var dto = service.MessageToMessageDTO(message);

            Assert.Equal(MessageType.MessageSystem, dto.type);
            Assert.Equal("system", dto.content);
        }

        [Fact]
        public void GetOtherUserNameByMessageDTO_whenMessageIsSentByCurrentUser_returnsReceiverUserName()
        {
            int id = 1, conversationId = 1, requestId = -1, paymentId = -1;

            var dto = new MessageDTO(
                id, conversationId, conversationParticaipantsId[0], conversationParticaipantsId[1], DateTime.Now, "hi",
                MessageType.MessageText, "", false, false, false, false, requestId, paymentId
            );

            var result = service.GetOtherUserNameByMessageDTO(dto);

            Assert.Equal("user2", result);
        }

        [Fact]
        public void MessageDTOToMessage_whenDtoTypeIsImage_returnsImageMessage()
        {
            var dto = CreateTextDTO() with { type = MessageType.MessageImage, imageUrl = "img.png" };

            var message = service.MessageDTOToMessage(dto);

            Assert.IsType<ImageMessage>(message);
        }

        [Fact]
        public void MessageDTOToMessage_whenDtoTypeIsRentalRequest_returnsRentalRequestMessage()
        {
            var dto = CreateTextDTO() with { type = MessageType.MessageRentalRequest };

            var message = service.MessageDTOToMessage(dto);

            Assert.IsType<RentalRequestMessage>(message);
        }

        [Fact]
        public void MessageDTOToMessage_whenDtoTypeIsCashAgreement_returnsCashAgreementMessage()
        {
            var dto = CreateTextDTO() with { type = MessageType.MessageCashAgreement };

            var message = service.MessageDTOToMessage(dto);

            Assert.IsType<CashAgreementMessage>(message);
        }

        [Fact]
        public void MessageDTOToMessage_whenDtoTypeIsSystem_returnsSystemMessage()
        {
            var dto = CreateTextDTO() with { type = MessageType.MessageSystem };

            var message = service.MessageDTOToMessage(dto);

            Assert.IsType<SystemMessage>(message);
        }

        [Fact]
        public void ConversationToConversationDTO_whenValidConversationIsProvided_returnsMappedConversationDTO()
        {
            int id = 1, conversationId = 1;
            var conversation= new Conversation(
                conversationId,
                conversationParticaipantsId,
                new List<Message> { new TextMessage(id, conversationId, conversationParticaipantsId[0], conversationParticaipantsId[1], DateTime.Now, "hi") },
                new Dictionary<int, DateTime>
                {
                    { conversationParticaipantsId[0], DateTime.Now },
                    { conversationParticaipantsId[1], DateTime.Now }
                }
            );

            var dto = service.ConversationToConversationDTO(conversation);

            Assert.Equal(1, dto.Id);
            Assert.Single(dto.MessageList);
        }

        [Fact]
        public void ReadReceiptToReadReceiptDTO_WhenValidReadReceiptIsProvided_ReturnsDtoWithSameValues()
        {
            int conversationId = 1;
            var readReceipt = new ReadReceipt(conversationId, conversationParticaipantsId[0], conversationParticaipantsId[1], DateTime.Now);

            var dto = service.ReadReceiptToReadReceiptDTO(readReceipt);

            Assert.Equal(1, dto.conversationId);
            Assert.Equal(1, dto.readerId);
            Assert.Equal(2, dto.receiverId);
        }

        [Fact]
        public void FetchConversations_WhenRepositoryReturnsMultipleConversations_ReturnsAllConversations()
        {

            var conversation = new List<Conversation>
            {
                new Conversation(1, conversationParticaipantsId, new List<Message>(), new()),
                new Conversation(2, new[] {1,3}, new List<Message>(), new())
            };

            repositoryMock.Setup(repository => repository.GetConversationsForUser(1)).Returns(conversation);

            var result = service.FetchConversations();

            Assert.Equal(2, result.Count);
        }
    }
}