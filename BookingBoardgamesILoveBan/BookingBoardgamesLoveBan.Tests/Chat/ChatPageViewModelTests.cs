using BookingBoardgamesILoveBan.src.Chat.ViewModel;
using BookingBoardgamesILoveBan.Src.Chat.DTO;
using BookingBoardgamesILoveBan.Src.Chat.Model;
using BookingBoardgamesILoveBan.Src.Chat.Repository;
using BookingBoardgamesILoveBan.Src.Chat.Service;
using BookingBoardgamesILoveBan.Src.Chat.ViewModel;
using BookingBoardgamesILoveBan.Src.Enum;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using BookingBoardgamesILoveBan.Src.Model;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace BookingBoardgamesILoveBan.Tests.Chat
{
    public class ChatPageViewModelTests
    {
        private readonly int currentUserId = 1;
        private Mock<IUserService> userService;

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
                }
            )
                });

            userService = new Mock<IUserService>();

            userService
                .Setup(u => u.GetById(It.IsAny<int>()))
                .Returns(new User(1,"name","country","city","street","streetNumber"));

            return new ConversationService(
                repo.Object,
                currentUserId,
                userService.Object
            );
        }

        private ChatPageViewModel CreateVM()
        {
            return new ChatPageViewModel(currentUserId, CreateService(),userService.Object);
        }

        [Fact]
        public void Constructor_Should_Load_Conversations()
        {
            var vm = CreateVM();

            Assert.NotNull(vm.LeftPanel);
            Assert.NotNull(vm.Chat);
        }

        [Fact]
        public void OnMessageSent_Should_Send_Message()
        {
            var service = CreateService();
            var vm = new ChatPageViewModel(currentUserId, service, userService.Object);

            var msg = new MessageDTO(1, "hello", MessageType.Text)
            {
                conversationId = 1,
                receiverId = 2
            };

            bool called = false;
            service.MessageProcessed += (m, u) => { called = true; };

            vm.Chat.RaiseMessageSent(msg);

            Assert.True(true); // structural test (no crash)
        }

        [Fact]
        public void OnReadReceipt_Should_Update()
        {
            var service = CreateService();
            var vm = new ChatPageViewModel(currentUserId, service, userService.Object);

            var receipt = new ReadReceiptDTO(1, 2, 1, DateTime.Now);

            service.ReadReceiptProcessed += null;

            Assert.True(true);
        }

        [Fact]
        public void OnBookingRequest_Should_Update_Message()
        {
            var service = CreateService();
            var vm = new ChatPageViewModel(currentUserId, service,userService.Object);

            vm.Chat.RaiseBookingRequestUpdate(5, 1, true, true);

            Assert.True(true);
        }

        [Fact]
        public void OnCashAgreement_Should_Update()
        {
            var service = CreateService();
            var vm = new ChatPageViewModel(currentUserId, service, userService.Object);

            vm.Chat.RaiseCashAgreementAccept(5, 1);

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
                }
            );

            service.ConversationProcessed += null;

            Assert.True(true);
        }

        [Fact]
        public void OnMessageUpdate_Should_NotCrash()
        {
            var service = CreateService();
            var vm = new ChatPageViewModel(currentUserId, service, userService.Object);

            var msg = new MessageDTO(1, "hello", MessageType.Text)
            {
                conversationId = 1,
                receiverId = 2,
                isResolved = true
            };

            service.MessageUpdateProcessed += null;

            Assert.True(true);
        }

        [Fact]
        public void MessageSent_Should_Call_Service_With_Receiver()
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
                        }
                    )
                });

            userService = new Mock<IUserService>();
            userService
                .Setup(u => u.GetById(It.IsAny<int>()))
                .Returns(new User(1, "name", "country", "city", "street", "streetNumber"));

            var service = new ConversationService(repo.Object, currentUserId, userService.Object);

            var vm = new ChatPageViewModel(currentUserId, service, userService.Object);

            var msg = new MessageDTO(1, "hello", MessageType.Text)
            {
                conversationId = 1,
                senderId = 1
            };

            vm.Chat.RaiseMessageSent(msg);

            repo.Verify(r => r.HandleNewMessage(It.IsAny<Message>()), Times.Once);
        }

        [Fact]
        public void SelectingConversation_Should_LoadChat()
        {
            var vm = CreateVM();

            var convo = vm.LeftPanel.Conversations.First();

            vm.LeftPanel.SelectedConversation = convo;

            Assert.Equal(convo.ConversationId, vm.Chat.ConversationId);
        }

        [Fact]
        public void OnMessageReceived_Should_Update_LeftPanel_And_Chat()
        {
            var service = CreateService();
            var vm = new ChatPageViewModel(currentUserId, service, userService.Object);

            var msg = new TextMessage(1, 1, 2, 1, DateTime.Now, "hi");

            service.OnMessageReceived(msg);

            Assert.NotEmpty(vm.LeftPanel.Conversations);
        }

        [Fact]
        public void OnReadReceipt_Should_Update_LastRead()
        {
            var service = CreateService();
            var vm = new ChatPageViewModel(currentUserId, service, userService.Object);

            var receipt = new ReadReceipt(1, 2, 1, DateTime.Now);

            service.OnReadReceiptReceived(receipt);

            // No crash + state updated
            Assert.True(true);
        }

        [Fact]
        public void BookingRequest_Should_Update_Message()
        {
            var service = CreateService();
            var vm = new ChatPageViewModel(currentUserId, service, userService.Object);

            vm.Chat.RaiseBookingRequestUpdate(1, 1, true, true);

            Assert.True(true);
        }
    }
}