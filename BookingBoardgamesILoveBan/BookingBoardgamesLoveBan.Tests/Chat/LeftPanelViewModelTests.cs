using System;
using System.Collections.Generic;
using System.Linq;
using BookingBoardgamesILoveBan.Src.Chat.DTO;
using BookingBoardgamesILoveBan.Src.Chat.ViewModel;
using BookingBoardgamesILoveBan.Src.Enum;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using Moq;
using Xunit;

namespace BookingBoardgamesILoveBan.Tests.Chat
{
    public class LeftPanelViewModelTests
    {
        private Mock<IUserRepository> userServiceMock = new Mock<IUserRepository>();

        private LeftPanelViewModel CreateViewModel()
        {
            int defaultUserId = 1;
            string testName = "name";
            string testCountry = "country";
            string testCity = "city";
            string testStreet = "street";
            string testStreetNumber = "streetNumber";

            userServiceMock
                .Setup(userRepository => userRepository.GetById(It.IsAny<int>()))
                .Returns(new User(defaultUserId, testName, testCountry, testCity, testStreet, testStreetNumber));
            return new LeftPanelViewModel();
        }

        private IUserRepository CreateUserService()
        {
            var serviceMock = new Mock<IUserRepository>();
            int defaultUserId = 1;
            string testName = "name";
            string testCountry = "country";
            string testCity = "city";
            string testStreet = "street";
            string testStreetNumber = "streetNumber";

            serviceMock.Setup(userRepository => userRepository.GetById(It.IsAny<int>()))
                .Returns(new User(defaultUserId, testName, testCountry, testCity, testStreet, testStreetNumber));

            return serviceMock.Object;
        }

        private ConversationDataTransferObject CreateConversation(int targetConversationId = 1)
        {
            int firstParticipantId = 1;
            int secondParticipantId = 2;

            return new ConversationDataTransferObject(
                targetConversationId,
                new[] { firstParticipantId, secondParticipantId },
                new List<MessageDataTransferObject>(),
                new Dictionary<int, DateTime>
                {
                    { firstParticipantId, DateTime.MinValue },
                    { secondParticipantId, DateTime.MinValue }
                });
        }

        private MessageDataTransferObject CreateMessage(int targetConversationId = 1)
        {
            int defaultMessageId = 1;
            int senderIdentifier = 1;
            int receiverIdentifier = 2;
            int missingIdentifier = -1;
            string textContent = "hello";

            return new MessageDataTransferObject(
                defaultMessageId,
                targetConversationId,
                senderIdentifier,
                receiverIdentifier,
                DateTime.Now,
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
        public void HandleIncomingMessage_NewConversation_AddsToCollection()
        {
            var viewModel = CreateViewModel();
            string senderName = "John";

            viewModel.HandleIncomingMessage(CreateMessage(), senderName, userServiceMock.Object);

            Assert.Single(viewModel.Conversations);
        }

        [Fact]
        public void HandleIncomingMessage_ExistingConversation_UpdatesPreview()
        {
            var viewModel = CreateViewModel();
            var originalMessage = CreateMessage();
            string senderName = "John";
            string updatedContent = "updated";

            viewModel.HandleIncomingMessage(originalMessage, senderName, userServiceMock.Object);
            viewModel.HandleIncomingMessage(originalMessage with { content = updatedContent }, senderName, userServiceMock.Object);

            Assert.Single(viewModel.Conversations);
            Assert.Equal(updatedContent, viewModel.Conversations.First().LastMessageText);
        }

        [Fact]
        public void HandleIncomingMessage_ConversationNotSelected_IncrementsUnreadCount()
        {
            var viewModel = CreateViewModel();
            string senderName = "John";
            int expectedUnreadCount = 1;

            viewModel.HandleIncomingMessage(CreateMessage(), senderName, userServiceMock.Object);

            var targetConversation = viewModel.Conversations.First();
            Assert.Equal(expectedUnreadCount, targetConversation.UnreadCount);
        }

        [Fact]
        public void SelectedConversation_ValidSelection_SetsUnreadCountToZero()
        {
            var viewModel = CreateViewModel();
            string senderName = "John";
            int expectedZeroUnreadCount = 0;

            viewModel.HandleIncomingMessage(CreateMessage(), senderName, userServiceMock.Object);

            var targetConversation = viewModel.Conversations.First();
            viewModel.SelectedConversation = targetConversation;

            Assert.Equal(expectedZeroUnreadCount, targetConversation.UnreadCount);
        }

        [Fact]
        public void SearchText_ValidSearchString_FiltersConversations()
        {
            var viewModel = CreateViewModel();
            int alternativeConversationId = 2;
            string firstSenderName = "John";
            string secondSenderName = "Mike";
            string searchString = "John";

            viewModel.HandleIncomingMessage(CreateMessage(), firstSenderName, userServiceMock.Object);
            viewModel.HandleIncomingMessage(CreateMessage() with { conversationId = alternativeConversationId }, secondSenderName, userServiceMock.Object);

            viewModel.SearchText = searchString;

            Assert.Single(viewModel.Conversations);
        }

        [Fact]
        public void HandleIncomingConversation_ValidConversation_AddsToCollection()
        {
            var viewModel = CreateViewModel();
            var userService = CreateUserService();
            var newConversation = CreateConversation();
            string senderName = "John";
            int currentUserId = 1;

            viewModel.HandleIncomingConversation(newConversation, senderName, currentUserId, userService);

            Assert.Single(viewModel.Conversations);
        }

        [Fact]
        public void HandleIncomingConversation_DuplicateConversation_DoesNotAddDuplicate()
        {
            var viewModel = CreateViewModel();
            var userService = CreateUserService();
            var newConversation = CreateConversation();
            string senderName = "John";
            int currentUserId = 1;

            viewModel.HandleIncomingConversation(newConversation, senderName, currentUserId, userService);
            viewModel.HandleIncomingConversation(newConversation, senderName, currentUserId, userService);

            Assert.Single(viewModel.Conversations);
        }

        [Fact]
        public void SortConversationsByTimestamp_ValidConversations_MaintainsTotalCount()
        {
            var viewModel = CreateViewModel();
            var userService = CreateUserService();
            int firstConversationId = 1;
            int secondConversationId = 2;
            int currentUserId = 1;
            int expectedTotalConversations = 2;

            var firstConversation = CreateConversation(firstConversationId);
            var secondConversation = CreateConversation(secondConversationId);

            viewModel.HandleIncomingConversation(firstConversation, "A", currentUserId, userService);
            viewModel.HandleIncomingConversation(secondConversation, "B", currentUserId, userService);

            viewModel.SortConversationsByTimestamp();

            Assert.Equal(expectedTotalConversations, viewModel.Conversations.Count);
        }

        [Fact]
        public void UIStates_ConversationAdded_UpdatesVisibilityFlags()
        {
            var viewModel = CreateViewModel();
            string senderName = "John";

            viewModel.HandleIncomingMessage(CreateMessage(), senderName, userServiceMock.Object);

            Assert.False(viewModel.IsEmptyStateVisible);
        }

        [Fact]
        public void ApplyFilter_ValidList_ReordersCorrectly()
        {
            var viewModel = CreateViewModel();
            int firstConversationId = 1;
            int secondConversationId = 2;
            string expectedFirstDisplayName = "A";

            viewModel.HandleIncomingMessage(CreateMessage(firstConversationId), "B", userServiceMock.Object);
            viewModel.HandleIncomingMessage(CreateMessage(secondConversationId), "A", userServiceMock.Object);

            viewModel.SearchText = "";

            Assert.Equal(expectedFirstDisplayName, viewModel.Conversations[0].DisplayName);
        }

        [Fact]
        public void ApplyFilter_TimestampsChanged_MovesItemsAccordingly()
        {
            var viewModel = CreateViewModel();
            int firstConversationId = 1;
            int secondConversationId = 2;
            int firstIndex = 0;
            int secondIndex = 1;
            string expectedFirstDisplayName = "John";

            viewModel.HandleIncomingMessage(CreateMessage(firstConversationId), "John", userServiceMock.Object);
            viewModel.HandleIncomingMessage(CreateMessage(secondConversationId), "Mike", userServiceMock.Object);

            viewModel.Conversations[firstIndex].Timestamp = DateTime.MinValue;
            viewModel.Conversations[secondIndex].Timestamp = DateTime.Now;

            viewModel.SortConversationsByTimestamp();

            Assert.Equal(expectedFirstDisplayName, viewModel.Conversations.First().DisplayName);
        }

        [Fact]
        public void HandleIncomingMessage_ConversationIsSelected_DoesNotIncreaseUnreadCount()
        {
            var viewModel = CreateViewModel();
            var message = CreateMessage();
            string senderName = "John";
            string newContent = "new";
            int expectedZeroUnreadCount = 0;

            viewModel.HandleIncomingMessage(message, senderName, userServiceMock.Object);
            var targetConversation = viewModel.Conversations.First();
            viewModel.SelectedConversation = targetConversation;
            viewModel.HandleIncomingMessage(message with { content = newContent }, senderName, userServiceMock.Object);

            Assert.Equal(expectedZeroUnreadCount, targetConversation.UnreadCount);
        }

        [Fact]
        public void HandleIncomingConversation_ContainsMessages_SetsPreviewAndTimestamp()
        {
            var viewModel = CreateViewModel();
            var userService = CreateUserService();
            var message = CreateMessage();
            int targetConversationId = 1;
            int firstParticipantId = 1;
            int secondParticipantId = 2;
            string senderName = "John";
            string expectedText = "hello";

            var conversation = new ConversationDataTransferObject(
                targetConversationId,
                new[] { firstParticipantId, secondParticipantId },
                new List<MessageDataTransferObject> { message },
                new Dictionary<int, DateTime>
                {
                    { firstParticipantId, DateTime.MinValue },
                    { secondParticipantId, DateTime.MinValue }
                }
            );

            viewModel.HandleIncomingConversation(conversation, senderName, firstParticipantId, userService);

            var previewModel = viewModel.Conversations.First();

            Assert.Equal(expectedText, previewModel.LastMessageText);
            Assert.Equal(message.sentAt, previewModel.Timestamp);
        }

        [Fact]
        public void SortConversationsByTimestamp_MultipleTimestamps_SortsDescending()
        {
            var viewModel = CreateViewModel();
            var userService = CreateUserService();
            int secondConversationId = 2;
            int timeAddedMinutes = 1;

            var firstMessage = CreateMessage();
            var secondMessage = CreateMessage(secondConversationId) with { sentAt = DateTime.Now.AddMinutes(timeAddedMinutes) };

            viewModel.HandleIncomingMessage(firstMessage, "A", userServiceMock.Object);
            viewModel.HandleIncomingMessage(secondMessage, "B", userServiceMock.Object);

            viewModel.SortConversationsByTimestamp();

            var orderedList = viewModel.Conversations.ToList();

            Assert.True(orderedList[0].Timestamp >= orderedList[1].Timestamp);
        }

        [Fact]
        public void RaisePropertyChanged_ValidProperty_InvokesEvent()
        {
            var viewModel = CreateViewModel();
            string testPropertyName = "TestProp";
            bool eventTriggered = false;

            viewModel.PropertyChanged += (eventSender, propertyChangedEventArgs) =>
            {
                if (propertyChangedEventArgs.PropertyName == testPropertyName)
                {
                    eventTriggered = true;
                }
            };

            viewModel.RaisePropertyChanged(testPropertyName);

            Assert.True(eventTriggered);
        }

        [Fact]
        public void SearchText_NoMatchesFound_ShowsNoMatchesState()
        {
            var viewModel = CreateViewModel();
            string senderName = "John";
            string invalidSearchString = "ZZZ";

            viewModel.HandleIncomingMessage(CreateMessage(), senderName, userServiceMock.Object);

            viewModel.SearchText = invalidSearchString;

            Assert.True(viewModel.IsNoMatchesVisible);
        }

        [Fact]
        public void SelectedConversation_ZeroUnreadCount_DoesNotChangeCount()
        {
            var viewModel = CreateViewModel();
            string senderName = "John";
            int expectedZeroUnreadCount = 0;

            viewModel.HandleIncomingMessage(CreateMessage(), senderName, userServiceMock.Object);

            var targetConversation = viewModel.Conversations.First();
            targetConversation.UnreadCount = expectedZeroUnreadCount;

            viewModel.SelectedConversation = targetConversation;

            Assert.Equal(expectedZeroUnreadCount, targetConversation.UnreadCount);
        }

        [Fact]
        public void SearchText_ValidFilter_RemovesNonMatchingItems()
        {
            var viewModel = CreateViewModel();
            int firstConversationId = 1;
            int secondConversationId = 2;
            string firstSenderName = "John";
            string secondSenderName = "Mike";

            viewModel.HandleIncomingMessage(CreateMessage(firstConversationId), firstSenderName, userServiceMock.Object);
            viewModel.HandleIncomingMessage(CreateMessage(secondConversationId), secondSenderName, userServiceMock.Object);

            viewModel.SearchText = firstSenderName;

            Assert.DoesNotContain(viewModel.Conversations, conversationItem => conversationItem.DisplayName == secondSenderName);
        }
    }
}