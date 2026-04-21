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
        public void MessageViewModelIsMine_senderMatchesCurrentUser_returnsTrue()
        {
            var message = CreateMessage() with { senderId = 1 };

            var viewModel = new MessageViewModel(message, 1);

            Assert.True(viewModel.IsMine);
        }

        [Fact]
        public void MessageViewModelIsMine_senderDoesNotMatchCurrentUser_returnsFalse()
        {
            var message = CreateMessage() with { senderId = 99 };

            var viewModel = new MessageViewModel(message, 1);

            Assert.False(viewModel.IsMine);
        }

        [Fact]
        public void MessageViewModelIsRead_newInstance_returnsFalse()
        {
            var viewModel = new MessageViewModel(CreateMessage(), 1);

            Assert.False(viewModel.IsRead);
        }

        [Fact]
        public void MessageViewModelIsResolved_setter_raisesPropertyChangedAndUpdatesValue()
        {
            var viewModel = new MessageViewModel(CreateMessage(), 1);

            bool raised = false;

            viewModel.PropertyChanged += (senderObject, eventArguments) =>
            {
                if (eventArguments.PropertyName == nameof(viewModel.IsResolved))
                {
                    raised = true;
                }
            };

            viewModel.IsResolved = true;

            Assert.True(viewModel.IsResolved);
            Assert.True(raised);
        }

        [Fact]
        public void MessageViewModelSettingAcceptedBy_bothUsersAccepted_updatesBothAcceptedToTrue()
        {
            var viewModel = new MessageViewModel(CreateMessage(), 1);

            viewModel.AcceptedBy = new[] { 1, 2 };

            Assert.True(viewModel.BothAccepted);
        }

        [Fact]
        public void MessageViewModelAcceptedBy_change_raisesPropertyChangedForBothAccepted()
        {
            var viewModel = new MessageViewModel(CreateMessage(), 1);

            bool raised = false;

            viewModel.PropertyChanged += (senderObject, eventArguments) =>
            {
                if (eventArguments.PropertyName == nameof(viewModel.BothAccepted))
                {
                    raised = true;
                }
            };

            viewModel.AcceptedBy = new[] { 1, 2 };

            Assert.True(raised);
        }

        [Fact]
        public void MessageViewModelIsRead_setter_updatesValue()
        {
            var viewModel = new MessageViewModel(CreateMessage(), 1);

            viewModel.IsRead = true;

            Assert.True(viewModel.IsRead);
        }

        [Fact]
        public void MessageViewModel_settingIsAccepted_isUpdated()
        {
            var message = CreateMessage() with { isAccepted = true };

            var viewModel = new MessageViewModel(message, 1);

            Assert.True(viewModel.IsAccepted);

            viewModel.IsAccepted = false;

            Assert.False(viewModel.IsAccepted);
        }

        [Fact]
        public void MessageViewModel_LessThanTwoUsersAccepte_bothAcceptedFalse()
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
        public void MessageViewModel_isMineToAlignment_returnsRightForMineAndLeftForOthers()
        {
            var viewModel = new MessageViewModel(CreateMessage(), 1);

            Assert.NotEqual(
                viewModel.IsMineToCornerRadius(true),
                viewModel.IsMineToCornerRadius(false)
            );
        }

        [Fact]
        public void MessageViewModel_isMineToTheirsOnlyBorderThickness_returnsCorrectThicknessForMineAndOthers()
        {
            var viewModel = new MessageViewModel(CreateMessage(), 1);

            Assert.Equal(new Thickness(0), viewModel.IsMineToTheirsOnlyBorderThickness(true));
            Assert.Equal(new Thickness(1), viewModel.IsMineToTheirsOnlyBorderThickness(false));
        }
    }
}