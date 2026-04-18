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
        private readonly Mock<UserRepository> mockUserService;
        private readonly Mock<ReceiptService> mockReceiptService;
        private readonly Mock<RequestService> mockRequestService;
        private readonly CardPaymentService cardPaymentService;

        public CardPaymentServiceTests()
        {
            mockPaymentRepository = new Mock<PaymentRepository>();
            mockUserService = new Mock<UserRepository>();

            Mock<BookingBoardgamesILoveBan.Src.Mocks.GameMock.GameRepository> mockGameRepository = new Mock<BookingBoardgamesILoveBan.Src.Mocks.GameMock.GameRepository>();
            mockRequestService = new Mock<RequestService>(mockGameRepository.Object);

            mockReceiptService = new Mock<ReceiptService>(
                mockUserService.Object,
                mockRequestService.Object,
                mockGameRepository.Object);

            cardPaymentService = new CardPaymentService(
                mockPaymentRepository.Object,
                mockUserService.Object,
                mockReceiptService.Object,
                mockRequestService.Object);
        }

        [Fact]
        public void CheckBalanceSufficiency_ClientHasMoreThanRequestPrice_ReturnsTrue()
        {
            int requestIdentifier = 1;
            int clientIdentifier = 2;
            decimal requestPrice = 50.0m;
            decimal clientBalance = 100.0m;

            mockRequestService.Setup(requestServiceMock => requestServiceMock.GetRequestPrice(requestIdentifier)).Returns(requestPrice);
            mockUserService.Setup(userServiceMock => userServiceMock.GetUserBalance(clientIdentifier)).Returns(clientBalance);

            bool isBalanceSufficient = cardPaymentService.CheckBalanceSufficiency(requestIdentifier, clientIdentifier);

            Assert.True(isBalanceSufficient);
        }

        [Fact]
        public void CheckBalanceSufficiency_ClientHasLessThanRequestPrice_ReturnsFalse()
        {
            int requestIdentifier = 1;
            int clientIdentifier = 2;
            decimal requestPrice = 100.0m;
            decimal clientBalance = 50.0m;

            mockRequestService.Setup(requestServiceMock => requestServiceMock.GetRequestPrice(requestIdentifier)).Returns(requestPrice);
            mockUserService.Setup(userServiceMock => userServiceMock.GetUserBalance(clientIdentifier)).Returns(clientBalance);

            bool isBalanceSufficient = cardPaymentService.CheckBalanceSufficiency(requestIdentifier, clientIdentifier);

            Assert.False(isBalanceSufficient);
        }

        [Fact]
        public void AddCardPayment_InsufficientFunds_ThrowsExceptionMessage()
        {
            int requestIdentifier = 1;
            int clientIdentifier = 2;
            int ownerIdentifier = 3;
            decimal paymentAmount = 100.0m;
            decimal requestPrice = 100.0m;
            decimal clientBalance = 50.0m;
            string expectedExceptionMessage = "Insufficient Funds";

            mockRequestService.Setup(requestServiceMock => requestServiceMock.GetRequestPrice(requestIdentifier)).Returns(requestPrice);
            mockUserService.Setup(userServiceMock => userServiceMock.GetUserBalance(clientIdentifier)).Returns(clientBalance);

            Exception thrownException = Assert.Throws<Exception>(() =>
                cardPaymentService.AddCardPayment(requestIdentifier, clientIdentifier, ownerIdentifier, paymentAmount));

            Assert.Equal(expectedExceptionMessage, thrownException.Message);
        }

        [Fact]
        public void AddCardPayment_InsufficientFunds_DoesNotCallRepository()
        {
            int requestIdentifier = 1;
            int clientIdentifier = 2;
            int ownerIdentifier = 3;
            decimal paymentAmount = 100.0m;
            decimal requestPrice = 100.0m;
            decimal clientBalance = 50.0m;

            mockRequestService.Setup(requestServiceMock => requestServiceMock.GetRequestPrice(requestIdentifier)).Returns(requestPrice);
            mockUserService.Setup(userServiceMock => userServiceMock.GetUserBalance(clientIdentifier)).Returns(clientBalance);

            try
            {
                cardPaymentService.AddCardPayment(requestIdentifier, clientIdentifier, ownerIdentifier, paymentAmount);
            }
            catch (Exception)
            {
            }

            mockPaymentRepository.Verify(paymentRepositoryMock => paymentRepositoryMock.AddPayment(It.IsAny<Payment>()), Times.Never);
        }

        [Fact]
        public void AddCardPayment_SufficientFunds_ReturnsNotNull()
        {
            int requestIdentifier = 1;
            int clientIdentifier = 2;
            int ownerIdentifier = 3;
            decimal paymentAmount = 100.0m;
            int expectedTransactionIdentifier = 999;
            string expectedReceiptPath = "/receipts/1.pdf";
            decimal requestPrice = 100.0m;
            decimal clientBalance = 150.0m;
            decimal ownerBalance = 500.0m;

            mockRequestService.Setup(requestServiceMock => requestServiceMock.GetRequestPrice(requestIdentifier)).Returns(requestPrice);
            mockUserService.Setup(userServiceMock => userServiceMock.GetUserBalance(clientIdentifier)).Returns(clientBalance);
            mockUserService.Setup(userServiceMock => userServiceMock.GetUserBalance(ownerIdentifier)).Returns(ownerBalance);
            mockPaymentRepository.Setup(paymentRepositoryMock => paymentRepositoryMock.AddPayment(It.IsAny<Payment>())).Returns(expectedTransactionIdentifier);
            mockReceiptService.Setup(receiptServiceMock => receiptServiceMock.GenerateReceiptRelativePath(requestIdentifier)).Returns(expectedReceiptPath);

            var resultDataTransferObject = cardPaymentService.AddCardPayment(requestIdentifier, clientIdentifier, ownerIdentifier, paymentAmount);

            Assert.NotNull(resultDataTransferObject);
        }

        [Fact]
        public void AddCardPayment_SufficientFunds_ReturnsCorrectTransactionIdentifier()
        {
            int requestIdentifier = 1;
            int clientIdentifier = 2;
            int ownerIdentifier = 3;
            decimal paymentAmount = 100.0m;
            int expectedTransactionIdentifier = 999;
            string expectedReceiptPath = "/receipts/1.pdf";
            decimal requestPrice = 100.0m;
            decimal clientBalance = 150.0m;
            decimal ownerBalance = 500.0m;

            mockRequestService.Setup(requestServiceMock => requestServiceMock.GetRequestPrice(requestIdentifier)).Returns(requestPrice);
            mockUserService.Setup(userServiceMock => userServiceMock.GetUserBalance(clientIdentifier)).Returns(clientBalance);
            mockUserService.Setup(userServiceMock => userServiceMock.GetUserBalance(ownerIdentifier)).Returns(ownerBalance);
            mockPaymentRepository.Setup(paymentRepositoryMock => paymentRepositoryMock.AddPayment(It.IsAny<Payment>())).Returns(expectedTransactionIdentifier);
            mockReceiptService.Setup(receiptServiceMock => receiptServiceMock.GenerateReceiptRelativePath(requestIdentifier)).Returns(expectedReceiptPath);

            var resultDataTransferObject = cardPaymentService.AddCardPayment(requestIdentifier, clientIdentifier, ownerIdentifier, paymentAmount);

            Assert.Equal(expectedTransactionIdentifier, resultDataTransferObject.TransactionIdentifier);
        }

        [Fact]
        public void AddCardPayment_SufficientFunds_ReturnsCorrectPaymentMethod()
        {
            int requestIdentifier = 1;
            int clientIdentifier = 2;
            int ownerIdentifier = 3;
            decimal paymentAmount = 100.0m;
            int expectedTransactionIdentifier = 999;
            string expectedReceiptPath = "/receipts/1.pdf";
            decimal requestPrice = 100.0m;
            decimal clientBalance = 150.0m;
            decimal ownerBalance = 500.0m;
            string expectedPaymentMethod = "CARD";

            mockRequestService.Setup(requestServiceMock => requestServiceMock.GetRequestPrice(requestIdentifier)).Returns(requestPrice);
            mockUserService.Setup(userServiceMock => userServiceMock.GetUserBalance(clientIdentifier)).Returns(clientBalance);
            mockUserService.Setup(userServiceMock => userServiceMock.GetUserBalance(ownerIdentifier)).Returns(ownerBalance);
            mockPaymentRepository.Setup(paymentRepositoryMock => paymentRepositoryMock.AddPayment(It.IsAny<Payment>())).Returns(expectedTransactionIdentifier);
            mockReceiptService.Setup(receiptServiceMock => receiptServiceMock.GenerateReceiptRelativePath(requestIdentifier)).Returns(expectedReceiptPath);

            var resultDataTransferObject = cardPaymentService.AddCardPayment(requestIdentifier, clientIdentifier, ownerIdentifier, paymentAmount);

            Assert.Equal(expectedPaymentMethod, resultDataTransferObject.PaymentMethod);
        }

        [Fact]
        public void AddCardPayment_SufficientFunds_UpdatesClientBalance()
        {
            int requestIdentifier = 1;
            int clientIdentifier = 2;
            int ownerIdentifier = 3;
            decimal paymentAmount = 100.0m;
            int expectedTransactionIdentifier = 999;
            string expectedReceiptPath = "/receipts/1.pdf";
            decimal requestPrice = 100.0m;
            decimal clientBalance = 150.0m;
            decimal ownerBalance = 500.0m;
            decimal expectedNewClientBalance = 50.0m;

            mockRequestService.Setup(requestServiceMock => requestServiceMock.GetRequestPrice(requestIdentifier)).Returns(requestPrice);
            mockUserService.Setup(userServiceMock => userServiceMock.GetUserBalance(clientIdentifier)).Returns(clientBalance);
            mockUserService.Setup(userServiceMock => userServiceMock.GetUserBalance(ownerIdentifier)).Returns(ownerBalance);
            mockPaymentRepository.Setup(paymentRepositoryMock => paymentRepositoryMock.AddPayment(It.IsAny<Payment>())).Returns(expectedTransactionIdentifier);
            mockReceiptService.Setup(receiptServiceMock => receiptServiceMock.GenerateReceiptRelativePath(requestIdentifier)).Returns(expectedReceiptPath);

            cardPaymentService.AddCardPayment(requestIdentifier, clientIdentifier, ownerIdentifier, paymentAmount);

            mockUserService.Verify(userServiceMock => userServiceMock.UpdateBalance(clientIdentifier, expectedNewClientBalance), Times.Once);
        }

        [Fact]
        public void AddCardPayment_SufficientFunds_UpdatesOwnerBalance()
        {
            int requestIdentifier = 1;
            int clientIdentifier = 2;
            int ownerIdentifier = 3;
            decimal paymentAmount = 100.0m;
            int expectedTransactionIdentifier = 999;
            string expectedReceiptPath = "/receipts/1.pdf";
            decimal requestPrice = 100.0m;
            decimal clientBalance = 150.0m;
            decimal ownerBalance = 500.0m;
            decimal expectedNewOwnerBalance = 600.0m;

            mockRequestService.Setup(requestServiceMock => requestServiceMock.GetRequestPrice(requestIdentifier)).Returns(requestPrice);
            mockUserService.Setup(userServiceMock => userServiceMock.GetUserBalance(clientIdentifier)).Returns(clientBalance);
            mockUserService.Setup(userServiceMock => userServiceMock.GetUserBalance(ownerIdentifier)).Returns(ownerBalance);
            mockPaymentRepository.Setup(paymentRepositoryMock => paymentRepositoryMock.AddPayment(It.IsAny<Payment>())).Returns(expectedTransactionIdentifier);
            mockReceiptService.Setup(receiptServiceMock => receiptServiceMock.GenerateReceiptRelativePath(requestIdentifier)).Returns(expectedReceiptPath);

            cardPaymentService.AddCardPayment(requestIdentifier, clientIdentifier, ownerIdentifier, paymentAmount);

            mockUserService.Verify(userServiceMock => userServiceMock.UpdateBalance(ownerIdentifier, expectedNewOwnerBalance), Times.Once);
        }

        [Fact]
        public void AddCardPayment_SufficientFunds_UpdatesPaymentInRepository()
        {
            int requestIdentifier = 1;
            int clientIdentifier = 2;
            int ownerIdentifier = 3;
            decimal paymentAmount = 100.0m;
            int expectedTransactionIdentifier = 999;
            string expectedReceiptPath = "/receipts/1.pdf";
            decimal requestPrice = 100.0m;
            decimal clientBalance = 150.0m;
            decimal ownerBalance = 500.0m;

            mockRequestService.Setup(requestServiceMock => requestServiceMock.GetRequestPrice(requestIdentifier)).Returns(requestPrice);
            mockUserService.Setup(userServiceMock => userServiceMock.GetUserBalance(clientIdentifier)).Returns(clientBalance);
            mockUserService.Setup(userServiceMock => userServiceMock.GetUserBalance(ownerIdentifier)).Returns(ownerBalance);
            mockPaymentRepository.Setup(paymentRepositoryMock => paymentRepositoryMock.AddPayment(It.IsAny<Payment>())).Returns(expectedTransactionIdentifier);
            mockReceiptService.Setup(receiptServiceMock => receiptServiceMock.GenerateReceiptRelativePath(requestIdentifier)).Returns(expectedReceiptPath);

            cardPaymentService.AddCardPayment(requestIdentifier, clientIdentifier, ownerIdentifier, paymentAmount);

            mockPaymentRepository.Verify(paymentRepositoryMock => paymentRepositoryMock.UpdatePayment(It.IsAny<Payment>()), Times.Once);
        }

        [Fact]
        public void GetRequestDataTransferObject_FetchesAndReturnsDataTransferObject()
        {
            int requestIdentifier = 1;

            var resultDataTransferObject = cardPaymentService.GetRequestDataTransferObject(requestIdentifier);

            Assert.NotNull(resultDataTransferObject);
        }

        [Fact]
        public void GetCardPayment_FetchesAndReturnsNotNull()
        {
            int expectedTransactionIdentifier = 999;
            decimal paymentAmount = 50.0m;
            string paymentMethod = "CARD";
            int requestIdentifier = 1;
            int clientIdentifier = 2;
            int ownerIdentifier = 3;

            Payment fakePayment = new Payment
            {
                Tid = expectedTransactionIdentifier,
                Amount = paymentAmount,
                PaymentMethod = paymentMethod,
                RequestId = requestIdentifier,
                ClientId = clientIdentifier,
                OwnerId = ownerIdentifier
            };

            mockPaymentRepository.Setup(paymentRepositoryMock => paymentRepositoryMock.GetById(expectedTransactionIdentifier)).Returns(fakePayment);

            var fetchedPayment = cardPaymentService.GetCardPayment(expectedTransactionIdentifier);

            Assert.NotNull(fetchedPayment);
        }

        [Fact]
        public void GetCardPayment_FetchesAndReturnsCorrectTransactionIdentifier()
        {
            int expectedTransactionIdentifier = 999;
            decimal paymentAmount = 50.0m;
            string paymentMethod = "CARD";
            int requestIdentifier = 1;
            int clientIdentifier = 2;
            int ownerIdentifier = 3;

            Payment fakePayment = new Payment
            {
                Tid = expectedTransactionIdentifier,
                Amount = paymentAmount,
                PaymentMethod = paymentMethod,
                RequestId = requestIdentifier,
                ClientId = clientIdentifier,
                OwnerId = ownerIdentifier
            };

            mockPaymentRepository.Setup(paymentRepositoryMock => paymentRepositoryMock.GetById(expectedTransactionIdentifier)).Returns(fakePayment);

            var fetchedPayment = cardPaymentService.GetCardPayment(expectedTransactionIdentifier);

            Assert.Equal(expectedTransactionIdentifier, fetchedPayment.TransactionIdentifier);
        }

        [Fact]
        public void GetCurrentBalance_FetchesAndReturnsBalance()
        {
            int clientIdentifier = 2;
            decimal expectedClientBalance = 250.0m;
            mockUserService.Setup(userServiceMock => userServiceMock.GetUserBalance(clientIdentifier)).Returns(expectedClientBalance);

            decimal retrievedBalance = cardPaymentService.GetCurrentBalance(clientIdentifier);

            Assert.Equal(expectedClientBalance, retrievedBalance);
        }

        [Fact]
        public void ProcessPayment_InsufficientFunds_ThrowsException()
        {
            int requestIdentifier = 1;
            int clientIdentifier = 2;
            int ownerIdentifier = 3;
            decimal requestPrice = 100.0m;
            decimal clientBalance = 50.0m;
            decimal ownerBalance = 500.0m;
            string expectedExceptionMessage = "Insufficient Funds";

            mockRequestService.Setup(requestServiceMock => requestServiceMock.GetRequestPrice(requestIdentifier)).Returns(requestPrice);
            mockUserService.Setup(userServiceMock => userServiceMock.GetUserBalance(clientIdentifier)).Returns(clientBalance);
            mockUserService.Setup(userServiceMock => userServiceMock.GetUserBalance(ownerIdentifier)).Returns(ownerBalance);

            Exception thrownException = Assert.Throws<Exception>(() =>
                cardPaymentService.ProcessPayment(requestIdentifier, clientIdentifier, ownerIdentifier));

            Assert.Equal(expectedExceptionMessage, thrownException.Message);
        }

        [Fact]
        public void ConvertToDataTransferObject_NullTransactionDate_ReturnsNotNull()
        {
            int transactionIdentifier = 1;
            int requestIdentifier = 1;
            int clientIdentifier = 1;
            int ownerIdentifier = 1;
            decimal paymentAmount = 10m;
            string paymentMethod = "CARD";

            Payment fakePayment = new Payment
            {
                Tid = transactionIdentifier,
                RequestId = requestIdentifier,
                ClientId = clientIdentifier,
                OwnerId = ownerIdentifier,
                Amount = paymentAmount,
                PaymentMethod = paymentMethod,
                DateOfTransaction = null
            };

            var convertedDataTransferObject = cardPaymentService.ConvertToDataTransferObject(fakePayment);

            Assert.NotNull(convertedDataTransferObject);
        }

        [Fact]
        public void ConvertToDataTransferObject_NullTransactionDate_UsesCurrentDate()
        {
            int transactionIdentifier = 1;
            int requestIdentifier = 1;
            int clientIdentifier = 1;
            int ownerIdentifier = 1;
            decimal paymentAmount = 10m;
            string paymentMethod = "CARD";

            Payment fakePayment = new Payment
            {
                Tid = transactionIdentifier,
                RequestId = requestIdentifier,
                ClientId = clientIdentifier,
                OwnerId = ownerIdentifier,
                Amount = paymentAmount,
                PaymentMethod = paymentMethod,
                DateOfTransaction = null
            };

            var convertedDataTransferObject = cardPaymentService.ConvertToDataTransferObject(fakePayment);

            Assert.Equal(DateTime.Now.Date, convertedDataTransferObject.DateOfTransaction.Date);
        }
    }
}
