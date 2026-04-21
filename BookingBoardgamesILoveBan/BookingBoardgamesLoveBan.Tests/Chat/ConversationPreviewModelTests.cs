using System;
using BookingBoardgamesILoveBan.Src.Chat.ViewModel;
using Xunit;

namespace BookingBoardgamesILoveBan.Tests.Chat
{
    public class ConversationPreviewModelTests
    {
        private ConversationPreviewModel CreateModel()
        {
            return new ConversationPreviewModel(
                conversationId: 1,
                displayName: "John Doe",
                initials: "JD",
                lastMessageTextInput: "hello",
                timestampInput: new DateTime(2024, 1, 1, 10, 30, 0),
                unreadCountInput: 3,
                avatarUrl: "avatar.png");
        } 

        [Fact]
        public void MessageViewModel_timestampString_formatsTimeAsHoursAndMinutes()
        {
            var model = CreateModel();

            Assert.Equal("10:30", model.TimestampString);
        }

        [Fact]
        public void MessageViewModel_lastMessageTextChanges_raisesPropertyChangedEvent()
        {
            var model = CreateModel();

            bool raised = false;
            model.PropertyChanged += (senderObject, eventArguments) =>
            {
                if (eventArguments.PropertyName == nameof(model.LastMessageText))
                {
                    raised = true;
                }
            };

            model.LastMessageText = "new message";

            Assert.True(raised);
        }

        [Fact]
        public void MessageViewModel_timestampSetter_raisesPropertyChangedForTimestampAndTimestampString()
        {
            var model = CreateModel();

            bool timestampRaised = false;
            bool stringRaised = false;

            model.PropertyChanged += (senderObject, eventArguments) =>
            {
                if (eventArguments.PropertyName == nameof(model.Timestamp))
                {
                    timestampRaised = true;
                }

                if (eventArguments.PropertyName == nameof(model.TimestampString))
                {
                    stringRaised = true;
                }
            };

            model.Timestamp = new DateTime(2024, 1, 1, 11, 0, 0);

            Assert.True(timestampRaised);
            Assert.True(stringRaised);
        }

        [Fact]
        public void MessageViewModel_unreadCountSetter_raisesPropertyChanged()
        {
            var model = CreateModel();

            bool raised = false;
            model.PropertyChanged += (senderObject, eventArguments) =>
            {
                if (eventArguments.PropertyName == nameof(model.UnreadCount))
                {
                    raised = true;
                }
            };

            model.UnreadCount = 10;

            Assert.True(raised);
        }

        [Fact]
        public void MessageViewModel_settingSamePropertyValue_doesNotThrowExceptionOrChangeState()
        {
            var model = CreateModel();

            model.LastMessageText = "hello";
            model.UnreadCount = 3;
            model.Timestamp = model.Timestamp;
        }

        [Fact]
        public void MessageViewModel_timestampChange_updatesTimestampString()
        {
            var model = CreateModel();

            model.Timestamp = new DateTime(2024, 1, 1, 15, 45, 0);

            Assert.Equal("15:45", model.TimestampString);
        }
    }
}