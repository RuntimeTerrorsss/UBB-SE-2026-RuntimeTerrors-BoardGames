using System;
using Xunit;
using BookingBoardgamesILoveBan.Src.Chat.ViewModel;
using BookingBoardgamesILoveBan.Src.Chat.DTO;
using BookingBoardgamesILoveBan.Src.Enum;

namespace BookingBoardgamesILoveBan.Tests.Chat
{
    public class MessageViewModelTests
    {
        private MessageDTO CreateMessage()
        {
            return new MessageDTO(
                id: 1,
                conversationId: 10,
                senderId: 2,
                receiverId: 1,
                sentAt: new DateTime(2026, 1, 1, 14, 30, 0),
                content: "hello",
                type: MessageType.MessageText,
                imageUrl: null,
                isAccepted: false,
                isResolved: false,
                isAcceptedByBuyer: false,
                isAcceptedBySeller: false,
                requestId: -1,
                paymentId: -1);
        }

        [Fact]
        public void Constructor_Should_Map_All_Fields_Correctly()
        {
            var msg = CreateMessage();

            var vm = new MessageViewModel(msg, currentUserId: 1);

            Assert.Equal(msg.id, vm.Id);
            Assert.Equal(msg.conversationId, vm.ConversationId);
            Assert.Equal(msg.senderId, vm.SenderId);
            Assert.Equal(msg.content, vm.Content);
            Assert.Equal(msg.sentAt, vm.SentAt);
            Assert.Equal(msg.imageUrl, vm.ImageUrl);
            Assert.Equal(msg.requestId, vm.RequestId);
        }

        [Fact]
        public void IsMine_Should_Be_True_When_Sender_Is_CurrentUser()
        {
            var msg = CreateMessage() with { senderId = 1 };

            var vm = new MessageViewModel(msg, 1);

            Assert.True(vm.IsMine);
        }

        [Fact]
        public void IsMine_Should_Be_False_When_Sender_Is_OtherUser()
        {
            var msg = CreateMessage() with { senderId = 99 };

            var vm = new MessageViewModel(msg, 1);

            Assert.False(vm.IsMine);
        }

        [Fact]
        public void TimestampString_Should_Format_As_HH_MM()
        {
            var msg = CreateMessage();

            var vm = new MessageViewModel(msg, 1);

            Assert.Equal("14:30", vm.TimestampString);
        }

        [Fact]
        public void IsRead_Should_Default_To_False()
        {
            var vm = new MessageViewModel(CreateMessage(), 1);

            Assert.False(vm.IsRead);
        }

        [Fact]
        public void Setting_IsResolved_Should_Raise_PropertyChanged()
        {
            var vm = new MessageViewModel(CreateMessage(), 1);

            bool raised = false;

            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.IsResolved))
                {
                    raised = true;
                }
            };

            vm.IsResolved = true;

            Assert.True(vm.IsResolved);
            Assert.True(raised);
        }

        [Fact]
        public void Setting_AcceptedBy_Should_Update_BothAccepted()
        {
            var vm = new MessageViewModel(CreateMessage(), 1);

            vm.AcceptedBy = new[] { 1, 2 };

            Assert.True(vm.BothAccepted);
        }

        [Fact]
        public void Changing_AcceptedBy_Should_Raise_BothAccepted_PropertyChanged()
        {
            var vm = new MessageViewModel(CreateMessage(), 1);

            bool raised = false;

            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.BothAccepted))
                {
                    raised = true;
                }
            };

            vm.AcceptedBy = new[] { 1, 2 };

            Assert.True(raised);
        }

        [Fact]
        public void IsRead_Can_Be_Updated()
        {
            var vm = new MessageViewModel(CreateMessage(), 1);

            vm.IsRead = true;

            Assert.True(vm.IsRead);
        }
    }
}