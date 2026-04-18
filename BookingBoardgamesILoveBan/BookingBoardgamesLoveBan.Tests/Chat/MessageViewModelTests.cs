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
                type: MessageType.Text,
                imageUrl: null,
                isAccepted: false,
                isResolved: false,
                isAcceptedByBuyer: false,
                isAcceptedBySeller: false,
                requestId: -1,
                paymentId: -1
            );
        }

        [Fact]
        public void Constructor_Should_Map_All_Fields_Correctly()
        {
            var message = CreateMessage();

            var viewModel = new MessageViewModel(message, currentUserId: 1);

            Assert.Equal(message.id, viewModel.Id);
            Assert.Equal(message.conversationId, viewModel.ConversationId);
            Assert.Equal(message.senderId, viewModel.SenderId);
            Assert.Equal(message.content, viewModel.Content);
            Assert.Equal(message.sentAt, viewModel.SentAt);
            Assert.Equal(message.imageUrl, viewModel.ImageUrl);
            Assert.Equal(message.requestId, viewModel.RequestId);
        }

        [Fact]
        public void IsMine_Should_Be_True_When_Sender_Is_CurrentUser()
        {
            var message = CreateMessage() with { senderId = 1 };

            var viewModel = new MessageViewModel(message, 1);

            Assert.True(viewModel.IsMine);
        }

        [Fact]
        public void IsMine_Should_Be_False_When_Sender_Is_OtherUser()
        {
            var message = CreateMessage() with { senderId = 99 };

            var viewModel = new MessageViewModel(message, 1);

            Assert.False(viewModel.IsMine);
        }

        [Fact]
        public void TimestampString_Should_Format_As_HH_MM()
        {
            var message = CreateMessage();

            var viewModel = new MessageViewModel(message, 1);

            Assert.Equal("14:30", viewModel.TimestampString);
        }

        [Fact]
        public void IsRead_Should_Default_To_False()
        {
            var viewModel = new MessageViewModel(CreateMessage(), 1);

            Assert.False(viewModel.IsRead);
        }

        [Fact]
        public void Setting_IsResolved_Should_Raise_PropertyChanged()
        {
            var viewModel = new MessageViewModel(CreateMessage(), 1);

            bool raised = false;

            viewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(viewModel.IsResolved))
                    raised = true;
            };

            viewModel.IsResolved = true;

            Assert.True(viewModel.IsResolved);
            Assert.True(raised);
        }

        [Fact]
        public void Setting_AcceptedBy_Should_Update_BothAccepted()
        {
            var viewModel = new MessageViewModel(CreateMessage(), 1);

            viewModel.AcceptedBy = new[] { 1, 2 };

            Assert.True(viewModel.BothAccepted);
        }

        [Fact]
        public void Changing_AcceptedBy_Should_Raise_BothAccepted_PropertyChanged()
        {
            var viewModel = new MessageViewModel(CreateMessage(), 1);

            bool raised = false;

            viewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(viewModel.BothAccepted))
                    raised = true;
            };

            viewModel.AcceptedBy = new[] { 1, 2 };

            Assert.True(raised);
        }

        [Fact]
        public void IsRead_Can_Be_Updated()
        {
            var viewModel = new MessageViewModel(CreateMessage(), 1);

            viewModel.IsRead = true;

            Assert.True(viewModel.IsRead);
        }

        [Fact]
        public void IsAccepted_Should_Be_Set_From_DTO_And_Updatable()
        {
            var message = CreateMessage() with { isAccepted = true };

            var viewModel = new MessageViewModel(message, 1);

            Assert.True(viewModel.IsAccepted);

            viewModel.IsAccepted = false;

            Assert.False(viewModel.IsAccepted);
        }

        [Fact]
        public void Constructor_Should_Set_AcceptedBy_When_BuyerAccepted()
        {
            var message = CreateMessage() with
            {
                isAcceptedByBuyer = true,
                receiverId = 99
            };

            var viewModel = new MessageViewModel(message, 1);

            Assert.Equal(2, viewModel.AcceptedBy.Length);
            Assert.All(viewModel.AcceptedBy, id => Assert.Equal(99, id));
        }

        [Fact]
        public void BothAccepted_Should_Be_False_When_Not_Two()
        {
            var viewModel = new MessageViewModel(CreateMessage(), 1);

            viewModel.AcceptedBy = new[] { 1 };

            Assert.False(viewModel.BothAccepted);
        }
    }
}