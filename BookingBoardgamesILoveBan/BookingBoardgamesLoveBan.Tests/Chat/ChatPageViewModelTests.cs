using System;
using System.Collections.Generic;
using BookingBoardgamesILoveBan.Src.Chat.DTO;
using BookingBoardgamesILoveBan.Src.Chat.Model;
using BookingBoardgamesILoveBan.Src.Chat.Repository;
using BookingBoardgamesILoveBan.Src.Chat.Service;
using BookingBoardgamesILoveBan.src.Chat.ViewModel;
using BookingBoardgamesILoveBan.Src.Chat.ViewModel;
using BookingBoardgamesILoveBan.Src.Enum;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using BookingBoardgamesILoveBan.Src.Model;
using Moq;
using Xunit;

namespace BookingBoardgamesILoveBan.Tests.Chat
{
    public class ChatPageViewModelTests
    {
        private readonly int currentUserId = 1;
        private Mock<IUserRepository> userService;
        private Mock<IUserService> userServiceMock;
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
                new List<Message>(),
                new Dictionary<int, DateTime>
                {
                    { 1, DateTime.MinValue },
                    { 2, DateTime.MinValue }
                })
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

            userService = new Mock<IUserRepository>();
            userServiceMock = new Mock<IUserService>();
            userServiceMock
                .Setup(service => service.GetById(It.IsAny<int>()))
                .Returns(new User(1, "name", "country", "city", "street", "streetNumber"));

            userService
                .Setup(u => u.GetById(It.IsAny<int>()))
                .Returns(new User(1, "name", "country", "city", "street", "streetNumber"));

            return new ConversationService(
                conversationRepositoryMock.Object,
                currentUserId,
                userService.Object);
                userServiceMock.Object
            );
        }

        private ChatPageViewModel CreateChatPageViewModel()
        {
            return new ChatPageViewModel(currentUserId, CreateService(), userService.Object);
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

            Assert.NotNull(vm.LeftPanelModelView);
            Assert.NotNull(vm.ChatModelView);
            Assert.Single(chatPageViewModel.LeftPanel.Conversations);
        }

        [Fact]
        public void MessageSent_Sets_Correct_Receiver_And_Calls_Repository()
        {
            var conversationService = CreateConversationService();

            var msg = new MessageDTO(1, "hello", MessageType.MessageText)
            var chatPageViewModel = new ChatPageViewModel(
                currentUserId,
                conversationService,
                userServiceMock.Object
            );

            var messageDto = new MessageDTO(1, "hello", MessageType.Text)
            {
                conversationId = 1,
                senderId = 1
            };

            bool called = false;
            service.ActionMessageProcessed += (m, u) => { called = true; };
            chatPageViewModel.Chat.RaiseMessageSent(messageDto);

            vm.ChatModelView.RaiseMessageSent(msg);
            conversationRepositoryMock.Verify(repository =>
                repository.HandleNewMessage(It.Is<Message>(message =>
                    message.SenderId == 1 &&
                    message.ReceiverId == 2
                )),
                Times.Once);
        }

        [Fact]
        public void SelectingConversation_LoadsChat()
        {
            var chatPageViewModel = CreateChatPageViewModel();

            var selectedConversation = chatPageViewModel.LeftPanel.Conversations.First();

            chatPageViewModel.LeftPanel.SelectedConversation = selectedConversation;

            Assert.Equal(
                selectedConversation.ConversationId,
                chatPageViewModel.Chat.ConversationId
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

            service.ActionReadReceiptProcessed += null;
            var newMessage = new TextMessage(5, 1, 2, 1, DateTime.Now, "new message");

            conversationService.OnMessageReceived(newMessage);

            var conversationPreview = chatPageViewModel.LeftPanel.Conversations.First();

            Assert.Contains("new message", conversationPreview.LastMessageText);
        }

        [Fact]
        public void ReadReceipt_Updates_LastRead()
        {
            var service = CreateService();
            var vm = new ChatPageViewModel(currentUserId, service, userService.Object);
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

            vm.ChatModelView.RaiseBookingRequestUpdate(5, 1, true, true);
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

            vm.ChatModelView.RaiseCashAgreementAccept(5, 1);
            chatPageViewModel.Chat.RaiseBookingRequestUpdate(1, 1, true, true);

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

            chatPageViewModel.Chat.RaiseCashAgreementAccept(1, 1);

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
            var selectedConversation = chatPageViewModel.LeftPanel.Conversations.First();
            chatPageViewModel.LeftPanel.SelectedConversation = selectedConversation;

            var updatedMessage = new TextMessage(1, 1, 2, 1, DateTime.Now, "updated");

            conversationService.OnMessageUpdateReceived(updatedMessage);

            Assert.Contains(
                chatPageViewModel.Chat.Messages,
                message => message.Content == "updated"
            );
        }

        [Fact]
        public void InvalidConversation_DoesNotCrash()
        {
            var chatPageViewModel = CreateChatPageViewModel();

            chatPageViewModel.Chat.RaiseBookingRequestUpdate(999, 999, true, true);

            Assert.NotNull(chatPageViewModel);
        }
    }
}