using BookingBoardgamesILoveBan.src.Chat.ViewModel;
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
        private int conversationId = 1;
        private int[] conversationParticaipantsId = { 1, 2 };

        private ConversationService CreateConversationService()
        {
            conversationRepositoryMock = new Mock<IConversationRepository>();
            int messageId = 1;
            conversationRepositoryMock
                .Setup(repository => repository.GetConversationsForUser(It.IsAny<int>()))
                .Returns(new List<Conversation>
                {
                    new Conversation(
                        conversationId,
                        conversationParticaipantsId,
                        new List<Message>
                        {
                            new TextMessage(messageId, conversationId, conversationParticaipantsId[1], conversationParticaipantsId[0], DateTime.Now, "initial")
                        },
                        new Dictionary<int, DateTime>
                        {
                            { conversationParticaipantsId[0], DateTime.MinValue },
                            { conversationParticaipantsId[1], DateTime.MinValue }
                        }
                    )
                });

            userServiceMock = new Mock<IUserRepository>();
            userServiceMock
                .Setup(service => service.GetById(It.IsAny<int>()))
                .Returns(new User(1, "name", "country", "city", "street", "streetNumber"));

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
        public void ChatPageViewModelConstructor_whenRepositoryReturnsSingleConversation_initializesSingleConversationInLeftPanel()
        {
            var chatPageViewModel = CreateChatPageViewModel();

            Assert.Single(chatPageViewModel.LeftPanelModelView.Conversations);
        }

        [Fact]
        public void ChatPageViewModel_messageSent_validMessageDto_setsCorrectReceiverAndCallsHandleNewMessage()
        {
            var conversationService = CreateConversationService();

            var chatPageViewModel = new ChatPageViewModel(
                currentUserId,
                conversationService,
                userServiceMock.Object
            );

            var messageDto = new MessageDTO(1, "hello", MessageType.MessageText)
            {
                conversationId = 1,
                senderId = 1
            };

            chatPageViewModel.ChatModelView.RaiseMessageSent(messageDto);

            conversationRepositoryMock.Verify(repository =>
                repository.HandleNewMessage(It.Is<Message>(message =>
                    message.MessageSenderId == conversationParticaipantsId[0] &&
                    message.MessageReceiverId == conversationParticaipantsId[1]
                )),
                Times.Once);
        }

        [Fact]
        public void ChatPageViewModel_selectedConversation_updatesChatModelViewConversationId()
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
        public void ChatPageViewModel_onMessageReceived_updatesLeftPanelConversationPreviewLastMessageText()
        {
            var conversationService = CreateConversationService();

            var chatPageViewModel = new ChatPageViewModel(
                currentUserId,
                conversationService,
                userServiceMock.Object
            );
            int messageId = 5;
            var newMessage = new TextMessage(messageId, conversationId, conversationParticaipantsId[1], conversationParticaipantsId[0], DateTime.Now, "new message");

            conversationService.OnMessageReceived(newMessage);

            var conversationPreview = chatPageViewModel.LeftPanelModelView.Conversations.First();

            Assert.Contains("new message", conversationPreview.LastMessageText);
        }

        [Fact]
        public void ChatPageViewModel_onReadReceiptReceived_updatesConversationLastReadTimestamp()
        {
            var conversationService = CreateConversationService();

            var chatPageViewModel = new ChatPageViewModel(
                currentUserId,
                conversationService,
                userServiceMock.Object
            );

            var timestamp = DateTime.Now;

            conversationService.OnReadReceiptReceived(
                new ReadReceipt(conversationId, conversationParticaipantsId[1], conversationParticaipantsId[0], timestamp)
            );

            var updatedConversation =
                chatPageViewModel.ConversationService.FetchConversations().First();

            Assert.True(updatedConversation.LastRead[2] >= timestamp);
        }

        [Fact]
        public void ChatPageViewModel_bookingRequestUpdate_callsHandleMessageUpdateOnRepository()
        {
            var conversationService = CreateConversationService();

            var chatPageViewModel = new ChatPageViewModel(
                currentUserId,
                conversationService,
                userServiceMock.Object
            );

            chatPageViewModel.ChatModelView.RaiseBookingRequestUpdate(1, 1, true, true);

            conversationRepositoryMock.Verify(
                repository => repository.HandleMessageUpdate(It.IsAny<Message>()),
                Times.AtMostOnce()
            );
        }

        [Fact]
        public void ChatPageViewModel_cashAgreementAccepted_callsHandleMessageUpdateOnRepository()
        {
            var conversationService = CreateConversationService();

            var chatPageViewModel = new ChatPageViewModel(
                currentUserId,
                conversationService,
                userServiceMock.Object
            );
            int messageId = 1;
            chatPageViewModel.ChatModelView.RaiseCashAgreementAccept(messageId, conversationId);

            conversationRepositoryMock.Verify(
                repository => repository.HandleMessageUpdate(It.IsAny<Message>()),
                Times.AtMostOnce()
            );
        }

        [Fact]
        public void ChatPageViewModel_onMessageUpdateReceived_updatesChatModelViewMessageContent()
        {
            var conversationService = CreateConversationService();

            var chatPageViewModel = new ChatPageViewModel(
                currentUserId,
                conversationService,
                userServiceMock.Object
            );

            var selectedConversation = chatPageViewModel.LeftPanelModelView.Conversations.First();
            chatPageViewModel.LeftPanelModelView.SelectedConversation = selectedConversation;
            int messageId = 1;
            var updatedMessage = new TextMessage(messageId, conversationId, conversationParticaipantsId[1], conversationParticaipantsId[0], DateTime.Now, "updated");

            conversationService.OnMessageUpdateReceived(updatedMessage);

            Assert.Contains(
                chatPageViewModel.ChatModelView.Messages,
                message => message.Content == "updated"
            );
        }

        [Fact]
        public void ChatPageViewModel_raiseBookingRequestUpdate_invalidConversationId_doesNotThrowException()
        {
            var chatPageViewModel = CreateChatPageViewModel();
            int invaliId = 999;
            chatPageViewModel.ChatModelView.RaiseBookingRequestUpdate(invaliId, invaliId, true, true);

            Assert.NotNull(chatPageViewModel);
        }
    }
}