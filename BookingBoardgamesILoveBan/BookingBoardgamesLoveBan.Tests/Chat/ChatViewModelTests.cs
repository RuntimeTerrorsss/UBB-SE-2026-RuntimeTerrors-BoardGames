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
        public void LoadConversation_SetHeaderAndMessages()
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
        public void SendMessage_AddMessageAndClearInput()
        {
            var viewModel = CreateViewModel();
            viewModel.LoadConversation(CreateConversation(), new List<MessageDTO>(), 0);

            bool invoked = false;
            viewModel.MessageSent += _ => invoked = true;

            viewModel.InputText = "hello world";
            viewModel.SendMessage();

            Assert.True(invoked);
        }

        [Fact]
        public void SendMessage_InputEmpty_DoNothing()
        {
            var viewModel = CreateViewModel();
            viewModel.LoadConversation(CreateConversation(), new List<MessageDTO>(), 0);

            viewModel.InputText = "";

            viewModel.SendMessage();

            Assert.Empty(viewModel.Messages);
        }

        [Fact]
        public void HandleIncomingMessage_WhenSameConversation_AddMessage()
        {
            var viewModel = CreateViewModel();
            viewModel.LoadConversation(CreateConversation(), new List<MessageDTO>(), 0);

            var message = CreateMessage(1);

            viewModel.HandleIncomingMessage(message);

            Assert.Single(viewModel.Messages);
        }

        [Fact]
        public void HandleIncomingMessage_WhenDifferentConversation_Ignore()
        {
            var viewModel = CreateViewModel();
            viewModel.LoadConversation(CreateConversation(), new List<MessageDTO>(), 0);

            var message = CreateMessage(99);

            viewModel.HandleIncomingMessage(message);

            Assert.Empty(viewModel.Messages);
        }

        [Fact]
        public void HandleIncomingMessage_DuplicateMessage_DoNotInsertDuplicate()
        {
            var viewModel = CreateViewModel();
            viewModel.LoadConversation(CreateConversation(), new List<MessageDTO>(), 0);

            var message = CreateMessage(1);

            viewModel.HandleIncomingMessage(message);
            viewModel.HandleIncomingMessage(message);

            Assert.Single(viewModel.Messages);
        }

        [Fact]
        public void ResolveBookingRequest_InvokeEvent()
        {
            var viewModel = CreateViewModel();
            viewModel.LoadConversation(CreateConversation(), new List<MessageDTO> { CreateMessage() }, 0);

            bool invoked = false;
            viewModel.BookingRequestUpdate += (_, __, ___, ____) => invoked = true;

            viewModel.ResolveBookingRequest(1, true);

            Assert.True(invoked);
        }

        [Fact]
        public void ResolveBookingRequest_WhenMessageMissing_NotThrow()
        {
            var viewModel = CreateViewModel();
            viewModel.ResolveBookingRequest(999, true);
        }

        [Fact]
        public void UpdateCashAgreement_InvokeEvent()
        {
            var viewModel = CreateViewModel();
            viewModel.LoadConversation(CreateConversation(), new List<MessageDTO> { CreateMessage() }, 0);

            bool invoked = false;
            viewModel.CashAgreementAccept += (_, __) => invoked = true;

            viewModel.UpdateCashAgreement(1);

            Assert.True(invoked);
        }

        [Fact]
        public void UpdateCashAgreement_WhenMissing_NotThrow()
        {
            var viewModel = CreateViewModel();

            viewModel.UpdateCashAgreement(999);
        }

        [Fact]
        public void SendImage_InvokeEvent()
        {
            var viewModel = CreateViewModel();
            viewModel.LoadConversation(CreateConversation(), new List<MessageDTO>(), 0);

            bool invoked = false;
            viewModel.MessageSent += _ => invoked = true;

            viewModel.SendImage("file.png");

            Assert.True(invoked);
        }

        [Fact]
        public void RaiseBookingRequestUpdate_InvokeEvent()
        {
            var viewModel = CreateViewModel();

            bool invoked = false;
            viewModel.BookingRequestUpdate += (_, __, ___, ____) => invoked = true;

            viewModel.RaiseBookingRequestUpdate(1, 1, true, false);

            Assert.True(invoked);
        }

        [Fact]
        public void RaiseCashAgreementAccept_InvokeEvent()
        {
            var viewModel = CreateViewModel();

            bool invoked = false;
            viewModel.CashAgreementAccept += (_, __) => invoked = true;

            viewModel.RaiseCashAgreementAccept(1, 1);

            Assert.True(invoked);
        }

        [Fact]
        public void RaiseMessageSent_InvokeEvent()
        {
            var viewModel = CreateViewModel();

            bool invoked = false;
            viewModel.MessageSent += _ => invoked = true;

            viewModel.RaiseMessageSent(CreateMessage());

            Assert.True(invoked);
        }

        [Fact]
        public void LoadConversation_SetReadStatusCorrectly()
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
        public void CanSend_InputIsWhiteSpace_IsFalse()
        {
            var viewModel = CreateViewModel();

            viewModel.InputText = "   ";

            Assert.False(viewModel.CanSend);
        }

        [Fact]
        public void ProceedToPayment_NotThrow()
        {
            var viewModel = CreateViewModel();

            viewModel.ProceedToPayment(1);
        }

        [Fact]
        public void HandleIncomingMessage_DuplicateMessage_NotInsertDuplicate()
        {
            var viewModel = CreateViewModel();
            viewModel.LoadConversation(CreateConversation(), new List<MessageDTO>(), 0);

            var message1 = CreateMessage();
            var message2 = CreateMessage();
            message2 = message2 with { sentAt = message1.sentAt.AddMilliseconds(500) };

            viewModel.HandleIncomingMessage(message1);
            viewModel.HandleIncomingMessage(message2);

            Assert.Single(viewModel.Messages);
        }
    }
}