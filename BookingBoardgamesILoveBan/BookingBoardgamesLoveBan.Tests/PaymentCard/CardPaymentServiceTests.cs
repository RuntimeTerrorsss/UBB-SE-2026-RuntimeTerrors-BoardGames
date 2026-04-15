using System;
using Xunit;
using Moq;
using BookingBoardgamesILoveBan.Src.PaymentCard.Service;
using BookingBoardgamesILoveBan.Src.Mocks.RequestMock;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Repository;
using BookingBoardgamesILoveBan.Src.Receipt.Service;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;

namespace BookingBoardgamesLoveBan.Tests.PaymentCard
{
    public class CardPaymentServiceTests
    {
        private readonly Mock<PaymentRepository> mockPaymentRepository;
        private readonly Mock<UserService> mockUserService;
        private readonly Mock<ReceiptService> mockReceiptService;
        private readonly Mock<RequestService> mockRequestService;
        private readonly CardPaymentService cardPaymentService;

        public CardPaymentServiceTests()
        {
            DatabaseBootstrap.Initialize();
            mockPaymentRepository = new Mock<PaymentRepository>();
            mockUserService = new Mock<UserService>();

            var mockGameService = new Mock<BookingBoardgamesILoveBan.Src.Mocks.GameMock.GameService>();
            mockRequestService = new Mock<RequestService>(mockGameService.Object);

            mockReceiptService = new Mock<ReceiptService>(
                mockUserService.Object,
                mockRequestService.Object,
                mockGameService.Object);

            cardPaymentService = new CardPaymentService(
                mockPaymentRepository.Object,
                mockUserService.Object,
                mockReceiptService.Object,
                mockRequestService.Object);
        }

        [Fact]
        public void CheckBalanceSufficiency_ClientHasMoreThanRequestPrice_ReturnsTrue()
        {
            int requestId = 1;
            int clientId = 2;
            mockRequestService.Setup(rs => rs.GetRequestPrice(requestId)).Returns(50.0m);
            mockUserService.Setup(us => us.GetUserBalance(clientId)).Returns(100.0m);

            bool result = cardPaymentService.CheckBalanceSufficiency(requestId, clientId);
            Assert.True(result);
        }

        [Fact]
        public void CheckBalanceSufficiency_ClientHasLessThanRequestPrice_ReturnsFalse()
        {
            int requestId = 1;
            int clientId = 2;
            mockRequestService.Setup(rs => rs.GetRequestPrice(requestId)).Returns(100.0m);
            mockUserService.Setup(us => us.GetUserBalance(clientId)).Returns(50.0m);

            bool result = cardPaymentService.CheckBalanceSufficiency(requestId, clientId);

            Assert.False(result);
        }

        [Fact]
        public void AddCardPayment_InsufficientFunds_ThrowsException()
        {
            int requestId = 1;
            int clientId = 2;
            int ownerId = 3;
            decimal amount = 100.0m;

            mockRequestService.Setup(rs => rs.GetRequestPrice(requestId)).Returns(100.0m);
            mockUserService.Setup(us => us.GetUserBalance(clientId)).Returns(50.0m);

            var exception = Assert.Throws<Exception>(() =>
                cardPaymentService.AddCardPayment(requestId, clientId, ownerId, amount));

            Assert.Equal("Insufficient Funds", exception.Message);
            mockPaymentRepository.Verify(repo => repo.AddPayment(It.IsAny<Payment>()), Times.Never);
        }

        [Fact]
        public void AddCardPayment_SufficientFunds_ProcessesPaymentAndReturnsDataTransferObject()
        {
            int requestId = 1;
            int clientId = 2;
            int ownerId = 3;
            decimal amount = 100.0m;
            int expectedTransactionId = 999;
            string expectedReceiptPath = "/receipts/1.pdf";

            mockRequestService.Setup(rs => rs.GetRequestPrice(requestId)).Returns(100.0m);
            mockUserService.Setup(us => us.GetUserBalance(clientId)).Returns(150.0m);
            mockUserService.Setup(us => us.GetUserBalance(ownerId)).Returns(500.0m);

            mockPaymentRepository.Setup(repo => repo.AddPayment(It.IsAny<Payment>())).Returns(expectedTransactionId);
            mockReceiptService.Setup(rs => rs.GenerateReceiptRelativePath(requestId)).Returns(expectedReceiptPath);

            var result = cardPaymentService.AddCardPayment(requestId, clientId, ownerId, amount);

            Assert.NotNull(result);
            Assert.Equal(expectedTransactionId, result.TransactionIdentifier);
            Assert.Equal("CARD", result.PaymentMethod);

            mockUserService.Verify(us => us.UpdateBalance(clientId, 50.0m), Times.Once);
            mockUserService.Verify(us => us.UpdateBalance(ownerId, 600.0m), Times.Once);
            mockPaymentRepository.Verify(repo => repo.UpdatePayment(It.IsAny<Payment>()), Times.Once);
        }
    }
}