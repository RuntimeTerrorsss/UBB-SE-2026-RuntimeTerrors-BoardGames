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

        private LeftPanelViewModel CreateVM()
        {
            userService
                .Setup(u => u.GetById(It.IsAny<int>()))
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
                convId: id,
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
            var vm = CreateVM();

            Assert.Empty(vm.Conversations);
            Assert.True(vm.IsEmptyStateVisible);
        }

        [Fact]
        public void HandleIncomingMessage_Should_Add_NewConversation()
        {
            var vm = CreateVM();

            vm.HandleIncomingMessage(CreateMessage(), "John", userService.Object);

            Assert.Single(vm.Conversations);
        }

        [Fact]
        public void HandleIncomingMessage_Should_UpdateExistingConversation()
        {
            var vm = CreateVM();

            var msg = CreateMessage();

            vm.HandleIncomingMessage(msg, "John", userService.Object);
            vm.HandleIncomingMessage(msg with { content = "updated" }, "John", userService.Object);

            Assert.Single(vm.Conversations);
            Assert.Equal("updated", vm.Conversations.First().LastMessageText);
        }

        [Fact]
        public void HandleIncomingMessage_Should_IncrementUnread_WhenNotSelected()
        {
            var vm = CreateVM();

            vm.HandleIncomingMessage(CreateMessage(), "John", userService.Object);

            var convo = vm.Conversations.First();
            Assert.Equal(1, convo.UnreadCount);
        }

        [Fact]
        public void SelectingConversation_Should_SetUnreadToZero()
        {
            var vm = CreateVM();

            vm.HandleIncomingMessage(CreateMessage(), "John", userService.Object);

            var convo = vm.Conversations.First();

            vm.SelectedConversation = convo;

            Assert.Equal(0, convo.UnreadCount);
        }

        [Fact]
        public void SearchText_Should_FilterConversations()
        {
            var vm = CreateVM();

            vm.HandleIncomingMessage(CreateMessage(), "John", userService.Object);
            vm.HandleIncomingMessage(CreateMessage() with { conversationId = 2 }, "Mike", userService.Object);

            vm.SearchText = "John";

            Assert.Single(vm.Conversations);
        }

        [Fact]
        public void HandleIncomingConversation_Should_AddConversation()
        {
            var vm = CreateVM();
            var service = CreateUserService();

            var convo = CreateConversation();

            vm.HandleIncomingConversation(convo, "John", 1, service);

            Assert.Single(vm.Conversations);
        }

        [Fact]
        public void HandleIncomingConversation_Should_NotDuplicate()
        {
            var vm = CreateVM();
            var service = CreateUserService();

            var convo = CreateConversation();

            vm.HandleIncomingConversation(convo, "John", 1, service);
            vm.HandleIncomingConversation(convo, "John", 1, service);

            Assert.Single(vm.Conversations);
        }

        [Fact]
        public void HandleIncomingConversation_Should_UseOtherUserCorrectly()
        {
            var vm = CreateVM();
            var service = CreateUserService();

            var convo = CreateConversation();

            vm.HandleIncomingConversation(convo, "John", 1, service);

            Assert.Equal("John", vm.Conversations.First().DisplayName);
        }

        [Fact]
        public void Sort_Should_KeepConversations()
        {
            var vm = CreateVM();
            var service = CreateUserService();

            var convo1 = CreateConversation(1);
            var convo2 = CreateConversation(2);

            vm.HandleIncomingConversation(convo1, "A", 1, service);
            vm.HandleIncomingConversation(convo2, "B", 1, service);

            vm.SortConversationsByTimestamp();

            Assert.Equal(2, vm.Conversations.Count);
        }

        [Fact]
        public void UI_States_Should_Update()
        {
            var vm = CreateVM();

            vm.HandleIncomingMessage(CreateMessage(), "John", userService.Object);

            Assert.False(vm.IsEmptyStateVisible);
        }

        [Fact]
        public void ApplyFilter_Should_Reorder_Correctly()
        {
            var vm = CreateVM();

            vm.HandleIncomingMessage(CreateMessage(1), "B", userService.Object);
            vm.HandleIncomingMessage(CreateMessage(2), "A", userService.Object);

            vm.SearchText = "";

            Assert.Equal("A", vm.Conversations[0].DisplayName);
        }

        [Fact]
        public void ApplyFilter_Should_Move_Items_When_Order_Changes()
        {
            var vm = CreateVM();

            vm.HandleIncomingMessage(CreateMessage(1), "John", userService.Object);
            vm.HandleIncomingMessage(CreateMessage(2), "Mike", userService.Object);

            vm.Conversations[0].Timestamp = DateTime.MinValue;
            vm.Conversations[1].Timestamp = DateTime.Now;

            vm.SortConversationsByTimestamp();

            Assert.Equal("John", vm.Conversations.First().DisplayName);
        }

        [Fact]
        public void HandleIncomingMessage_WhenSelected_Should_NotIncreaseUnread()
        {
            var vm = CreateVM();

            var msg = CreateMessage();

            vm.HandleIncomingMessage(msg, "John", userService.Object);

            var convo = vm.Conversations.First();

            vm.SelectedConversation = convo;

            vm.HandleIncomingMessage(msg with { content = "new" }, "John", userService.Object);

            Assert.Equal(0, convo.UnreadCount);
        }

        [Fact]
        public void HandleIncomingConversation_WithMessages_Should_SetPreviewAndTimestamp()
        {
            var vm = CreateVM();
            var service = CreateUserService();

            var message = CreateMessage();

            var convo = new ConversationDTO(
                convId: 1,
                participants: new[] { 1, 2 },
                messages: new List<MessageDTO> { message },
                lastRead: new Dictionary<int, DateTime>
                {
            { 1, DateTime.MinValue },
            { 2, DateTime.MinValue }
                }
            );

            vm.HandleIncomingConversation(convo, "John", 1, service);

            var result = vm.Conversations.First();

            Assert.Equal("hello", result.LastMessageText);
            Assert.Equal(message.sentAt, result.Timestamp);
        }

        [Fact]
        public void SortConversationsByTimestamp_Should_OrderDescending()
        {
            var vm = CreateVM();
            var service = CreateUserService();

            var msg1 = CreateMessage();
            var msg2 = CreateMessage(2) with { sentAt = DateTime.Now.AddMinutes(1) };

            vm.HandleIncomingMessage(msg1, "A", userService.Object);
            vm.HandleIncomingMessage(msg2, "B", userService.Object);

            vm.SortConversationsByTimestamp();

            var list = vm.Conversations.ToList();

            Assert.True(list[0].Timestamp >= list[1].Timestamp);
        }

        [Fact]
        public void RaisePropertyChanged_Should_InvokeEvent()
        {
            var vm = CreateVM();

            bool triggered = false;

            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "TestProp")
                    triggered = true;
            };

            vm.RaisePropertyChanged("TestProp");

            Assert.True(triggered);
        }
    }
}