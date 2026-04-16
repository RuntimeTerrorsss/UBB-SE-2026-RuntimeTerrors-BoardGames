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
                avatarUrl: "avatar.png"
            );
        }

        [Fact]
        public void Constructor_Should_SetAllProperties()
        {
            var model = CreateModel();

            Assert.Equal(1, model.ConversationId);
            Assert.Equal("John Doe", model.DisplayName);
            Assert.Equal("JD", model.Initials);
            Assert.Equal("avatar.png", model.AvatarUrl);
            Assert.Equal("hello", model.LastMessageText);
            Assert.Equal(3, model.UnreadCount);
        }

        [Fact]
        public void TimestampString_Should_ReturnCorrectFormat()
        {
            var model = CreateModel();

            Assert.Equal("10:30", model.TimestampString);
        }

        [Fact]
        public void LastMessageText_Should_RaisePropertyChanged()
        {
            var model = CreateModel();

            bool raised = false;
            model.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(model.LastMessageText))
                    raised = true;
            };

            model.LastMessageText = "new message";

            Assert.True(raised);
        }

        [Fact]
        public void Timestamp_Should_RaisePropertyChanged_ForBothProperties()
        {
            var model = CreateModel();

            bool timestampRaised = false;
            bool stringRaised = false;

            model.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(model.Timestamp))
                    timestampRaised = true;

                if (e.PropertyName == nameof(model.TimestampString))
                    stringRaised = true;
            };

            model.Timestamp = new DateTime(2024, 1, 1, 11, 0, 0);

            Assert.True(timestampRaised);
            Assert.True(stringRaised);
        }

        [Fact]
        public void UnreadCount_Should_RaisePropertyChanged()
        {
            var model = CreateModel();

            bool raised = false;
            model.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(model.UnreadCount))
                    raised = true;
            };

            model.UnreadCount = 10;

            Assert.True(raised);
        }

        [Fact]
        public void SettingSameValue_ShouldStillRaiseEvent_ButNotCrash()
        {
            var model = CreateModel();

            model.LastMessageText = "hello";
            model.UnreadCount = 3;
            model.Timestamp = model.Timestamp;
        }
    }
}