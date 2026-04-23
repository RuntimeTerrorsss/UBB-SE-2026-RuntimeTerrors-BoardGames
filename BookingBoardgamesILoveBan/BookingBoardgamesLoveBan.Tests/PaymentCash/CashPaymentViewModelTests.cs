using BookingBoardgamesILoveBan.Src.Chat.Repository;
using BookingBoardgamesILoveBan.Src.Chat.Service;
using BookingBoardgamesILoveBan.Src.Mocks.GameMock;
using BookingBoardgamesILoveBan.Src.Mocks.RequestMock;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using BookingBoardgamesILoveBan.Src.PaymentCash.Model;
using BookingBoardgamesILoveBan.Src.PaymentCash.Service;
using BookingBoardgamesILoveBan.Src.PaymentCash.ViewModel;
using Moq;

namespace BookingBoardgamesLoveBan.Tests.PaymentCash
{
    public class CashPaymentViewModelTests
    {
        [Fact]
        public void Constructor_SetsSummaryFromRequestGameAndUsers()
        {
            var requestId = 100;
            var messageId = 77;
            var delivery = "22B";
            var start = new DateTime(2026, 5, 1);
            var end = new DateTime(2026, 5, 5);
            var request = new Request(requestId, gameId: 200, clientId: 301, ownerId: 302, start, end);
            var game = new Game(200, "Azul", 12m);
            var client = new User(301, "renter", "R", "Ro", "Clooj", "Low", "22B", string.Empty, 0m);
            var owner = new User(302, "lender", "L", "Ro", "Valcea", "High", "1", string.Empty, 0m);

            var requestService = new Mock<IRequestService>();
            requestService.Setup(requestServiceDependency => requestServiceDependency.GetRequestById(requestId)).Returns(request);
            requestService.Setup(requestServiceDependency => requestServiceDependency.GetRequestPrice(requestId)).Returns(88.5m);
            var gameRepository = new Mock<IGameRepository>();
            gameRepository.Setup(gameRepositoryDependency => gameRepositoryDependency.GetById(200)).Returns(game);
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(userRepositoryDependency => userRepositoryDependency.GetById(301)).Returns(client);
            userRepository.Setup(userRepositoryDependency => userRepositoryDependency.GetById(302)).Returns(owner);
            var cashPaymentService = new Mock<ICashPaymentService>();
            cashPaymentService.Setup(cashPaymentServiceDependency => cashPaymentServiceDependency.AddCashPayment(It.IsAny<CashPaymentDataTransferObject>())).Returns(999);
            var conversationRepository = new Mock<IConversationRepository>();
            conversationRepository.Setup(conversationRepositoryService => conversationRepositoryService.Subscribe(It.IsAny<int>(), It.IsAny<IConversationService>()));
            var conversationUserRepository = new Mock<IUserRepository>();
            var conversationService = new ConversationService(
                conversationRepository.Object,
                userIdInput: 1,
                conversationUserRepository.Object);

            var viewModel = new CashPaymentViewModel(
                cashPaymentService.Object,
                userRepository.Object,
                requestService.Object,
                gameRepository.Object,
                requestId,
                delivery,
                messageId,
                conversationService);

            var expected = new
            {
                OwnerName = "lender",
                GameName = "Azul",
                DeliveryAddress = delivery,
            };
            var actual = new
            {
                viewModel.OwnerName,
                viewModel.GameName,
                viewModel.DeliveryAddress,
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Constructor_CreatesCashPaymentUsingRequestParticipantsAndQuotedPrice()
        {
            var requestId = 50;
            var messageId = 12;
            var request = new Request(requestId, 1, clientId: 10, ownerId: 20, DateTime.Now, DateTime.Now.AddDays(1));
            var requestService = new Mock<IRequestService>();
            requestService.Setup(requestServiceDependency => requestServiceDependency.GetRequestById(requestId)).Returns(request);
            requestService.Setup(requestServiceDependency => requestServiceDependency.GetRequestPrice(requestId)).Returns(40m);
            var gameRepository = new Mock<IGameRepository>();
            gameRepository.Setup(gameRepositoryDependency => gameRepositoryDependency.GetById(1)).Returns(new Game(1, "G", 1m));
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(userRepositoryDependency => userRepositoryDependency.GetById(10)).Returns(new User(10, "a", "b", "c", "d", "e", "f", string.Empty, 0m));
            userRepository.Setup(userRepositoryDependency => userRepositoryDependency.GetById(20)).Returns(new User(20, "b", "b", "c", "d", "e", "f", string.Empty, 0m));
            var cashPaymentService = new Mock<ICashPaymentService>();
            CashPaymentDataTransferObject? addedCashPaymentDataTransferObject = null;
            cashPaymentService.Setup(cashPaymentServiceDependency => cashPaymentServiceDependency.AddCashPayment(It.IsAny<CashPaymentDataTransferObject>()))
                .Callback<CashPaymentDataTransferObject>(cashPaymentDataTransferObject => addedCashPaymentDataTransferObject = cashPaymentDataTransferObject)
                .Returns(1);
            var conversationRepository = new Mock<IConversationRepository>();
            conversationRepository.Setup(conversationRepositoryService => conversationRepositoryService.Subscribe(It.IsAny<int>(), It.IsAny<IConversationService>()));
            var conversationUserRepository = new Mock<IUserRepository>();
            var conversationService = new ConversationService(
                conversationRepository.Object,
                userIdInput: 1,
                conversationUserRepository.Object);

            var viewModel = new CashPaymentViewModel(
                cashPaymentService.Object,
                userRepository.Object,
                requestService.Object,
                gameRepository.Object,
                requestId,
                "address",
                messageId,
                conversationService);

            Assert.NotNull(addedCashPaymentDataTransferObject);
            var expected = new
            {
                Id = -1,
                RequestId = requestId,
                ClientId = 10,
                OwnerId = 20,
                PaidAmount = 40m,
            };
            var actual = new
            {
                addedCashPaymentDataTransferObject!.Id,
                addedCashPaymentDataTransferObject.RequestId,
                addedCashPaymentDataTransferObject.ClientId,
                addedCashPaymentDataTransferObject.OwnerId,
                addedCashPaymentDataTransferObject.PaidAmount,
            };

            Assert.Equal(expected, actual);
            Assert.NotNull(viewModel);
        }

        [Fact]
        public void Constructor_FinalizesRentalRequestAndCreatesCashAgreementMessage()
        {
            var requestId = 60;
            var messageId = 33;
            var returnedPaymentId = 555;
            var request = new Request(requestId, 1, 40, 50, DateTime.Now, DateTime.Now.AddDays(1));
            var requestService = new Mock<IRequestService>();
            requestService.Setup(requestServiceDependency => requestServiceDependency.GetRequestById(requestId)).Returns(request);
            requestService.Setup(requestServiceDependency => requestServiceDependency.GetRequestPrice(requestId)).Returns(1m);
            var gameRepository = new Mock<IGameRepository>();
            gameRepository.Setup(gameRepositoryDependency => gameRepositoryDependency.GetById(1)).Returns(new Game(1, "G", 1m));
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(userRepositoryDependency => userRepositoryDependency.GetById(40)).Returns(new User(40, "a", "b", "c", "d", "e", "f", string.Empty, 0m));
            userRepository.Setup(userRepositoryDependency => userRepositoryDependency.GetById(50)).Returns(new User(50, "b", "b", "c", "d", "e", "f", string.Empty, 0m));
            var cashPaymentService = new Mock<ICashPaymentService>();
            cashPaymentService.Setup(cashPaymentServiceDependency => cashPaymentServiceDependency.AddCashPayment(It.IsAny<CashPaymentDataTransferObject>())).Returns(returnedPaymentId);
            var conversationRepository = new Mock<IConversationRepository>();
            conversationRepository.Setup(conversationRepositoryService => conversationRepositoryService.Subscribe(It.IsAny<int>(), It.IsAny<IConversationService>()));
            var conversationUserRepository = new Mock<IUserRepository>();
            var conversationService = new ConversationService(
                conversationRepository.Object,
                userIdInput: 1,
                conversationUserRepository.Object);

            var viewModel = new CashPaymentViewModel(
                cashPaymentService.Object,
                userRepository.Object,
                requestService.Object,
                gameRepository.Object,
                requestId,
                "address",
                messageId,
                conversationService);

            conversationRepository.Verify(conversationRepositoryService => conversationRepositoryService.HandleRentalRequestFinalization(messageId), Times.Once);
            conversationRepository.Verify(conversationRepositoryService => conversationRepositoryService.CreateCashAgreementMessage(messageId, returnedPaymentId), Times.Once);
            Assert.NotNull(viewModel);
        }
    }
}
