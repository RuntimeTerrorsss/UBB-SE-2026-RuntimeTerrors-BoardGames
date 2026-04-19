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
        private ChatViewModel CreateVM()
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
                type: MessageType.Text,
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
            var vm = CreateVM();
            var convo = CreateConversation();

            var messages = new List<MessageDTO>
            {
                CreateMessage(),
                CreateMessage()
            };

            vm.LoadConversation(convo, messages, theirUnreadCount: 1);

            Assert.Equal(1, vm.ConversationId);
            Assert.Equal("John Doe", vm.DisplayName);
            Assert.Equal("JD", vm.Initials);
            Assert.Equal("avatar.png", vm.AvatarUrl);

            Assert.Equal(2, vm.Messages.Count);
        }

        [Fact]
        public void SendMessage_Should_AddMessage_And_ClearInput()
        {
            var vm = CreateVM();
            vm.LoadConversation(CreateConversation(), new List<MessageDTO>(), 0);

            bool invoked = false;
            vm.MessageSent += _ => invoked = true;

            vm.InputText = "hello world";
            vm.SendMessage();

            Assert.Single(vm.Messages);
            Assert.Equal(string.Empty, vm.InputText);
            Assert.True(invoked);
        }

        [Fact]
        public void SendMessage_Should_DoNothing_WhenInputEmpty()
        {
            var vm = CreateVM();
            vm.LoadConversation(CreateConversation(), new List<MessageDTO>(), 0);

            vm.InputText = string.Empty;

            vm.SendMessage();

            Assert.Empty(vm.Messages);
        }

        [Fact]
        public void HandleIncomingMessage_Should_AddMessage_WhenSameConversation()
        {
            var vm = CreateVM();
            vm.LoadConversation(CreateConversation(), new List<MessageDTO>(), 0);

            var msg = CreateMessage(1);

            vm.HandleIncomingMessage(msg);

            Assert.Single(vm.Messages);
        }

        [Fact]
        public void HandleIncomingMessage_Should_IgnoreDifferentConversation()
        {
            var vm = CreateVM();
            vm.LoadConversation(CreateConversation(), new List<MessageDTO>(), 0);

            var msg = CreateMessage(99);

            vm.HandleIncomingMessage(msg);

            Assert.Empty(vm.Messages);
        }

        [Fact]
        public void HandleIncomingMessage_Should_PreventDuplicates()
        {
            var vm = CreateVM();
            vm.LoadConversation(CreateConversation(), new List<MessageDTO>(), 0);

            var msg = CreateMessage(1);

            vm.HandleIncomingMessage(msg);
            vm.HandleIncomingMessage(msg);

            Assert.Single(vm.Messages);
        }

        [Fact]
        public void ResolveBookingRequest_Should_InvokeEvent()
        {
            var vm = CreateVM();
            vm.LoadConversation(CreateConversation(), new List<MessageDTO> { CreateMessage() }, 0);

            bool invoked = false;
            vm.BookingRequestUpdate += (sender, msgId, accepted, resolved) => invoked = true;

            vm.ResolveBookingRequest(1, true);

            Assert.True(invoked);
        }

        [Fact]
        public void ResolveBookingRequest_Should_NotThrow_WhenMessageMissing()
        {
            var vm = CreateVM();

            vm.ResolveBookingRequest(999, true);
        }

        [Fact]
        public void UpdateCashAgreement_Should_InvokeEvent()
        {
            var vm = CreateVM();
            vm.LoadConversation(CreateConversation(), new List<MessageDTO> { CreateMessage() }, 0);

            bool invoked = false;
            vm.CashAgreementAccept += (sender, msgId) => invoked = true;

            vm.UpdateCashAgreement(1);

            Assert.True(invoked);
        }

        [Fact]
        public void UpdateCashAgreement_Should_NotThrow_WhenMissing()
        {
            var vm = CreateVM();

            vm.UpdateCashAgreement(999);
        }

        [Fact]
        public void SendImage_Should_InvokeEvent()
        {
            var vm = CreateVM();
            vm.LoadConversation(CreateConversation(), new List<MessageDTO>(), 0);

            bool invoked = false;
            vm.MessageSent += _ => invoked = true;

            vm.SendImage("file.png");

            Assert.True(invoked);
        }

        [Fact]
        public void RaiseBookingRequestUpdate_Should_FireEvent()
        {
            var vm = CreateVM();

            bool invoked = false;
            vm.BookingRequestUpdate += (sender, msgId, accepted, resolved) => invoked = true;

            vm.RaiseBookingRequestUpdate(1, 1, true, false);

            Assert.True(invoked);
        }

        [Fact]
        public void RaiseCashAgreementAccept_Should_FireEvent()
        {
            var vm = CreateVM();

            bool invoked = false;
            vm.CashAgreementAccept += (sender, msgId) => invoked = true;

            vm.RaiseCashAgreementAccept(1, 1);

            Assert.True(invoked);
        }

        [Fact]
        public void RaiseMessageSent_Should_FireEvent()
        {
            var vm = CreateVM();

            bool invoked = false;
            vm.MessageSent += _ => invoked = true;

            vm.RaiseMessageSent(CreateMessage());

            Assert.True(invoked);
        }
    }
}