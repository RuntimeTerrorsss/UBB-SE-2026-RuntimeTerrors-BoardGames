using System;
using Xunit;
using BookingBoardgamesILoveBan.Src.Chat.ViewModel;
using BookingBoardgamesILoveBan.Src.Chat.DTO;
using BookingBoardgamesILoveBan.Src.Enum;
using Microsoft.UI.Xaml;

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
        public void IsMine_SenderIsCurrentUser_IsTrue()
        {
            var message = CreateMessage() with { senderId = 1 };

            var viewModel = new MessageViewModel(message, 1);

            Assert.True(viewModel.IsMine);
        }

        [Fact]
        public void IsMine_SenderIsNotCurrentUser_IsFalse()
        {
            var message = CreateMessage() with { senderId = 99 };

            var viewModel = new MessageViewModel(message, 1);

            Assert.False(viewModel.IsMine);
        }

        [Fact]
        public void IsRead_Default_False()
        {
            var viewModel = new MessageViewModel(CreateMessage(), 1);

            Assert.False(viewModel.IsRead);
        }

        [Fact]
        public void SettingIsResolved_RaisePropertyChanged()
        {
            var viewModel = new MessageViewModel(CreateMessage(), 1);

            bool raised = false;

            viewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(viewModel.IsResolved))
                {
                    raised = true;
                }
            };

            viewModel.IsResolved = true;

            Assert.True(viewModel.IsResolved);
            Assert.True(raised);
        }

        [Fact]
        public void SettingAcceptedByUpdate_BothAccepted()
        {
            var viewModel = new MessageViewModel(CreateMessage(), 1);

            viewModel.AcceptedBy = new[] { 1, 2 };

            Assert.True(viewModel.BothAccepted);
        }

        [Fact]
        public void ChangingAcceptedBy_RaiseBothAcceptedPropertyChanged()
        {
            var viewModel = new MessageViewModel(CreateMessage(), 1);

            bool raised = false;

            viewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(viewModel.BothAccepted))
                {
                    raised = true;
                }
            };

            viewModel.AcceptedBy = new[] { 1, 2 };

            Assert.True(raised);
        }

        [Fact]
        public void IsRead_CanBeUpdated()
        {
            var viewModel = new MessageViewModel(CreateMessage(), 1);

            viewModel.IsRead = true;

            Assert.True(viewModel.IsRead);
        }

        [Fact]
        public void IsAccepted_BeSetFromDTOAndUpdatable()
        {
            var message = CreateMessage() with { isAccepted = true };

            var viewModel = new MessageViewModel(message, 1);

            Assert.True(viewModel.IsAccepted);

            viewModel.IsAccepted = false;

            Assert.False(viewModel.IsAccepted);
        }

        [Fact]
        public void BothAccepted_NotTwo_False()
        {
            var viewModel = new MessageViewModel(CreateMessage(), 1);

            viewModel.AcceptedBy = new[] { 1 };

            Assert.False(viewModel.BothAccepted);
        }

        [Fact]
        public void IsMineToAlignment_ReturnCorrectValue()
        {
            var viewModel = new MessageViewModel(CreateMessage(), 1);

            Assert.Equal(HorizontalAlignment.Right, viewModel.IsMineToAlignment(true));
            Assert.Equal(HorizontalAlignment.Left, viewModel.IsMineToAlignment(false));
        }

        [Fact]
        public void IsMineToCornerRadius_ReturnDifferentValues()
        {
            var viewModel = new MessageViewModel(CreateMessage(), 1);

            Assert.NotEqual(
                viewModel.IsMineToCornerRadius(true),
                viewModel.IsMineToCornerRadius(false)
            );
        }

        [Fact]
        public void IsMineToBorderThickness_ReturnCorrect()
        {
            var viewModel = new MessageViewModel(CreateMessage(), 1);

            Assert.Equal(new Thickness(0), viewModel.IsMineToTheirsOnlyBorderThickness(true));
            Assert.Equal(new Thickness(1), viewModel.IsMineToTheirsOnlyBorderThickness(false));
        }
    }
}