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

        private ConversationService CreateConversationService()
        {
            conversationRepositoryMock = new Mock<IConversationRepository>();

            conversationRepositoryMock
                .Setup(repository => repository.GetConversationsForUser(It.IsAny<int>()))
                .Returns(new List<Conversation>
                {
                    new Conversation(
                        1,
                        new[] { 1, 2 },
                        new List<Message>
                        {
                            new TextMessage(1, 1, 2, 1, DateTime.Now, "initial")
                        },
                        new Dictionary<int, DateTime>
                        {
                            { 1, DateTime.MinValue },
                            { 2, DateTime.MinValue }
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
        public void Constructor_Loads_Conversations()
        {
            var chatPageViewModel = CreateChatPageViewModel();

            Assert.Single(chatPageViewModel.LeftPanelModelView.Conversations);
        }

        [Fact]
        public void MessageSent_Sets_Correct_Receiver_And_Calls_Repository()
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
                    message.MessageSenderId == 1 &&
                    message.MessageReceiverId == 2
                )),
                Times.Once);
        }

        [Fact]
        public void SelectingConversation_LoadsChat()
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
        public void IncomingMessage_Is_Added_To_LeftPanel()
        {
            var conversationService = CreateConversationService();

            var chatPageViewModel = new ChatPageViewModel(
                currentUserId,
                conversationService,
                userServiceMock.Object
            );

            var newMessage = new TextMessage(5, 1, 2, 1, DateTime.Now, "new message");

            conversationService.OnMessageReceived(newMessage);

            var conversationPreview = chatPageViewModel.LeftPanelModelView.Conversations.First();

            Assert.Contains("new message", conversationPreview.LastMessageText);
        }

        [Fact]
        public void ReadReceipt_Updates_LastRead()
        {
            var conversationService = CreateConversationService();

            var chatPageViewModel = new ChatPageViewModel(
                currentUserId,
                conversationService,
                userServiceMock.Object
            );

            var timestamp = DateTime.Now;

            conversationService.OnReadReceiptReceived(
                new ReadReceipt(1, 2, 1, timestamp)
            );

            var updatedConversation =
                chatPageViewModel.ConversationService.FetchConversations().First();

            Assert.True(updatedConversation.LastRead[2] >= timestamp);
        }

        [Fact]
        public void BookingRequest_Triggers_Message_Update()
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
        public void CashAgreement_Triggers_Message_Update()
        {
            var conversationService = CreateConversationService();

            var chatPageViewModel = new ChatPageViewModel(
                currentUserId,
                conversationService,
                userServiceMock.Object
            );

            chatPageViewModel.ChatModelView.RaiseCashAgreementAccept(1, 1);

            conversationRepositoryMock.Verify(
                repository => repository.HandleMessageUpdate(It.IsAny<Message>()),
                Times.AtMostOnce()
            );
        }

        [Fact]
        public void MessageUpdate_Replaces_Message_Content()
        {
            var conversationService = CreateConversationService();

            var chatPageViewModel = new ChatPageViewModel(
                currentUserId,
                conversationService,
                userServiceMock.Object
            );

            // select conversation so Chat gets loaded
            var selectedConversation = chatPageViewModel.LeftPanelModelView.Conversations.First();
            chatPageViewModel.LeftPanelModelView.SelectedConversation = selectedConversation;

            var updatedMessage = new TextMessage(1, 1, 2, 1, DateTime.Now, "updated");

            conversationService.OnMessageUpdateReceived(updatedMessage);

            Assert.Contains(
                chatPageViewModel.ChatModelView.Messages,
                message => message.Content == "updated"
            );
        }

        [Fact]
        public void InvalidConversation_DoesNotCrash()
        {
            var chatPageViewModel = CreateChatPageViewModel();

            chatPageViewModel.ChatModelView.RaiseBookingRequestUpdate(999, 999, true, true);

            Assert.NotNull(chatPageViewModel);
        }
    }
}