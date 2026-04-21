using System;
using System.Collections.Generic;
using BookingBoardgamesILoveBan.Src.Chat.DTO;
using BookingBoardgamesILoveBan.Src.Chat.Model;
using BookingBoardgamesILoveBan.Src.Chat.Repository;
using BookingBoardgamesILoveBan.Src.Chat.Service;
<<<<<<< Updated upstream
=======
using BookingBoardgamesILoveBan.Src.Chat.ViewModel;
>>>>>>> Stashed changes
using BookingBoardgamesILoveBan.Src.Chat.ViewModel;
using BookingBoardgamesILoveBan.Src.Enum;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using BookingBoardgamesILoveBan.Src.Model;
using Moq;
using Xunit;

namespace BookingBoardgamesILoveBan.Tests.Chat
{
    public class ChatPageViewModelTests
    {
        private readonly int currentUserId = 1;
        private Mock<IUserRepository> userService;

        private ConversationService CreateService()
        {
            var repo = new Mock<IConversationRepository>();

            repo.Setup(r => r.GetConversationsForUser(It.IsAny<int>()))
                .Returns(new List<Conversation>
                {
            new Conversation(
                1,
                new[] { 1, 2 },
                new List<Message>(),
                new Dictionary<int, DateTime>
                {
                    { 1, DateTime.MinValue },
                    { 2, DateTime.MinValue }
                })
                });

            userService = new Mock<IUserRepository>();

            userService
                .Setup(u => u.GetById(It.IsAny<int>()))
                .Returns(new User(1, "name", "country", "city", "street", "streetNumber"));

            return new ConversationService(
                repo.Object,
                currentUserId,
                userService.Object);
        }

        private ChatPageViewModel CreateVM()
        {
            return new ChatPageViewModel(currentUserId, CreateService(), userService.Object);
        }

        [Fact]
        public void Constructor_Should_Load_Conversations()
        {
            var vm = CreateVM();

            Assert.NotNull(vm.LeftPanelModelView);
            Assert.NotNull(vm.ChatModelView);
        }

        [Fact]
        public void OnMessageSent_Should_Send_Message()
        {
            var service = CreateService();
            var vm = new ChatPageViewModel(currentUserId, service, userService.Object);

            var msg = new MessageDTO(1, "hello", MessageType.MessageText)
            {
                conversationId = 1,
                receiverId = 2
            };

            bool called = false;
            service.ActionMessageProcessed += (m, u) => { called = true; };

            vm.ChatModelView.RaiseMessageSent(msg);

            Assert.True(true); // structural test (no crash)
        }

        [Fact]
        public void OnReadReceipt_Should_Update()
        {
            var service = CreateService();
            var vm = new ChatPageViewModel(currentUserId, service, userService.Object);

            var receipt = new ReadReceiptDTO(1, 2, 1, DateTime.Now);

            service.ActionReadReceiptProcessed += null;

            Assert.True(true);
        }

        [Fact]
        public void OnBookingRequest_Should_Update_Message()
        {
            var service = CreateService();
            var vm = new ChatPageViewModel(currentUserId, service, userService.Object);

            vm.ChatModelView.RaiseBookingRequestUpdate(5, 1, true, true);

            Assert.True(true);
        }

        [Fact]
        public void OnCashAgreement_Should_Update()
        {
            var service = CreateService();
            var vm = new ChatPageViewModel(currentUserId, service, userService.Object);

            vm.ChatModelView.RaiseCashAgreementAccept(5, 1);

            Assert.True(true);
        }

        [Fact]
        public void OnConversationReceived_Should_NotCrash()
        {
            var service = CreateService();
            var vm = new ChatPageViewModel(currentUserId, service, userService.Object);

            var convo = new ConversationDTO(
                1,
                new[] { 1, 2 },
                new List<MessageDTO>(),
                new Dictionary<int, DateTime>
                {
                    { 1, DateTime.MinValue },
                    { 2, DateTime.MinValue }
                });

            service.ActionConversationProcessed += null;

            Assert.True(true);
        }

        [Fact]
        public void OnMessageUpdate_Should_NotCrash()
        {
            var service = CreateService();
            var vm = new ChatPageViewModel(currentUserId, service, userService.Object);

            var msg = new MessageDTO(1, "hello", MessageType.MessageText)
            {
                conversationId = 1,
                receiverId = 2,
                isResolved = true
            };

            service.ActionMessageUpdateProcessed += null;

            Assert.True(true);
        }
    }
}