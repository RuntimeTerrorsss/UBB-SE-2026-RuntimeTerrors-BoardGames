using System;
using System.Collections.Generic;
using System.Linq;
using BookingBoardgamesILoveBan.Src.Chat.DTO;
using BookingBoardgamesILoveBan.Src.Chat.Model;
using BookingBoardgamesILoveBan.Src.Chat.ViewModel;
using BookingBoardgamesILoveBan.Src.Enum;
using Xunit;

namespace BookingBoardgamesILoveBan.Tests.Chat
{
    public class ChatViewModelTests
    {
        private ChatViewModel CreateViewModel()
        {
            int currentUserId = 1;
            return new ChatViewModel(currentUserId);
        }

        private ConversationPreviewModel CreateConversation()
        {
            int targetConversationId = 1;
            string displayName = "John Doe";
            string initials = "JD";
            string lastMessageText = "hi";
            int unreadCount = 0;
            string avatarImageName = "avatar.png";

            return new ConversationPreviewModel(
                targetConversationId,
                displayName,
                initials,
                lastMessageText,
                DateTime.Now,
                unreadCount,
                avatarImageName);
        }

        private MessageDataTransferObject CreateMessage(int targetConversationId = 1)
        {
            int defaultMessageId = 1;
            int defaultSenderId = 1;
            int defaultReceiverId = 2;
            int missingIdentifier = -1;
            string textContent = "hello";

            return new MessageDataTransferObject(
                defaultMessageId,
                targetConversationId,
                defaultSenderId,
                defaultReceiverId,
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
        public void LoadConversation_ValidData_SetsHeaderAndMessages()
        {
            var viewModel = CreateViewModel();
            var conversation = CreateConversation();
            int testUnreadCount = 1;
            int expectedTotalMessages = 2;

            var messages = new List<MessageDataTransferObject>
            {
                CreateMessage(),
                CreateMessage()
            };

            viewModel.LoadConversation(conversation, messages, testUnreadCount);

            Assert.Equal(expectedTotalMessages, viewModel.Messages.Count);
        }

        [Fact]
        public void SendMessage_ValidInput_AddsMessageAndClearsInput()
        {
            var viewModel = CreateViewModel();
            int testUnreadCount = 0;
            viewModel.LoadConversation(CreateConversation(), new List<MessageDataTransferObject>(), testUnreadCount);

            bool eventInvoked = false;
            viewModel.MessageSent += (messageData) => eventInvoked = true;

            viewModel.InputText = "hello world";
            viewModel.SendMessage();

            Assert.True(eventInvoked);
        }

        [Fact]
        public void SendMessage_EmptyInput_DoesNotAddMessage()
        {
            var viewModel = CreateViewModel();
            int testUnreadCount = 0;
            viewModel.LoadConversation(CreateConversation(), new List<MessageDataTransferObject>(), testUnreadCount);

            viewModel.InputText = "";
            viewModel.SendMessage();

            Assert.Empty(viewModel.Messages);
        }

        [Fact]
        public void HandleIncomingMessage_MatchingConversation_AddsMessage()
        {
            var viewModel = CreateViewModel();
            int testUnreadCount = 0;
            int targetConversationId = 1;
            viewModel.LoadConversation(CreateConversation(), new List<MessageDataTransferObject>(), testUnreadCount);

            var message = CreateMessage(targetConversationId);

            viewModel.HandleIncomingMessage(message);

            Assert.Single(viewModel.Messages);
        }

        [Fact]
        public void HandleIncomingMessage_DifferentConversation_IgnoresMessage()
        {
            var viewModel = CreateViewModel();
            int testUnreadCount = 0;
            int invalidConversationId = 99;
            viewModel.LoadConversation(CreateConversation(), new List<MessageDataTransferObject>(), testUnreadCount);

            var message = CreateMessage(invalidConversationId);

            viewModel.HandleIncomingMessage(message);

            Assert.Empty(viewModel.Messages);
        }

        [Fact]
        public void HandleIncomingMessage_DuplicateMessage_DoesNotInsertDuplicate()
        {
            var viewModel = CreateViewModel();
            int testUnreadCount = 0;
            int targetConversationId = 1;
            viewModel.LoadConversation(CreateConversation(), new List<MessageDataTransferObject>(), testUnreadCount);

            var message = CreateMessage(targetConversationId);

            viewModel.HandleIncomingMessage(message);
            viewModel.HandleIncomingMessage(message);

            Assert.Single(viewModel.Messages);
        }

        [Fact]
        public void ResolveBookingRequest_ValidRequest_InvokesEvent()
        {
            var viewModel = CreateViewModel();
            int testUnreadCount = 0;
            int targetMessageId = 1;
            bool isAccepted = true;
            viewModel.LoadConversation(CreateConversation(), new List<MessageDataTransferObject> { CreateMessage() }, testUnreadCount);

            bool eventInvoked = false;
            viewModel.BookingRequestUpdate += (messageIdentifier, conversationIdentifier, acceptStatus, resolveStatus) => eventInvoked = true;

            viewModel.ResolveBookingRequest(targetMessageId, isAccepted);

            Assert.True(eventInvoked);
        }

        [Fact]
        public void ResolveBookingRequest_MissingMessage_ExecutesWithoutError()
        {
            var viewModel = CreateViewModel();
            int missingMessageId = 999;
            bool isAccepted = true;

            Exception executionException = Record.Exception(() => viewModel.ResolveBookingRequest(missingMessageId, isAccepted));

            Assert.Null(executionException);
        }

        [Fact]
        public void UpdateCashAgreement_ValidAgreement_InvokesEvent()
        {
            var viewModel = CreateViewModel();
            int testUnreadCount = 0;
            int targetMessageId = 1;
            viewModel.LoadConversation(CreateConversation(), new List<MessageDataTransferObject> { CreateMessage() }, testUnreadCount);

            bool eventInvoked = false;
            viewModel.CashAgreementAccept += (messageIdentifier, conversationIdentifier) => eventInvoked = true;

            viewModel.UpdateCashAgreement(targetMessageId);

            Assert.True(eventInvoked);
        }

        [Fact]
        public void UpdateCashAgreement_MissingMessage_ExecutesWithoutError()
        {
            var viewModel = CreateViewModel();
            int missingMessageId = 999;

            Exception executionException = Record.Exception(() => viewModel.UpdateCashAgreement(missingMessageId));

            Assert.Null(executionException);
        }

        [Fact]
        public void SendImage_ValidFile_InvokesEvent()
        {
            var viewModel = CreateViewModel();
            int testUnreadCount = 0;
            string testFileName = "file.png";
            viewModel.LoadConversation(CreateConversation(), new List<MessageDataTransferObject>(), testUnreadCount);

            bool eventInvoked = false;
            viewModel.MessageSent += (messageData) => eventInvoked = true;

            viewModel.SendImage(testFileName);

            Assert.True(eventInvoked);
        }

        [Fact]
        public void RaiseBookingRequestUpdate_ValidTrigger_InvokesEvent()
        {
            var viewModel = CreateViewModel();
            int targetMessageId = 1;
            int targetConversationId = 1;
            bool isAccepted = true;
            bool isResolved = false;

            bool eventInvoked = false;
            viewModel.BookingRequestUpdate += (messageIdentifier, conversationIdentifier, acceptStatus, resolveStatus) => eventInvoked = true;

            viewModel.RaiseBookingRequestUpdate(targetMessageId, targetConversationId, isAccepted, isResolved);

            Assert.True(eventInvoked);
        }

        [Fact]
        public void RaiseCashAgreementAccept_ValidTrigger_InvokesEvent()
        {
            var viewModel = CreateViewModel();
            int targetMessageId = 1;
            int targetConversationId = 1;

            bool eventInvoked = false;
            viewModel.CashAgreementAccept += (messageIdentifier, conversationIdentifier) => eventInvoked = true;

            viewModel.RaiseCashAgreementAccept(targetMessageId, targetConversationId);

            Assert.True(eventInvoked);
        }

        [Fact]
        public void RaiseMessageSent_ValidTrigger_InvokesEvent()
        {
            var viewModel = CreateViewModel();

            bool eventInvoked = false;
            viewModel.MessageSent += (messageData) => eventInvoked = true;

            viewModel.RaiseMessageSent(CreateMessage());

            Assert.True(eventInvoked);
        }

        [Fact]
        public void LoadConversation_ValidUnreadCount_SetsReadStatusCorrectly()
        {
            var viewModel = CreateViewModel();
            var conversation = CreateConversation();
            int testUnreadCount = 1;
            int firstMessageIndex = 0;
            int secondMessageIndex = 1;

            var messages = new List<MessageDataTransferObject>
            {
                CreateMessage(),
                CreateMessage()
            };

            viewModel.LoadConversation(conversation, messages, testUnreadCount);

            Assert.True(viewModel.Messages[firstMessageIndex].IsRead);
            Assert.False(viewModel.Messages[secondMessageIndex].IsRead);
        }

        [Fact]
        public void CanSend_WhiteSpaceInput_ReturnsFalse()
        {
            var viewModel = CreateViewModel();
            string whitespaceString = "   ";

            viewModel.InputText = whitespaceString;

            Assert.False(viewModel.CanSend);
        }

        [Fact]
        public void ProceedToPayment_ValidCall_ExecutesWithoutError()
        {
            var viewModel = CreateViewModel();
            int targetMessageId = 1;

            Exception executionException = Record.Exception(() => viewModel.ProceedToPayment(targetMessageId));

            Assert.Null(executionException);
        }

        [Fact]
        public void HandleIncomingMessage_DelayedDuplicate_DoesNotInsertDuplicate()
        {
            var viewModel = CreateViewModel();
            int testUnreadCount = 0;
            int timeDelayMilliseconds = 500;
            viewModel.LoadConversation(CreateConversation(), new List<MessageDataTransferObject>(), testUnreadCount);

            var originalMessage = CreateMessage();
            var duplicateMessage = CreateMessage();
            duplicateMessage = duplicateMessage with { sentAt = originalMessage.sentAt.AddMilliseconds(timeDelayMilliseconds) };

            viewModel.HandleIncomingMessage(originalMessage);
            viewModel.HandleIncomingMessage(duplicateMessage);

            Assert.Single(viewModel.Messages);
        }
    }
}