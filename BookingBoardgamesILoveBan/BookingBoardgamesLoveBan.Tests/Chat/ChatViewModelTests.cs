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
        public void LoadConversation_Should_SetHeaderAndMessages()
        {
            var viewModel = CreateViewModel();
            var conversation = CreateConversation();

            var messages = new List<MessageDTO>
            {
                CreateMessage(),
                CreateMessage()
            };

            viewModel.LoadConversation(conversation, messages, theirUnreadCount: 1);

            Assert.Equal(1, viewModel.ConversationId);
            Assert.Equal("John Doe", viewModel.DisplayName);
            Assert.Equal("JD", viewModel.Initials);
            Assert.Equal("avatar.png", viewModel.AvatarUrl);

            Assert.Equal(2, viewModel.Messages.Count);
        }

        [Fact]
        public void SendMessage_Should_AddMessage_And_ClearInput()
        {
            var viewModel = CreateViewModel();
            viewModel.LoadConversation(CreateConversation(), new List<MessageDTO>(), 0);

            bool invoked = false;
            viewModel.MessageSent += _ => invoked = true;

            viewModel.InputText = "hello world";
            viewModel.SendMessage();

            Assert.Single(viewModel.Messages);
            Assert.Equal(string.Empty, viewModel.InputText);
            Assert.True(invoked);
        }

        [Fact]
        public void SendMessage_Should_DoNothing_WhenInputEmpty()
        {
            var viewModel = CreateViewModel();
            viewModel.LoadConversation(CreateConversation(), new List<MessageDTO>(), 0);

            viewModel.InputText = "";

            viewModel.SendMessage();

            Assert.Empty(viewModel.Messages);
        }

        [Fact]
        public void HandleIncomingMessage_Should_AddMessage_WhenSameConversation()
        {
            var viewModel = CreateViewModel();
            viewModel.LoadConversation(CreateConversation(), new List<MessageDTO>(), 0);

            var message = CreateMessage(1);

            viewModel.HandleIncomingMessage(message);

            Assert.Single(viewModel.Messages);
        }

        [Fact]
        public void HandleIncomingMessage_Should_IgnoreDifferentConversation()
        {
            var viewModel = CreateViewModel();
            viewModel.LoadConversation(CreateConversation(), new List<MessageDTO>(), 0);

            var message = CreateMessage(99);

            viewModel.HandleIncomingMessage(message);

            Assert.Empty(viewModel.Messages);
        }

        [Fact]
        public void HandleIncomingMessage_Should_PreventDuplicates()
        {
            var viewModel = CreateViewModel();
            viewModel.LoadConversation(CreateConversation(), new List<MessageDTO>(), 0);

            var message = CreateMessage(1);

            viewModel.HandleIncomingMessage(message);
            viewModel.HandleIncomingMessage(message);

            Assert.Single(viewModel.Messages);
        }

        [Fact]
        public void ResolveBookingRequest_Should_InvokeEvent()
        {
            var viewModel = CreateViewModel();
            viewModel.LoadConversation(CreateConversation(), new List<MessageDTO> { CreateMessage() }, 0);

            bool invoked = false;
            viewModel.BookingRequestUpdate += (_, __, ___, ____) => invoked = true;

            viewModel.ResolveBookingRequest(1, true);

            Assert.True(invoked);
        }

        [Fact]
        public void ResolveBookingRequest_Should_NotThrow_WhenMessageMissing()
        {
            var viewModel = CreateViewModel();

            viewModel.ResolveBookingRequest(999, true);
        }

        [Fact]
        public void UpdateCashAgreement_Should_InvokeEvent()
        {
            var viewModel = CreateViewModel();
            viewModel.LoadConversation(CreateConversation(), new List<MessageDTO> { CreateMessage() }, 0);

            bool invoked = false;
            viewModel.CashAgreementAccept += (_, __) => invoked = true;

            viewModel.UpdateCashAgreement(1);

            Assert.True(invoked);
        }

        [Fact]
        public void UpdateCashAgreement_Should_NotThrow_WhenMissing()
        {
            var viewModel = CreateViewModel();

            viewModel.UpdateCashAgreement(999);
        }

        [Fact]
        public void SendImage_Should_InvokeEvent()
        {
            var viewModel = CreateViewModel();
            viewModel.LoadConversation(CreateConversation(), new List<MessageDTO>(), 0);

            bool invoked = false;
            viewModel.MessageSent += _ => invoked = true;

            viewModel.SendImage("file.png");

            Assert.True(invoked);
        }

        [Fact]
        public void RaiseBookingRequestUpdate_Should_FireEvent()
        {
            var viewModel = CreateViewModel();

            bool invoked = false;
            viewModel.BookingRequestUpdate += (_, __, ___, ____) => invoked = true;

            viewModel.RaiseBookingRequestUpdate(1, 1, true, false);

            Assert.True(invoked);
        }

        [Fact]
        public void RaiseCashAgreementAccept_Should_FireEvent()
        {
            var viewModel = CreateViewModel();

            bool invoked = false;
            viewModel.CashAgreementAccept += (_, __) => invoked = true;

            viewModel.RaiseCashAgreementAccept(1, 1);

            Assert.True(invoked);
        }

        [Fact]
        public void RaiseMessageSent_Should_FireEvent()
        {
            var viewModel = CreateViewModel();

            bool invoked = false;
            viewModel.MessageSent += _ => invoked = true;

            viewModel.RaiseMessageSent(CreateMessage());

            Assert.True(invoked);
        }

        [Fact]
        public void LoadConversation_Should_Set_Read_Status_Correctly()
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
        public void CanSend_Should_Be_False_For_Whitespace()
        {
            var viewModel = CreateViewModel();

            viewModel.InputText = "   ";

            Assert.False(viewModel.CanSend);
        }

        [Fact]
        public void ProceedToPayment_Should_NotThrow()
        {
            var viewModel = CreateViewModel();

            viewModel.ProceedToPayment(1);
        }

        [Fact]
        public void HandleIncomingMessage_Should_Prevent_Duplicate_By_Time()
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