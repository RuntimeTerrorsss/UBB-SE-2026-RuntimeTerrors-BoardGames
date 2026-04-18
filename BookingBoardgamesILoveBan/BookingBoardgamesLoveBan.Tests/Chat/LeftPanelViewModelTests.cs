using BookingBoardgamesILoveBan.Src.Chat.DTO;
using BookingBoardgamesILoveBan.Src.Chat.ViewModel;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using BookingBoardgamesILoveBan.Src.Enum;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BookingBoardgamesILoveBan.Tests.Chat
{
    public class LeftPanelViewModelTests
    {
        private Mock<IUserService> userService = new Mock<IUserService>();

        private LeftPanelViewModel CreateViewModel()
        {
            userService
                .Setup(user => user.GetById(It.IsAny<int>()))
                .Returns(new User(1, "name", "country", "city", "street", "streetNumber"));
            return new LeftPanelViewModel();
        }
        private IUserService CreateUserService()
        {
            var service = new Moq.Mock<IUserService>();

            service.Setup(s => s.GetById(It.IsAny<int>()))
                .Returns(new User(1, "name", "country", "city", "street", "streetNumber"));

            return service.Object;
        }
        private ConversationDTO CreateConversation(int id = 1)
        {
            return new ConversationDTO(
                conversationId: id,
                participants: new[] { 1, 2 },
                messages: new List<MessageDTO>(),
                lastRead: new Dictionary<int, DateTime>
                {
                    { 1, DateTime.MinValue },
                    { 2, DateTime.MinValue }
                }
            );
        }
        private MessageDTO CreateMessage(int convId = 1)
        {
            return new MessageDTO(
                id: 1,
                conversationId: convId,
                senderId: 1,
                receiverId: 2,
                sentAt: DateTime.Now,
                content: "hello",
                type: MessageType.Text,
                imageUrl: null,
                isAccepted: false,
                isResolved: false,
                isAcceptedBySeller: false,
                isAcceptedByBuyer: false,
                requestId: -1,
                paymentId: -1
            );
        }

        [Fact]
        public void Initial_State_Should_Be_Empty()
        {
            var viewModel = CreateViewModel();

            Assert.Empty(viewModel.Conversations);
            Assert.True(viewModel.IsEmptyStateVisible);
        }

        [Fact]
        public void HandleIncomingMessage_Should_Add_NewConversation()
        {
            var viewModel = CreateViewModel();

            viewModel.HandleIncomingMessage(CreateMessage(), "John", userService.Object);

            Assert.Single(viewModel.Conversations);
        }

        [Fact]
        public void HandleIncomingMessage_Should_UpdateExistingConversation()
        {
            var viewModel = CreateViewModel();

            var message = CreateMessage();

            viewModel.HandleIncomingMessage(message, "John", userService.Object);
            viewModel.HandleIncomingMessage(message with { content = "updated" }, "John", userService.Object);

            Assert.Single(viewModel.Conversations);
            Assert.Equal("updated", viewModel.Conversations.First().LastMessageText);
        }

        [Fact]
        public void HandleIncomingMessage_Should_IncrementUnread_WhenNotSelected()
        {
            var viewModel = CreateViewModel();

            viewModel.HandleIncomingMessage(CreateMessage(), "John", userService.Object);

            var conversation = viewModel.Conversations.First();
            Assert.Equal(1, conversation.UnreadCount);
        }

        [Fact]
        public void SelectingConversation_Should_SetUnreadToZero()
        {
            var viewModel = CreateViewModel();

            viewModel.HandleIncomingMessage(CreateMessage(), "John", userService.Object);

            var conversation = viewModel.Conversations.First();

            viewModel.SelectedConversation = conversation;

            Assert.Equal(0, conversation.UnreadCount);
        }

        [Fact]
        public void SearchText_Should_FilterConversations()
        {
            var viewModel = CreateViewModel();

            viewModel.HandleIncomingMessage(CreateMessage(), "John", userService.Object);
            viewModel.HandleIncomingMessage(CreateMessage() with { conversationId = 2 }, "Mike", userService.Object);

            viewModel.SearchText = "John";

            Assert.Single(viewModel.Conversations);
        }

        [Fact]
        public void HandleIncomingConversation_Should_AddConversation()
        {
            var viewModel = CreateViewModel();
            var service = CreateUserService();

            var conversation = CreateConversation();

            viewModel.HandleIncomingConversation(conversation, "John", 1, service);

            Assert.Single(viewModel.Conversations);
        }

        [Fact]
        public void HandleIncomingConversation_Should_NotDuplicate()
        {
            var viewModel = CreateViewModel();
            var service = CreateUserService();

            var conversation = CreateConversation();

            viewModel.HandleIncomingConversation(conversation, "John", 1, service);
            viewModel.HandleIncomingConversation(conversation, "John", 1, service);

            Assert.Single(viewModel.Conversations);
        }

        [Fact]
        public void HandleIncomingConversation_Should_UseOtherUserCorrectly()
        {
            var viewModel = CreateViewModel();
            var service = CreateUserService();

            var conversation = CreateConversation();

            viewModel.HandleIncomingConversation(conversation, "John", 1, service);

            Assert.Equal("John", viewModel.Conversations.First().DisplayName);
        }

        [Fact]
        public void Sort_Should_KeepConversations()
        {
            var viewModel = CreateViewModel();
            var service = CreateUserService();

            var conversation1 = CreateConversation(1);
            var conversation2 = CreateConversation(2);

            viewModel.HandleIncomingConversation(conversation1, "A", 1, service);
            viewModel.HandleIncomingConversation(conversation2, "B", 1, service);

            viewModel.SortConversationsByTimestamp();

            Assert.Equal(2, viewModel.Conversations.Count);
        }

        [Fact]
        public void UI_States_Should_Update()
        {
            var viewModel = CreateViewModel();

            viewModel.HandleIncomingMessage(CreateMessage(), "John", userService.Object);

            Assert.False(viewModel.IsEmptyStateVisible);
        }

        [Fact]
        public void ApplyFilter_Should_Reorder_Correctly()
        {
            var viewModel = CreateViewModel();

            viewModel.HandleIncomingMessage(CreateMessage(1), "B", userService.Object);
            viewModel.HandleIncomingMessage(CreateMessage(2), "A", userService.Object);

            viewModel.SearchText = "";

            Assert.Equal("A", viewModel.Conversations[0].DisplayName);
        }

        [Fact]
        public void ApplyFilter_Should_Move_Items_When_Order_Changes()
        {
            var viewModel = CreateViewModel();

            viewModel.HandleIncomingMessage(CreateMessage(1), "John", userService.Object);
            viewModel.HandleIncomingMessage(CreateMessage(2), "Mike", userService.Object);

            viewModel.Conversations[0].Timestamp = DateTime.MinValue;
            viewModel.Conversations[1].Timestamp = DateTime.Now;

            viewModel.SortConversationsByTimestamp();

            Assert.Equal("John", viewModel.Conversations.First().DisplayName);
        }

        [Fact]
        public void HandleIncomingMessage_WhenSelected_Should_NotIncreaseUnread()
        {
            var viewModel = CreateViewModel();

            var message = CreateMessage();

            viewModel.HandleIncomingMessage(message, "John", userService.Object);

            var conversation = viewModel.Conversations.First();

            viewModel.SelectedConversation = conversation;

            viewModel.HandleIncomingMessage(message with { content = "new" }, "John", userService.Object);

            Assert.Equal(0, conversation.UnreadCount);
        }

        [Fact]
        public void HandleIncomingConversation_WithMessages_Should_SetPreviewAndTimestamp()
        {
            var viewModel = CreateViewModel();
            var service = CreateUserService();

            var message = CreateMessage();

            var conversation = new ConversationDTO(
                conversationId: 1,
                participants: new[] { 1, 2 },
                messages: new List<MessageDTO> { message },
                lastRead: new Dictionary<int, DateTime>
                {
            { 1, DateTime.MinValue },
            { 2, DateTime.MinValue }
                }
            );

            viewModel.HandleIncomingConversation(conversation, "John", 1, service);

            var result = viewModel.Conversations.First();

            Assert.Equal("hello", result.LastMessageText);
            Assert.Equal(message.sentAt, result.Timestamp);
        }

        [Fact]
        public void SortConversationsByTimestamp_Should_OrderDescending()
        {
            var viewModel = CreateViewModel();
            var service = CreateUserService();

            var message1 = CreateMessage();
            var message2 = CreateMessage(2) with { sentAt = DateTime.Now.AddMinutes(1) };

            viewModel.HandleIncomingMessage(message1, "A", userService.Object);
            viewModel.HandleIncomingMessage(message2, "B", userService.Object);

            viewModel.SortConversationsByTimestamp();

            var list = viewModel.Conversations.ToList();

            Assert.True(list[0].Timestamp >= list[1].Timestamp);
        }

        [Fact]
        public void RaisePropertyChanged_Should_InvokeEvent()
        {
            var viewModel = CreateViewModel();

            bool triggered = false;

            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "TestProp")
                    triggered = true;
            };

            viewModel.RaisePropertyChanged("TestProp");

            Assert.True(triggered);
        }
    }
}