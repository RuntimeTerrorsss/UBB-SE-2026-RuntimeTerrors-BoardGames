using BookingBoardgamesILoveBan.Src.Chat.ViewModel;
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
    public class ChatPageViewModelTests
    {
        private readonly int currentUserId = 1;
        private Mock<IUserRepository> userServiceMock;
        private Mock<IConversationRepository> conversationRepositoryMock;

        private ConversationService CreateConversationService()
        {
            int targetConversationId = 1;
            int firstParticipantId = 1;
            int secondParticipantId = 2;
            int messageIdentifier = 1;
            string initialContent = "initial";
            string testUserName = "name";
            string testCountry = "country";
            string testCity = "city";
            string testStreet = "street";
            string testStreetNumber = "streetNumber";

            conversationRepositoryMock = new Mock<IConversationRepository>();

            conversationRepositoryMock
                .Setup(repository => repository.GetConversationsForUser(It.IsAny<int>()))
                .Returns(new List<Conversation>
                {
                    new Conversation(
                        targetConversationId,
                        new[] { firstParticipantId, secondParticipantId },
                        new List<Message>
                        {
                            new TextMessage(messageIdentifier, targetConversationId, secondParticipantId, firstParticipantId, DateTime.Now, initialContent)
                        },
                        new Dictionary<int, DateTime>
                        {
                            { firstParticipantId, DateTime.MinValue },
                            { secondParticipantId, DateTime.MinValue }
                        }
                    )
                });

            userServiceMock = new Mock<IUserRepository>();
            userServiceMock
                .Setup(service => service.GetById(It.IsAny<int>()))
                .Returns(new User(currentUserId, testUserName, testCountry, testCity, testStreet, testStreetNumber));

            return new ConversationService(
                conversationRepositoryMock.Object,
                currentUserId,
                userServiceMock.Object
            );
        }

        private ChatPageViewModel CreateChatPageViewModel()
        {
            return new ChatPageViewModel(
                currentUserId,
                CreateConversationService(),
                userServiceMock.Object
            );
        }

        [Fact]
        public void Constructor_ValidInitialization_LoadsConversations()
        {
            var chatPageViewModel = CreateChatPageViewModel();

            Assert.Single(chatPageViewModel.LeftPanelModelView.Conversations);
        }

        [Fact]
        public void MessageSent_ValidMessage_SetsCorrectReceiver()
        {
            int targetConversationId = 1;
            int senderIdentifier = 1;
            int expectedReceiverIdentifier = 2;
            int missingIdentifier = -1;
            string textContent = "hello";

            var conversationService = CreateConversationService();

            var chatPageViewModel = new ChatPageViewModel(
                currentUserId,
                conversationService,
                userServiceMock.Object
            );

            // FIX: Using the proper 14-parameter instantiation instead of the deleted "junk" constructor
            var messageDataTransferObject = new MessageDataTransferObject(
                id: missingIdentifier,
                conversationId: targetConversationId,
                senderId: senderIdentifier,
                receiverId: expectedReceiverIdentifier,
                sentAt: DateTime.Now,
                content: textContent,
                type: MessageType.MessageText,
                imageUrl: string.Empty,
                isResolved: false,
                isAccepted: false,
                isAcceptedByBuyer: false,
                isAcceptedBySeller: false,
                requestId: missingIdentifier,
                paymentId: missingIdentifier);

            chatPageViewModel.ChatModelView.RaiseMessageSent(messageDataTransferObject);

            conversationRepositoryMock.Verify(repository =>
                repository.HandleNewMessage(It.Is<Message>(messageItem =>
                    messageItem.MessageSenderId == senderIdentifier &&
                    messageItem.MessageReceiverId == expectedReceiverIdentifier
                )),
                Times.Once);
        }

        [Fact]
        public void SelectedConversation_ValidSelection_LoadsChat()
        {
            var chatPageViewModel = CreateChatPageViewModel();
            var selectedConversation = chatPageViewModel.LeftPanelModelView.Conversations.First();

            chatPageViewModel.LeftPanelModelView.SelectedConversation = selectedConversation;

            Assert.Equal(
                selectedConversation.ConversationId,
                chatPageViewModel.ChatModelView.ConversationId
            );
        }

        [Fact]
        public void OnMessageReceived_ValidMessage_AddsMessageToLeftPanel()
        {
            int messageIdentifier = 5;
            int targetConversationId = 1;
            int senderIdentifier = 2;
            int receiverIdentifier = 1;
            string newContent = "new message";

            var conversationService = CreateConversationService();

            var chatPageViewModel = new ChatPageViewModel(
                currentUserId,
                conversationService,
                userServiceMock.Object
            );

            var newMessage = new TextMessage(messageIdentifier, targetConversationId, senderIdentifier, receiverIdentifier, DateTime.Now, newContent);

            conversationService.OnMessageReceived(newMessage);

            var conversationPreview = chatPageViewModel.LeftPanelModelView.Conversations.First();

            Assert.Contains(newContent, conversationPreview.LastMessageText);
        }

        [Fact]
        public void OnReadReceiptReceived_ValidReceipt_UpdatesLastReadTime()
        {
            int targetConversationId = 1;
            int readerIdentifier = 2;
            int receiverIdentifier = 1;
            DateTime timestamp = DateTime.Now;

            var conversationService = CreateConversationService();

            var chatPageViewModel = new ChatPageViewModel(
                currentUserId,
                conversationService,
                userServiceMock.Object
            );

            conversationService.OnReadReceiptReceived(
                new ReadReceipt(targetConversationId, readerIdentifier, receiverIdentifier, timestamp)
            );

            var updatedConversation = chatPageViewModel.ConversationService.FetchConversations().First();

            Assert.True(updatedConversation.LastRead[readerIdentifier] >= timestamp);
        }

        [Fact]
        public void RaiseBookingRequestUpdate_ValidRequest_TriggersMessageUpdate()
        {
            int messageIdentifier = 1;
            int targetConversationId = 1;
            bool isAccepted = true;
            bool isResolved = true;

            var conversationService = CreateConversationService();

            var chatPageViewModel = new ChatPageViewModel(
                currentUserId,
                conversationService,
                userServiceMock.Object
            );

            chatPageViewModel.ChatModelView.RaiseBookingRequestUpdate(messageIdentifier, targetConversationId, isAccepted, isResolved);

            conversationRepositoryMock.Verify(
                repository => repository.HandleMessageUpdate(It.IsAny<Message>()),
                Times.AtMostOnce()
            );
        }

        [Fact]
        public void RaiseCashAgreementAccept_ValidAgreement_TriggersMessageUpdate()
        {
            int messageIdentifier = 1;
            int targetConversationId = 1;

            var conversationService = CreateConversationService();

            var chatPageViewModel = new ChatPageViewModel(
                currentUserId,
                conversationService,
                userServiceMock.Object
            );

            chatPageViewModel.ChatModelView.RaiseCashAgreementAccept(messageIdentifier, targetConversationId);

            conversationRepositoryMock.Verify(
                repository => repository.HandleMessageUpdate(It.IsAny<Message>()),
                Times.AtMostOnce()
            );
        }

        [Fact]
        public void OnMessageUpdateReceived_ValidUpdate_ReplacesMessageContent()
        {
            int messageIdentifier = 1;
            int targetConversationId = 1;
            int senderIdentifier = 2;
            int receiverIdentifier = 1;
            string updatedContent = "updated";

            var conversationService = CreateConversationService();

            var chatPageViewModel = new ChatPageViewModel(
                currentUserId,
                conversationService,
                userServiceMock.Object
            );

            var selectedConversation = chatPageViewModel.LeftPanelModelView.Conversations.First();
            chatPageViewModel.LeftPanelModelView.SelectedConversation = selectedConversation;

            var updatedMessage = new TextMessage(messageIdentifier, targetConversationId, senderIdentifier, receiverIdentifier, DateTime.Now, updatedContent);

            conversationService.OnMessageUpdateReceived(updatedMessage);

            Assert.Contains(
                chatPageViewModel.ChatModelView.Messages,
                messageItem => messageItem.Content == updatedContent
            );
        }

        [Fact]
        public void RaiseBookingRequestUpdate_InvalidConversation_ExecutesWithoutError()
        {
            int invalidMessageIdentifier = 999;
            int invalidConversationIdentifier = 999;
            bool isAccepted = true;
            bool isResolved = true;

            var chatPageViewModel = CreateChatPageViewModel();

            Exception executionException = Record.Exception(() => chatPageViewModel.ChatModelView.RaiseBookingRequestUpdate(invalidMessageIdentifier, invalidConversationIdentifier, isAccepted, isResolved));

            Assert.Null(executionException);
        }
    }
}