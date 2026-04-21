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
            return new ChatViewModel(1);
        }

        private ConversationPreviewModel CreateConversation()
        {
            return new ConversationPreviewModel(
                conversationId: 1,
                displayName: "John Doe",
                initials: "JD",
                lastMessageTextInput: "hi",
                timestampInput: DateTime.Now,
                unreadCountInput: 0,
                avatarUrl: "avatar.png");
        }

        private MessageDTO CreateMessage(int convId = 1)
        {
            return new MessageDTO(
                id: 1,
                conversationId: convId,
                senderId: 1,
                receiverId: 2,
                sentAt: DateTime.Now,
                content: "hello",
                type: MessageType.MessageText,
                imageUrl: null,
                isAccepted: false,
                isResolved: false,
                isAcceptedBySeller: false,
                isAcceptedByBuyer: false,
                requestId: -1,
                paymentId: -1);
        }

        [Fact]
        public void ChatPageViewModelLoadConversation_withMessages_setsMessageListCount()
        {
            var viewModel = CreateViewModel();
            var conversation = CreateConversation();

            var messages = new List<MessageDTO>
            {
                CreateMessage(),
                CreateMessage()
            };

            viewModel.LoadConversation(conversation, messages, theirUnreadCount: 1);
            Assert.Equal(2, viewModel.Messages.Count);
        }

        [Fact]
        public void ChatPageViewModelSendMessage_withValidInput_raisesMessageSentEvent()
        {
            var viewModel = CreateViewModel();
            viewModel.LoadConversation(CreateConversation(), new List<MessageDTO>(), theirUnreadCount: 0);

            bool invoked = false;
            viewModel.MessageSent += argument1 => invoked = true;

            viewModel.InputText = "hello world";
            viewModel.SendMessage();

            Assert.True(invoked);
        }

        [Fact]
        public void ChatPageViewModelSendMessage_emptyInput_doesNotAddMessage()
        {
            var viewModel = CreateViewModel();
            viewModel.LoadConversation(CreateConversation(), new List<MessageDTO>(), theirUnreadCount: 0);

            viewModel.InputText = "";

            viewModel.SendMessage();

            Assert.Empty(viewModel.Messages);
        }

        [Fact]
        public void ChatPageViewModelHandleIncomingMessage_sameConversation_addsMessageToCollection()
        {
            var viewModel = CreateViewModel();
            viewModel.LoadConversation(CreateConversation(), new List<MessageDTO>(), theirUnreadCount: 0);

            var message = CreateMessage(1);

            viewModel.HandleIncomingMessage(message);

            Assert.Single(viewModel.Messages);
        }

        [Fact]
        public void ChatPageViewModelHandleIncomingMessage_differentConversation_doesNotAddMessageToCollection()
        {
            var viewModel = CreateViewModel();
            viewModel.LoadConversation(CreateConversation(), new List<MessageDTO>(), theirUnreadCount: 0);

            var message = CreateMessage(99);

            viewModel.HandleIncomingMessage(message);

            Assert.Empty(viewModel.Messages);
        }

        [Fact]
        public void ChatPageViewModelHandleIncomingMessage_duplicateMessage_doesNotAddDuplicateToCollection()
        {
            var viewModel = CreateViewModel();
            viewModel.LoadConversation(CreateConversation(), new List<MessageDTO>(), theirUnreadCount: 0);

            var message = CreateMessage(1);

            viewModel.HandleIncomingMessage(message);
            viewModel.HandleIncomingMessage(message);

            Assert.Single(viewModel.Messages);
        }

        [Fact]
        public void ChatPageViewModel_resolveBookingRequest_raisesBookingRequestUpdateEvent()
        {
            var viewModel = CreateViewModel();
            viewModel.LoadConversation(CreateConversation(), new List<MessageDTO> { CreateMessage() }, theirUnreadCount: 0);

            bool invoked = false;
            viewModel.BookingRequestUpdate += (argument1, argument2, argument3, argument4) => invoked = true;
            int messageId = 1;
            viewModel.ResolveBookingRequest(messageId, true);

            Assert.True(invoked);
        }

        [Fact]
        public void ChatPageViewModelResolveBookingRequest_missingMessageId_doesNotThrowException()
        {
            var viewModel = CreateViewModel();
            viewModel.ResolveBookingRequest(999, true);
        }

        [Fact]
        public void ChatPageViewModelUpdateCashAgreement_raisesCashAgreementAcceptEvent()
        {
            var viewModel = CreateViewModel();
            viewModel.LoadConversation(CreateConversation(), new List<MessageDTO> { CreateMessage() }, theirUnreadCount: 0);

            bool invoked = false;
            viewModel.CashAgreementAccept += (argument1, argument2) => invoked = true;

            viewModel.UpdateCashAgreement(1);

            Assert.True(invoked);
        }

        [Fact]
        public void ChatPageViewModelUpdateCashAgreement_missingMessageId_doesNotThrowException()
        {
            var viewModel = CreateViewModel();

            viewModel.UpdateCashAgreement(999);
        }

        [Fact]
        public void ChatPageViewModel_sendImage_raisesMessageSentEvent()
        {
            var viewModel = CreateViewModel();
            viewModel.LoadConversation(CreateConversation(), new List<MessageDTO>(), theirUnreadCount: 0);

            bool invoked = false;
            viewModel.MessageSent += argument1 => invoked = true;

            viewModel.SendImage("file.png");

            Assert.True(invoked);
        }

        [Fact]
        public void ChatPageViewModel_raiseBookingRequestUpdate_raisesBookingRequestUpdateEvent()
        {
            var viewModel = CreateViewModel();

            bool invoked = false;
            viewModel.BookingRequestUpdate += (argument1, argument2, argument3, argument4) => invoked = true;
            int messageId = 1, conversationId = 1;
            viewModel.RaiseBookingRequestUpdate(messageId, conversationId, true, false);

            Assert.True(invoked);
        }

        [Fact]
        public void ChatPageViewModel_raiseCashAgreementAccept_raisesCashAgreementAcceptEvent()
        {
            var viewModel = CreateViewModel();

            bool invoked = false;
            viewModel.CashAgreementAccept += (argument1, argument2) => invoked = true;
            int messageId = 1, conversationId = 1;
            viewModel.RaiseCashAgreementAccept(messageId, conversationId);

            Assert.True(invoked);
        }

        [Fact]
        public void ChatPageViewModel_raiseMessageSent_raisesMessageSentEvent()
        {
            var viewModel = CreateViewModel();

            bool invoked = false;
            viewModel.MessageSent += argument1 => invoked = true;

            viewModel.RaiseMessageSent(CreateMessage());

            Assert.True(invoked);
        }

        [Fact]
        public void ChatPageViewModel_loadConversation_setsMessageReadStatusBasedOnUnreadCount()
        {
            var viewModel = CreateViewModel();
            var conversation = CreateConversation();

            var messages = new List<MessageDTO>
            {
                CreateMessage(),
                CreateMessage()
            };

            viewModel.LoadConversation(conversation, messages, theirUnreadCount: 1);

            Assert.True(viewModel.Messages[0].IsRead);
            Assert.False(viewModel.Messages[1].IsRead);
        }

        [Fact]
        public void ChatPageViewModel_canSendWhitespaceInput_returnsFalse()
        {
            var viewModel = CreateViewModel();

            viewModel.InputText = "   ";

            Assert.False(viewModel.CanSend);
        }

        [Fact]
        public void ChatPageViewModel_proceedToPaymentValidMessageId_doesNotThrowException()
        {
            var viewModel = CreateViewModel();

            viewModel.ProceedToPayment(1);
        }

        [Fact]
        public void ChatPageViewModel_handleIncomingMessageDuplicateMessages_areNotAddedTwice()
        {
            var viewModel = CreateViewModel();
            viewModel.LoadConversation(CreateConversation(), new List<MessageDTO>(), theirUnreadCount: 0);

            var message1 = CreateMessage();
            var message2 = CreateMessage();
            message2 = message2 with { sentAt = message1.sentAt.AddMilliseconds(500) };

            viewModel.HandleIncomingMessage(message1);
            viewModel.HandleIncomingMessage(message2);

            Assert.Single(viewModel.Messages);
        }
    }
}