using System;
using Xunit;
using BookingBoardgamesILoveBan.Src.PaymentCard.Service;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Repository;
using BookingBoardgamesILoveBan.Src.Mocks.RequestMock;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using BookingBoardgamesILoveBan.Src.Receipt.Service;
using BookingBoardgamesILoveBan.Src.Mocks.GameMock;

namespace BookingBoardgamesLoveBan.Tests.PaymentCard
{
    public class CardPaymentIntegrationTests
    {
        public CardPaymentIntegrationTests()
        {
            DatabaseBootstrap.Initialize();
        }

        [Fact]
        public void AddCardPayment_ValidPipeline_ReturnsNotNullResult()
        {
            PaymentRepository paymentRepository = new PaymentRepository();
            UserRepository userService = new UserRepository();
            GameRepository gameRepository = new GameRepository();
            RequestRepository requestRepository = new RequestRepository();
            RequestService requestService = new RequestService(requestRepository, gameRepository);
            ReceiptService receiptService = new ReceiptService(userService, requestService, gameRepository);
            CardPaymentService cardPaymentService = new CardPaymentService(paymentRepository, userService, receiptService, requestService);

            int clientIdentifier = 5;
            int ownerIdentifier = 2;
            int requestIdentifier = 5;
            decimal paymentPrice = 1.50m;

            decimal currentClientBalance = userService.GetUserBalance(clientIdentifier);
            decimal currentOwnerBalance = userService.GetUserBalance(ownerIdentifier);

            var resultDataTransferObject = cardPaymentService.AddCardPayment(requestIdentifier, clientIdentifier, ownerIdentifier, paymentPrice);

            Assert.NotNull(resultDataTransferObject);

            userService.UpdateBalance(clientIdentifier, currentClientBalance);
            userService.UpdateBalance(ownerIdentifier, currentOwnerBalance);
        }

        [Fact]
        public void AddCardPayment_ValidPipeline_ReturnsCardPaymentMethod()
        {
            PaymentRepository paymentRepository = new PaymentRepository();
            UserRepository userService = new UserRepository();
            GameRepository gameRepository = new GameRepository();
            RequestRepository requestRepository = new RequestRepository();
            RequestService requestService = new RequestService(requestRepository, gameRepository);
            ReceiptService receiptService = new ReceiptService(userService, requestService, gameRepository);
            CardPaymentService cardPaymentService = new CardPaymentService(paymentRepository, userService, receiptService, requestService);

            int clientIdentifier = 5;
            int ownerIdentifier = 2;
            int requestIdentifier = 5;
            decimal paymentPrice = 1.50m;
            string expectedPaymentMethod = "CARD";

            decimal currentClientBalance = userService.GetUserBalance(clientIdentifier);
            decimal currentOwnerBalance = userService.GetUserBalance(ownerIdentifier);

            var resultDataTransferObject = cardPaymentService.AddCardPayment(requestIdentifier, clientIdentifier, ownerIdentifier, paymentPrice);

            Assert.Equal(expectedPaymentMethod, resultDataTransferObject.PaymentMethod);

            userService.UpdateBalance(clientIdentifier, currentClientBalance);
            userService.UpdateBalance(ownerIdentifier, currentOwnerBalance);
        }

        [Fact]
        public void GetCardPayment_ValidTransaction_ReturnsNotNull()
        {
            PaymentRepository paymentRepository = new PaymentRepository();
            UserRepository userService = new UserRepository();
            GameRepository gameRepository = new GameRepository();
            RequestRepository requestRepository = new RequestRepository();
            RequestService requestService = new RequestService(requestRepository, gameRepository);
            ReceiptService receiptService = new ReceiptService(userService, requestService, gameRepository);
            CardPaymentService cardPaymentService = new CardPaymentService(paymentRepository, userService, receiptService, requestService);

            int clientIdentifier = 5;
            int ownerIdentifier = 2;
            int requestIdentifier = 5;
            decimal paymentPrice = 1.50m;

            decimal currentClientBalance = userService.GetUserBalance(clientIdentifier);
            decimal currentOwnerBalance = userService.GetUserBalance(ownerIdentifier);

            var resultDataTransferObject = cardPaymentService.AddCardPayment(requestIdentifier, clientIdentifier, ownerIdentifier, paymentPrice);
            var retrievedPayment = cardPaymentService.GetCardPayment(resultDataTransferObject.TransactionIdentifier);

            Assert.NotNull(retrievedPayment);

            userService.UpdateBalance(clientIdentifier, currentClientBalance);
            userService.UpdateBalance(ownerIdentifier, currentOwnerBalance);
        }

        [Fact]
        public void GetCardPayment_ValidTransaction_ReturnsCorrectAmount()
        {
            PaymentRepository paymentRepository = new PaymentRepository();
            UserRepository userService = new UserRepository();
            GameRepository gameRepository = new GameRepository();
            RequestRepository requestRepository = new RequestRepository();
            RequestService requestService = new RequestService(requestRepository, gameRepository);
            ReceiptService receiptService = new ReceiptService(userService, requestService, gameRepository);
            CardPaymentService cardPaymentService = new CardPaymentService(paymentRepository, userService, receiptService, requestService);

            int clientIdentifier = 5;
            int ownerIdentifier = 2;
            int requestIdentifier = 5;
            decimal paymentPrice = 1.50m;

            decimal currentClientBalance = userService.GetUserBalance(clientIdentifier);
            decimal currentOwnerBalance = userService.GetUserBalance(ownerIdentifier);

            var resultDataTransferObject = cardPaymentService.AddCardPayment(requestIdentifier, clientIdentifier, ownerIdentifier, paymentPrice);
            var retrievedPayment = cardPaymentService.GetCardPayment(resultDataTransferObject.TransactionIdentifier);

            Assert.Equal(paymentPrice, retrievedPayment.Amount);

            userService.UpdateBalance(clientIdentifier, currentClientBalance);
            userService.UpdateBalance(ownerIdentifier, currentOwnerBalance);
        }

        [Fact]
        public void GetCardPayment_ValidTransaction_ReturnsCorrectClientIdentifier()
        {
            PaymentRepository paymentRepository = new PaymentRepository();
            UserRepository userService = new UserRepository();
            GameRepository gameRepository = new GameRepository();
            RequestRepository requestRepository = new RequestRepository();
            RequestService requestService = new RequestService(requestRepository, gameRepository);
            ReceiptService receiptService = new ReceiptService(userService, requestService, gameRepository);
            CardPaymentService cardPaymentService = new CardPaymentService(paymentRepository, userService, receiptService, requestService);

            int clientIdentifier = 5;
            int ownerIdentifier = 2;
            int requestIdentifier = 5;
            decimal paymentPrice = 1.50m;

            decimal currentClientBalance = userService.GetUserBalance(clientIdentifier);
            decimal currentOwnerBalance = userService.GetUserBalance(ownerIdentifier);

            var resultDataTransferObject = cardPaymentService.AddCardPayment(requestIdentifier, clientIdentifier, ownerIdentifier, paymentPrice);
            var retrievedPayment = cardPaymentService.GetCardPayment(resultDataTransferObject.TransactionIdentifier);

            Assert.Equal(clientIdentifier, retrievedPayment.ClientIdentifier);

            userService.UpdateBalance(clientIdentifier, currentClientBalance);
            userService.UpdateBalance(ownerIdentifier, currentOwnerBalance);
        }

        [Fact]
        public void AddCardPayment_InsufficientFunds_ThrowsException()
        {
            PaymentRepository paymentRepository = new PaymentRepository();
            UserRepository userService = new UserRepository();
            GameRepository gameRepository = new GameRepository();
            RequestRepository requestRepository = new RequestRepository();
            RequestService requestService = new RequestService(requestRepository, gameRepository);
            ReceiptService receiptService = new ReceiptService(userService, requestService, gameRepository);
            CardPaymentService cardPaymentService = new CardPaymentService(paymentRepository, userService, receiptService, requestService);

            int lowBalanceClientIdentifier = 8;
            int ownerIdentifier = 2;
            int requestIdentifier = 5;
            decimal paymentPrice = 1.50m;

            Assert.Throws<Exception>(() => cardPaymentService.AddCardPayment(requestIdentifier, lowBalanceClientIdentifier, ownerIdentifier, paymentPrice));
        }
    }
}
