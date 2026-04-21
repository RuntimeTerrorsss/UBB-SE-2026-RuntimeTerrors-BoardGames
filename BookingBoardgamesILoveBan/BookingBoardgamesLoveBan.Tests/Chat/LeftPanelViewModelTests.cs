using System;
using System.Collections.Generic;
using System.Linq;
using BookingBoardgamesILoveBan.Src.Chat.DTO;
using BookingBoardgamesILoveBan.Src.Chat.ViewModel;
using BookingBoardgamesILoveBan.Src.Enum;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using Moq;
using Xunit;

namespace BookingBoardgamesILoveBan.Tests.Chat
{
    public class LeftPanelViewModelTests
    {
        private Mock<IUserRepository> userService = new Mock<IUserRepository>();

        private LeftPanelViewModel CreateViewModel()
        {
            userService
                .Setup(user => user.GetById(It.IsAny<int>()))
                .Returns(new User(1, "name", "country", "city", "street", "streetNumber"));
            return new LeftPanelViewModel();
        }
        private IUserRepository CreateUserService()
        {
            var service = new Moq.Mock<IUserRepository>();

            service.Setup(receivedService => receivedService.GetById(It.IsAny<int>()))
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
                });
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
                type: MessageType.MessageText,
                imageUrl: null,
                isAccepted: false,
                isResolved: false,
                isAcceptedBySeller: false,
                isAcceptedByBuyer: false,
                requestId: -1,
                paymentId: -1);
        }

        [Fact]
        public void ChatPageViewModel_handleIncomingMessageNewConversation_isAddedToConversations()
        {
            var viewModel = CreateViewModel();

            viewModel.HandleIncomingMessage(CreateMessage(), "John", userService.Object);

            Assert.Single(viewModel.Conversations);
        }

        [Fact]
        public void ChatPageViewModel_handleIncomingMessageExistingConversation_updatesLastMessageText()
        {
            var viewModel = CreateViewModel();

            var message = CreateMessage();

            viewModel.HandleIncomingMessage(message, "John", userService.Object);
            viewModel.HandleIncomingMessage(message with { content = "updated" }, "John", userService.Object);

            Assert.Single(viewModel.Conversations);
            Assert.Equal("updated", viewModel.Conversations.First().LastMessageText);
        }

        [Fact]
        public void ChatPageViewModel_handleIncomingMessageUnselectedConversation_incrementsUnreadCount()
        {
            var viewModel = CreateViewModel();

            viewModel.HandleIncomingMessage(CreateMessage(), "John", userService.Object);

            var conversation = viewModel.Conversations.First();
            Assert.Equal(1, conversation.UnreadCount);
        }

        [Fact]
        public void ChatPageViewModel_selectingConversation_marksMessagesAsReadResetsUnreadCount()
        {
            var viewModel = CreateViewModel();

            viewModel.HandleIncomingMessage(CreateMessage(), "John", userService.Object);

            var conversation = viewModel.Conversations.First();

            viewModel.SelectedConversation = conversation;

            Assert.Equal(0, conversation.UnreadCount);
        }

        [Fact]
        public void ChatPageViewModel_searchText_filtersConversationsByParticipantName()
        {
            var viewModel = CreateViewModel();

            viewModel.HandleIncomingMessage(CreateMessage(), "John", userService.Object);
            viewModel.HandleIncomingMessage(CreateMessage() with { conversationId = 2 }, "Mike", userService.Object);

            viewModel.SearchText = "John";

            Assert.Single(viewModel.Conversations);
        }

        [Fact]
        public void ChatPageViewModel_handleIncomingConversation_addsConversationToList()
        {
            var viewModel = CreateViewModel();
            var service = CreateUserService();

            var conversation = CreateConversation();

            viewModel.HandleIncomingConversation(conversation, "John", 1, service);

            Assert.Single(viewModel.Conversations);
        }

        [Fact]
        public void ChatPageViewModel_handleIncomingConversationDuplicateConversation_doesNotAddDuplicateToList()
        {
            var viewModel = CreateViewModel();
            var service = CreateUserService();

            var conversation = CreateConversation();

            viewModel.HandleIncomingConversation(conversation, "John", 1, service);
            viewModel.HandleIncomingConversation(conversation, "John", 1, service);

            Assert.Single(viewModel.Conversations);
        }

        [Fact]
        public void ChatPageViewModel_sortConversationsByTimestamp_doesNotRemoveAnyConversations()
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
        public void ChatPageViewModel_handleIncomingMessage_hidesEmptyStateWhenMessagesExist()
        {
            var viewModel = CreateViewModel();

            viewModel.HandleIncomingMessage(CreateMessage(), "John", userService.Object);

            Assert.False(viewModel.IsEmptyStateVisible);
        }

        [Fact]
        public void ChatPageViewModel_searchTextCleared_ordersConversationsByDisplayName()
        {
            var viewModel = CreateViewModel();

            viewModel.HandleIncomingMessage(CreateMessage(1), "B", userService.Object);
            viewModel.HandleIncomingMessage(CreateMessage(2), "A", userService.Object);

            viewModel.SearchText = "";

            Assert.Equal("A", viewModel.Conversations[0].DisplayName);
        }

        [Fact]
        public void ChatPageViewModel_sortConversationsByTimestamp_ordersByOldestFirst()
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
        public void ChatPageViewModel_handleIncomingMessageSelectedConversation_doesNotIncrementUnreadCount()
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
        public void ChatPageViewModel_handleIncomingConversationWithMessages_setsLastMessageTextAndTimestamp()
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
        public void ChatPageViewModel_sortConversationsByTimestamp_ordersConversationsByDescendingTimestamp()
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
        public void ChatPageViewModel_raisePropertyChanged_raisesPropertyChangedForGivenPropertyName()
        {
            var viewModel = CreateViewModel();

            bool triggered = false;

            viewModel.PropertyChanged += (sentObject, eventArguments) =>
            {
                if (eventArguments.PropertyName == "TestProp")
                    triggered = true;
            };

            viewModel.RaisePropertyChanged("TestProp");

            Assert.True(triggered);
        }

        [Fact]
        public void ChatPageViewModel_searchTextNoMatchingConversations_showsNoMatchesState()
        {
            var viewModel = CreateViewModel();

            viewModel.HandleIncomingMessage(CreateMessage(), "John", userService.Object);

            viewModel.SearchText = "ZZZ";

            Assert.True(viewModel.IsNoMatchesVisible);
        }

        [Fact]
        public void ChatPageViewModel_selectingConversationWithZeroUnreadCount_keepsUnreadCountUnchanged()
        {
            var viewModel = CreateViewModel();

            viewModel.HandleIncomingMessage(CreateMessage(), "John", userService.Object);

            var conversation = viewModel.Conversations.First();
            conversation.UnreadCount = 0;

            viewModel.SelectedConversation = conversation;

            Assert.Equal(0, conversation.UnreadCount);
        }

        [Fact]
        public void ChatPageViewModel_searchText_filtersOutNonMatchingConversations()
        {
            var viewModel = CreateViewModel();

            viewModel.HandleIncomingMessage(CreateMessage(1), "John", userService.Object);
            viewModel.HandleIncomingMessage(CreateMessage(2), "Mike", userService.Object);

            viewModel.SearchText = "John";

            Assert.DoesNotContain(viewModel.Conversations, conversation => conversation.DisplayName == "Mike");
        }
    }
}