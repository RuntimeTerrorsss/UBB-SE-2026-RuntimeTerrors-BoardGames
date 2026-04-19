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
        public void TimestampString_ReturnCorrectFormat()
        {
            var model = CreateModel();

            Assert.Equal("10:30", model.TimestampString);
        }

        [Fact]
        public void LastMessageText_RaisePropertyChanged()
        {
            var model = CreateModel();

            bool raised = false;
            model.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(model.LastMessageText))
                {
                    raised = true;
                }
            };

            model.LastMessageText = "new message";

            Assert.True(raised);
        }

        [Fact]
        public void Timestamp_ChangeProperty_RaisedFlagTrue()
        {
            var model = CreateModel();

            bool timestampRaised = false;
            bool stringRaised = false;

            model.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(model.Timestamp))
                {
                    timestampRaised = true;
                }

                if (e.PropertyName == nameof(model.TimestampString))
                {
                    stringRaised = true;
                }
            };

            model.Timestamp = new DateTime(2024, 1, 1, 11, 0, 0);

            Assert.True(timestampRaised);
            Assert.True(stringRaised);
        }

        [Fact]
        public void UnreadCount_ChangeProperty_RaisedFlagTrue()
        {
            var model = CreateModel();

            bool raised = false;
            model.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(model.UnreadCount))
                {
                    raised = true;
                }
            };

            model.UnreadCount = 10;

            Assert.True(raised);
        }

        [Fact]
        public void Property_SettingSameValue_NotCrash()
        {
            var model = CreateModel();

            model.LastMessageText = "hello";
            model.UnreadCount = 3;
            model.Timestamp = model.Timestamp;
        }

        [Fact]
        public void TimestampString_TimestampChanges_Updates()
        {
            var model = CreateModel();

            model.Timestamp = new DateTime(2024, 1, 1, 15, 45, 0);

            Assert.Equal("15:45", model.TimestampString);
        }
    }
}