using System;
using BookingBoardgamesILoveBan.Src.Chat.ViewModel;
using Xunit;

namespace BookingBoardgamesILoveBan.Tests.Chat
{
    public class ConversationPreviewModelTests
    {
        private ConversationPreviewModel CreateModel()
        {
            int targetConversationId = 1;
            string displayName = "John Doe";
            string initials = "JD";
            string lastMessageText = "hello";
            int testYear = 2024;
            int testMonth = 1;
            int testDay = 1;
            int testHour = 10;
            int testMinute = 30;
            int testSecond = 0;
            int unreadCount = 3;
            string avatarImageName = "avatar.png";

            return new ConversationPreviewModel(
                targetConversationId,
                displayName,
                initials,
                lastMessageText,
                new DateTime(testYear, testMonth, testDay, testHour, testMinute, testSecond),
                unreadCount,
                avatarImageName);
        }

        [Fact]
        public void TimestampString_ValidTimestamp_ReturnsCorrectFormat()
        {
            var previewModel = CreateModel();
            string expectedFormat = "10:30";

            Assert.Equal(expectedFormat, previewModel.TimestampString);
        }

        [Fact]
        public void LastMessageText_ValueChanged_RaisesPropertyChanged()
        {
            var previewModel = CreateModel();
            string newMessageText = "new message";

            bool eventRaised = false;
            previewModel.PropertyChanged += (eventSender, propertyChangedEventArgs) =>
            {
                if (propertyChangedEventArgs.PropertyName == nameof(previewModel.LastMessageText))
                {
                    eventRaised = true;
                }
            };

            previewModel.LastMessageText = newMessageText;

            Assert.True(eventRaised);
        }

        [Fact]
        public void Timestamp_ValueChanged_RaisesDependentPropertyChanges()
        {
            var previewModel = CreateModel();
            int testYear = 2024;
            int testMonth = 1;
            int testDay = 1;
            int testHour = 11;
            int testMinute = 0;
            int testSecond = 0;

            bool timestampEventRaised = false;
            bool stringEventRaised = false;

            previewModel.PropertyChanged += (eventSender, propertyChangedEventArgs) =>
            {
                if (propertyChangedEventArgs.PropertyName == nameof(previewModel.Timestamp))
                {
                    timestampEventRaised = true;
                }

                if (propertyChangedEventArgs.PropertyName == nameof(previewModel.TimestampString))
                {
                    stringEventRaised = true;
                }
            };

            previewModel.Timestamp = new DateTime(testYear, testMonth, testDay, testHour, testMinute, testSecond);

            Assert.True(timestampEventRaised);
            Assert.True(stringEventRaised);
        }

        [Fact]
        public void UnreadCount_ValueChanged_RaisesPropertyChanged()
        {
            var previewModel = CreateModel();
            int newUnreadCount = 10;

            bool eventRaised = false;
            previewModel.PropertyChanged += (eventSender, propertyChangedEventArgs) =>
            {
                if (propertyChangedEventArgs.PropertyName == nameof(previewModel.UnreadCount))
                {
                    eventRaised = true;
                }
            };

            previewModel.UnreadCount = newUnreadCount;

            Assert.True(eventRaised);
        }

        [Fact]
        public void SetProperties_SameValues_ExecutesWithoutError()
        {
            var previewModel = CreateModel();
            string testMessage = "hello";
            int testUnreadCount = 3;

            Exception executionException = Record.Exception(() =>
            {
                previewModel.LastMessageText = testMessage;
                previewModel.UnreadCount = testUnreadCount;
                previewModel.Timestamp = previewModel.Timestamp;
            });

            Assert.Null(executionException);
        }

        [Fact]
        public void TimestampString_TimestampUpdated_ReturnsNewFormattedString()
        {
            var previewModel = CreateModel();
            int testYear = 2024;
            int testMonth = 1;
            int testDay = 1;
            int testHour = 15;
            int testMinute = 45;
            int testSecond = 0;
            string expectedFormat = "15:45";

            previewModel.Timestamp = new DateTime(testYear, testMonth, testDay, testHour, testMinute, testSecond);

            Assert.Equal(expectedFormat, previewModel.TimestampString);
        }
    }
}