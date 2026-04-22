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
        private MessageDataTransferObject CreateMessage()
        {
            int defaultMessageId = 1;
            int targetConversationId = 10;
            int defaultSenderId = 2;
            int defaultReceiverId = 1;
            int testYear = 2026;
            int testMonth = 1;
            int testDay = 1;
            int testHour = 14;
            int testMinute = 30;
            int testSecond = 0;
            int missingIdentifier = -1;
            string textContent = "hello";

            return new MessageDataTransferObject(
                defaultMessageId,
                targetConversationId,
                defaultSenderId,
                defaultReceiverId,
                new DateTime(testYear, testMonth, testDay, testHour, testMinute, testSecond),
                textContent,
                MessageType.MessageText,
                null,
                false,
                false,
                false,
                false,
                missingIdentifier,
                missingIdentifier);
        }

        [Fact]
        public void IsMine_SenderMatchesCurrentUser_ReturnsTrue()
        {
            int currentUserId = 1;
            var messageData = CreateMessage() with { senderId = currentUserId };

            var viewModel = new MessageViewModel(messageData, currentUserId);

            Assert.True(viewModel.IsMine);
        }

        [Fact]
        public void IsMine_SenderDiffersFromCurrentUser_ReturnsFalse()
        {
            int currentUserId = 1;
            int externalUserId = 99;
            var messageData = CreateMessage() with { senderId = externalUserId };

            var viewModel = new MessageViewModel(messageData, currentUserId);

            Assert.False(viewModel.IsMine);
        }

        [Fact]
        public void IsRead_DefaultInitialization_ReturnsFalse()
        {
            int currentUserId = 1;
            var viewModel = new MessageViewModel(CreateMessage(), currentUserId);

            Assert.False(viewModel.IsRead);
        }

        [Fact]
        public void IsResolved_ValueChanged_RaisesPropertyChanged()
        {
            int currentUserId = 1;
            var viewModel = new MessageViewModel(CreateMessage(), currentUserId);

            bool eventRaised = false;

            viewModel.PropertyChanged += (eventSender, propertyChangedEventArgs) =>
            {
                if (propertyChangedEventArgs.PropertyName == nameof(viewModel.IsResolved))
                {
                    eventRaised = true;
                }
            };

            viewModel.IsResolved = true;

            Assert.True(viewModel.IsResolved);
            Assert.True(eventRaised);
        }

        [Fact]
        public void BothAccepted_TwoParticipantsAccepted_ReturnsTrue()
        {
            int currentUserId = 1;
            int firstUserIdentifier = 1;
            int secondUserIdentifier = 2;
            var viewModel = new MessageViewModel(CreateMessage(), currentUserId);

            viewModel.AcceptedBy = new[] { firstUserIdentifier, secondUserIdentifier };

            Assert.True(viewModel.BothAccepted);
        }

        [Fact]
        public void AcceptedBy_ValueChanged_RaisesBothAcceptedPropertyChanged()
        {
            int currentUserId = 1;
            int firstUserIdentifier = 1;
            int secondUserIdentifier = 2;
            var viewModel = new MessageViewModel(CreateMessage(), currentUserId);

            bool eventRaised = false;

            viewModel.PropertyChanged += (eventSender, propertyChangedEventArgs) =>
            {
                if (propertyChangedEventArgs.PropertyName == nameof(viewModel.BothAccepted))
                {
                    eventRaised = true;
                }
            };

            viewModel.AcceptedBy = new[] { firstUserIdentifier, secondUserIdentifier };

            Assert.True(eventRaised);
        }

        [Fact]
        public void IsRead_PropertyIsMutable_CanBeUpdated()
        {
            int currentUserId = 1;
            var viewModel = new MessageViewModel(CreateMessage(), currentUserId);

            viewModel.IsRead = true;

            Assert.True(viewModel.IsRead);
        }

        [Fact]
        public void IsAccepted_MappedFromDTO_IsUpdatable()
        {
            int currentUserId = 1;
            var messageData = CreateMessage() with { isAccepted = true };

            var viewModel = new MessageViewModel(messageData, currentUserId);

            Assert.True(viewModel.IsAccepted);

            viewModel.IsAccepted = false;

            Assert.False(viewModel.IsAccepted);
        }

        [Fact]
        public void BothAccepted_LessThenTwoAccepted_ReturnsFalse()
        {
            int currentUserId = 1;
            int singleUserIdentifier = 1;
            var viewModel = new MessageViewModel(CreateMessage(), currentUserId);

            viewModel.AcceptedBy = new[] { singleUserIdentifier };

            Assert.False(viewModel.BothAccepted);
        }

        [Fact]
        public void IsMineToAlignment_ValidInput_ReturnsCorrectAlignment()
        {
            int currentUserId = 1;
            bool belongsToCurrentUser = true;
            bool belongsToExternalUser = false;
            var viewModel = new MessageViewModel(CreateMessage(), currentUserId);

            Assert.Equal(HorizontalAlignment.Right, viewModel.IsMineToAlignment(belongsToCurrentUser));
            Assert.Equal(HorizontalAlignment.Left, viewModel.IsMineToAlignment(belongsToExternalUser));
        }

        [Fact]
        public void IsMineToCornerRadius_BooleanValues_ReturnDifferentShapes()
        {
            int currentUserId = 1;
            bool belongsToCurrentUser = true;
            bool belongsToExternalUser = false;
            var viewModel = new MessageViewModel(CreateMessage(), currentUserId);

            Assert.NotEqual(
                viewModel.IsMineToCornerRadius(belongsToCurrentUser),
                viewModel.IsMineToCornerRadius(belongsToExternalUser)
            );
        }

        [Fact]
        public void IsMineToBorderThickness_ValidInput_ReturnsCorrectThickness()
        {
            int currentUserId = 1;
            bool belongsToCurrentUser = true;
            bool belongsToExternalUser = false;
            double noThickness = 0;
            double defaultThickness = 1;
            var viewModel = new MessageViewModel(CreateMessage(), currentUserId);

            Assert.Equal(new Thickness(noThickness), viewModel.IsMineToTheirsOnlyBorderThickness(belongsToCurrentUser));
            Assert.Equal(new Thickness(defaultThickness), viewModel.IsMineToTheirsOnlyBorderThickness(belongsToExternalUser));
        }
    }
}