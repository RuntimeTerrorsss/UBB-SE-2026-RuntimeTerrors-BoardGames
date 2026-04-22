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
        private readonly Mock<IConversationRepository> conversationRepositoryMock;
        private readonly Mock<IUserRepository> userRepositoryMock;
        private readonly ConversationService conversationService;

        public ConversationServiceTests()
        {
            int currentUserId = 1;
            int defaultBalance = 0;
            string testDisplayName = "display";
            string testCountry = "RO";
            string testCity = "Sibiu";
            string testStreet = "street";
            string testStreetNumber = "1";

            conversationRepositoryMock = new Mock<IConversationRepository>();
            userRepositoryMock = new Mock<IUserRepository>();

            userRepositoryMock
                .Setup(userRepository => userRepository.GetById(It.IsAny<int>()))
                .Returns((int userIdentifier) => new User(
                    userIdentifier,
                    "user" + userIdentifier,
                    testDisplayName,
                    testCountry,
                    testCity,
                    testStreet,
                    testStreetNumber,
                    string.Empty,
                    defaultBalance));

            conversationService = new ConversationService(
                conversationRepositoryMock.Object,
                currentUserId,
                userRepositoryMock.Object);
        }

        private MessageDataTransferObject CreateTextDTO()
        {
            int defaultMessageId = 1;
            int targetConversationId = 1;
            int senderIdentifier = 1;
            int receiverIdentifier = 2;
            int missingIdentifier = -1;
            string textContent = "hello";

            return new MessageDataTransferObject(
                defaultMessageId,
                targetConversationId,
                senderIdentifier,
                receiverIdentifier,
                DateTime.Now,
                textContent,
                MessageType.MessageText,
                string.Empty,
                false,
                false,
                false,
                false,
                missingIdentifier,
                missingIdentifier);
        }

        [Fact]
        public void FetchConversations_EmptyRepository_ReturnsEmptyList()
        {
            conversationRepositoryMock.Setup(repository => repository.GetConversationsForUser(It.IsAny<int>()))
                     .Returns(new List<Conversation>());

            var resultList = conversationService.FetchConversations();

            Assert.Empty(resultList);
        }

        [Fact]
        public void FetchConversations_ValidRepository_ReturnsMappedConversations()
        {
            int targetConversationId = 1;
            int firstParticipantId = 1;
            int secondParticipantId = 2;

            var testConversation = new Conversation(
                targetConversationId,
                new int[] { firstParticipantId, secondParticipantId },
                new List<Message>(),
                new Dictionary<int, DateTime>
                {
                    { firstParticipantId, DateTime.Now },
                    { secondParticipantId, DateTime.Now }
                });

            conversationRepositoryMock.Setup(repository => repository.GetConversationsForUser(firstParticipantId))
                     .Returns(new List<Conversation> { testConversation });

            var resultList = conversationService.FetchConversations();

            Assert.Single(resultList);
            Assert.Equal(targetConversationId, resultList.First().Id);
        }

        [Fact]
        public void SendMessage_ValidInput_CallsRepositoryHandleNewMessage()
        {
            int defaultMessageId = 1;
            int targetConversationId = 1;
            int senderIdentifier = 1;
            int receiverIdentifier = 2;
            int missingIdentifier = -1;
            string textContent = "hello";

            var messageDataTransferObject = new MessageDataTransferObject(
                defaultMessageId,
                targetConversationId,
                senderIdentifier,
                receiverIdentifier,
                DateTime.Now,
                textContent,
                MessageType.MessageText,
                string.Empty,
                false,
                false,
                false,
                false,
                missingIdentifier,
                missingIdentifier);

            conversationRepositoryMock.Setup(repository => repository.HandleNewMessage(It.IsAny<Message>()));

            conversationService.SendMessage(messageDataTransferObject);

            conversationRepositoryMock.Verify(repository => repository.HandleNewMessage(It.IsAny<Message>()), Times.Once);
        }

        [Fact]
        public void UpdateMessage_ValidInput_CallsRepositoryHandleMessageUpdate()
        {
            var messageDataTransferObject = CreateTextDTO();

            conversationRepositoryMock.Setup(repository => repository.HandleMessageUpdate(It.IsAny<Message>()));

            conversationService.UpdateMessage(messageDataTransferObject);

            conversationRepositoryMock.Verify(repository => repository.HandleMessageUpdate(It.IsAny<Message>()), Times.Once);
        }

        [Fact]
        public void SendReadReceipt_ValidConversation_CallsRepositoryHandleReadReceipt()
        {
            int targetConversationId = 1;
            int firstParticipantId = 1;
            int secondParticipantId = 2;

            var conversationDataTransferObject = new ConversationDataTransferObject(
                targetConversationId,
                new int[] { firstParticipantId, secondParticipantId },
                new List<MessageDataTransferObject>(),
                new Dictionary<int, DateTime>
                {
                    { firstParticipantId, DateTime.Now },
                    { secondParticipantId, DateTime.Now }
                });

            conversationRepositoryMock.Setup(repository => repository.HandleReadReceipt(It.IsAny<ReadReceipt>()));

            conversationService.SendReadReceipt(conversationDataTransferObject);

            conversationRepositoryMock.Verify(repository => repository.HandleReadReceipt(It.IsAny<ReadReceipt>()), Times.Once);
        }

        [Fact]
        public void SendReadReceipt_ValidConversation_SelectsOtherParticipantCorrectly()
        {
            int targetConversationId = 1;
            int currentUserId = 1;
            int externalUserId = 2;

            var conversationDataTransferObject = new ConversationDataTransferObject(
                targetConversationId,
                new int[] { currentUserId, externalUserId },
                new List<MessageDataTransferObject>(),
                new Dictionary<int, DateTime>
                {
                    { currentUserId, DateTime.Now },
                    { externalUserId, DateTime.Now }
                });

            ReadReceipt capturedReceipt = null;

            conversationRepositoryMock
                .Setup(repository => repository.HandleReadReceipt(It.IsAny<ReadReceipt>()))
                .Callback<ReadReceipt>(receiptObject => capturedReceipt = receiptObject);

            conversationService.SendReadReceipt(conversationDataTransferObject);

            Assert.Equal(currentUserId, capturedReceipt.messageReaderId);
            Assert.Equal(externalUserId, capturedReceipt.messageReceiverId);
        }

        [Fact]
        public void MessageToDTO_TextMessage_MapsCorrectly()
        {
            int defaultMessageId = 1;
            int targetConversationId = 1;
            int senderIdentifier = 1;
            int receiverIdentifier = 2;
            string textContent = "hello";

            var textMessage = new TextMessage(defaultMessageId, targetConversationId, senderIdentifier, receiverIdentifier, DateTime.Now, textContent);

            var messageDataTransferObject = conversationService.MessageToMessageDTO(textMessage);

            Assert.Equal(MessageType.MessageText, messageDataTransferObject.type);
            Assert.Equal(textContent, messageDataTransferObject.content);
        }

        [Fact]
        public void MessageDTOToMessage_TextMessage_MapsCorrectly()
        {
            var messageDataTransferObject = CreateTextDTO();

            var domainMessage = conversationService.MessageDTOToMessage(messageDataTransferObject);

            Assert.IsType<TextMessage>(domainMessage);
        }

        [Fact]
        public void OnMessageReceived_ValidMessage_TriggersActionMessageProcessedEvent()
        {
            int defaultMessageId = 1;
            int targetConversationId = 1;
            int senderIdentifier = 1;
            int receiverIdentifier = 2;
            string textContent = "hi";

            var newTextMessage = new TextMessage(defaultMessageId, targetConversationId, senderIdentifier, receiverIdentifier, DateTime.Now, textContent);

            bool eventInvoked = false;

            conversationService.ActionMessageProcessed += (messageDataTransferObject, senderName) => eventInvoked = true;

            conversationService.OnMessageReceived(newTextMessage);

            Assert.True(eventInvoked);
        }

        [Fact]
        public void OnConversationReceived_ValidConversation_TriggersActionConversationProcessedEvent()
        {
            int targetConversationId = 1;
            int firstParticipantId = 1;
            int secondParticipantId = 2;

            var testConversation = new Conversation(
                targetConversationId,
                new int[] { firstParticipantId, secondParticipantId },
                new List<Message>(),
                new Dictionary<int, DateTime> { { firstParticipantId, DateTime.Now }, { secondParticipantId, DateTime.Now } });

            bool eventInvoked = false;

            conversationService.ActionConversationProcessed += (conversationDataTransferObject, senderName) => eventInvoked = true;

            conversationService.OnConversationReceived(testConversation);

            Assert.True(eventInvoked);
        }

        [Fact]
        public void OnReadReceiptReceived_ValidReceipt_TriggersActionReadReceiptProcessedEvent()
        {
            int targetConversationId = 1;
            int readerIdentifier = 1;
            int receiverIdentifier = 2;

            var testReadReceipt = new ReadReceipt(targetConversationId, readerIdentifier, receiverIdentifier, DateTime.Now);

            bool eventInvoked = false;

            conversationService.ActionReadReceiptProcessed += (receiptDataTransferObject) => eventInvoked = true;

            conversationService.OnReadReceiptReceived(testReadReceipt);

            Assert.True(eventInvoked);
        }

        [Fact]
        public void OnMessageUpdateReceived_ValidUpdate_TriggersActionMessageUpdateProcessedEvent()
        {
            int defaultMessageId = 1;
            int targetConversationId = 1;
            int senderIdentifier = 1;
            int receiverIdentifier = 2;
            string textContent = "hi";

            var updatedMessage = new TextMessage(defaultMessageId, targetConversationId, senderIdentifier, receiverIdentifier, DateTime.Now, textContent);

            bool eventInvoked = false;

            conversationService.ActionMessageUpdateProcessed += (messageDataTransferObject, senderName) => eventInvoked = true;

            conversationService.OnMessageUpdateReceived(updatedMessage);

            Assert.True(eventInvoked);
        }

        [Fact]
        public void OnCardPaymentSelected_ValidCall_CallsHandleRentalRequestFinalizationOnly()
        {
            int testMessageId = 10;

            conversationRepositoryMock
                .Setup(repository => repository.HandleRentalRequestFinalization(It.IsAny<int>()));

            conversationService.OnCardPaymentSelected(testMessageId);

            conversationRepositoryMock.Verify(repository =>
                repository.HandleRentalRequestFinalization(testMessageId),
                Times.Once);
        }

        [Fact]
        public void OnCashPaymentSelected_ValidCall_CallsHandleRentalRequestFinalizationAndCreateCashAgreement()
        {
            int testMessageId = 10;
            int testPaymentId = 99;

            conversationRepositoryMock
                .Setup(repository => repository.HandleRentalRequestFinalization(It.IsAny<int>()));

            conversationRepositoryMock
                .Setup(repository => repository.CreateCashAgreementMessage(It.IsAny<int>(), It.IsAny<int>()));

            conversationService.OnCashPaymentSelected(testMessageId, testPaymentId);

            conversationRepositoryMock.Verify(repository =>
                repository.HandleRentalRequestFinalization(testMessageId),
                Times.Once);

            conversationRepositoryMock.Verify(repository =>
                repository.CreateCashAgreementMessage(testMessageId, testPaymentId),
                Times.Once);
        }

        [Fact]
        public void GetOtherUserName_MissingUser_ReturnsUnknownUser()
        {
            string expectedUnknownUser = "Unknown User";
            int targetConversationId = 1;
            int firstParticipantId = 1;
            int secondParticipantId = 2;

            userRepositoryMock
                .Setup(userRepository => userRepository.GetById(It.IsAny<int>()))
                .Returns((User)null);

            var conversationDataTransferObject = new ConversationDataTransferObject(
                targetConversationId,
                new int[] { firstParticipantId, secondParticipantId },
                new List<MessageDataTransferObject>(),
                new Dictionary<int, DateTime>
                {
                    { firstParticipantId, DateTime.Now },
                    { secondParticipantId, DateTime.Now }
                });

            var resultName = conversationService.GetOtherUserNameByConversationDTO(conversationDataTransferObject);

            Assert.Equal(expectedUnknownUser, resultName);
        }

        [Fact]
        public void MessageToDTO_ImageMessage_MapsCorrectly()
        {
            int defaultMessageId = 1;
            int targetConversationId = 1;
            int senderIdentifier = 1;
            int receiverIdentifier = 2;
            string testImageName = "img.png";

            var testImageMessage = new ImageMessage(defaultMessageId, targetConversationId, senderIdentifier, receiverIdentifier, DateTime.Now, testImageName);

            var messageDataTransferObject = conversationService.MessageToMessageDTO(testImageMessage);

            Assert.Equal(MessageType.MessageImage, messageDataTransferObject.type);
            Assert.Equal(testImageName, messageDataTransferObject.imageUrl);
        }

        [Fact]
        public void MessageToDTO_CashAgreement_MapsCorrectly()
        {
            int defaultMessageId = 1;
            int targetConversationId = 1;
            int sellerIdentifier = 1;
            int buyerIdentifier = 2;
            int testPaymentId = 55;
            string textContent = "cash";
            bool isResolved = false;
            bool isAcceptedByBuyer = true;
            bool isAcceptedBySeller = false;

            var testCashAgreement = new CashAgreementMessage(
                defaultMessageId, targetConversationId, sellerIdentifier, buyerIdentifier,
                testPaymentId,
                DateTime.Now,
                textContent,
                isResolved,
                isAcceptedByBuyer,
                isAcceptedBySeller);

            var messageDataTransferObject = conversationService.MessageToMessageDTO(testCashAgreement);

            Assert.Equal(MessageType.MessageCashAgreement, messageDataTransferObject.type);
            Assert.Equal(testPaymentId, messageDataTransferObject.paymentId);
        }

        [Fact]
        public void MessageToDTO_RentalRequest_MapsCorrectly()
        {
            int defaultMessageId = 1;
            int targetConversationId = 1;
            int senderIdentifier = 1;
            int receiverIdentifier = 2;
            string textContent = "rent";
            int testRequestId = 99;
            bool isResolved = false;
            bool isAccepted = true;

            var testRentalRequest = new RentalRequestMessage(
                defaultMessageId, targetConversationId, senderIdentifier, receiverIdentifier,
                DateTime.Now,
                textContent,
                testRequestId,
                isResolved,
                isAccepted);

            var messageDataTransferObject = conversationService.MessageToMessageDTO(testRentalRequest);

            Assert.Equal(MessageType.MessageRentalRequest, messageDataTransferObject.type);
            Assert.Equal(testRequestId, messageDataTransferObject.requestId);
        }

        [Fact]
        public void MessageToDTO_SystemMessage_MapsCorrectly()
        {
            int defaultMessageId = 1;
            int targetConversationId = 1;
            string systemContent = "system";

            var testSystemMessage = new SystemMessage(defaultMessageId, targetConversationId, DateTime.Now, systemContent);

            var messageDataTransferObject = conversationService.MessageToMessageDTO(testSystemMessage);

            Assert.Equal(MessageType.MessageSystem, messageDataTransferObject.type);
            Assert.Equal(systemContent, messageDataTransferObject.content);
        }

        [Fact]
        public void GetOtherUserNameByMessageDTO_ValidMessage_ReturnsCorrectUser()
        {
            int defaultMessageId = 1;
            int targetConversationId = 1;
            int senderIdentifier = 1;
            int receiverIdentifier = 2;
            string textContent = "hi";
            int missingIdentifier = -1;
            string expectedResultName = "user2";

            var messageDataTransferObject = new MessageDataTransferObject(
                defaultMessageId, targetConversationId, senderIdentifier, receiverIdentifier, DateTime.Now, textContent,
                MessageType.MessageText, string.Empty, false, false, false, false, missingIdentifier, missingIdentifier
            );

            var resultName = conversationService.GetOtherUserNameByMessageDTO(messageDataTransferObject);

            Assert.Equal(expectedResultName, resultName);
        }

        [Fact]
        public void MessageDTOToMessage_ImageMessage_MapsCorrectly()
        {
            string testImageName = "img.png";
            var messageDataTransferObject = CreateTextDTO() with { type = MessageType.MessageImage, imageUrl = testImageName };

            var domainMessage = conversationService.MessageDTOToMessage(messageDataTransferObject);

            Assert.IsType<ImageMessage>(domainMessage);
        }

        [Fact]
        public void MessageDTOToMessage_RentalRequest_MapsCorrectly()
        {
            var messageDataTransferObject = CreateTextDTO() with { type = MessageType.MessageRentalRequest };

            var domainMessage = conversationService.MessageDTOToMessage(messageDataTransferObject);

            Assert.IsType<RentalRequestMessage>(domainMessage);
        }

        [Fact]
        public void MessageDTOToMessage_CashAgreement_MapsCorrectly()
        {
            var messageDataTransferObject = CreateTextDTO() with { type = MessageType.MessageCashAgreement };

            var domainMessage = conversationService.MessageDTOToMessage(messageDataTransferObject);

            Assert.IsType<CashAgreementMessage>(domainMessage);
        }

        [Fact]
        public void MessageDTOToMessage_SystemMessage_MapsCorrectly()
        {
            var messageDataTransferObject = CreateTextDTO() with { type = MessageType.MessageSystem };

            var domainMessage = conversationService.MessageDTOToMessage(messageDataTransferObject);

            Assert.IsType<SystemMessage>(domainMessage);
        }

        [Fact]
        public void ConversationToConversationDTO_ValidConversation_MapsCorrectly()
        {
            int targetConversationId = 1;
            int firstParticipantId = 1;
            int secondParticipantId = 2;
            int defaultMessageId = 1;
            string textContent = "hi";

            var testConversation = new Conversation(
                targetConversationId,
                new[] { firstParticipantId, secondParticipantId },
                new List<Message> { new TextMessage(defaultMessageId, targetConversationId, firstParticipantId, secondParticipantId, DateTime.Now, textContent) },
                new Dictionary<int, DateTime>
                {
                    { firstParticipantId, DateTime.Now },
                    { secondParticipantId, DateTime.Now }
                }
            );

            var conversationDataTransferObject = conversationService.ConversationToConversationDTO(testConversation);

            Assert.Equal(targetConversationId, conversationDataTransferObject.Id);
            Assert.Single(conversationDataTransferObject.MessageList);
        }

        [Fact]
        public void ReadReceiptToReadReceiptDTO_ValidReceipt_MapsCorrectly()
        {
            int targetConversationId = 1;
            int readerIdentifier = 1;
            int receiverIdentifier = 2;

            var testReadReceipt = new ReadReceipt(targetConversationId, readerIdentifier, receiverIdentifier, DateTime.Now);

            var receiptDataTransferObject = conversationService.ReadReceiptToReadReceiptDTO(testReadReceipt);

            Assert.Equal(targetConversationId, receiptDataTransferObject.conversationId);
            Assert.Equal(readerIdentifier, receiptDataTransferObject.readerId);
            Assert.Equal(receiverIdentifier, receiptDataTransferObject.receiverId);
        }

        [Fact]
        public void FetchConversations_ValidRepository_ReturnsMultipleConversations()
        {
            int firstConversationId = 1;
            int secondConversationId = 2;
            int firstParticipantId = 1;
            int secondParticipantId = 2;
            int thirdParticipantId = 3;
            int expectedConversationCount = 2;

            var testConversationList = new List<Conversation>
            {
                new Conversation(firstConversationId, new[] {firstParticipantId, secondParticipantId}, new List<Message>(), new Dictionary<int, DateTime>()),
                new Conversation(secondConversationId, new[] {firstParticipantId, thirdParticipantId}, new List<Message>(), new Dictionary<int, DateTime>())
            };

            conversationRepositoryMock.Setup(repository => repository.GetConversationsForUser(firstParticipantId)).Returns(testConversationList);

            var resultList = conversationService.FetchConversations();

            Assert.Equal(expectedConversationCount, resultList.Count);
        }
    }
}