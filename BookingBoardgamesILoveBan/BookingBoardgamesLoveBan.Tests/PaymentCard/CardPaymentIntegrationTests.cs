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
        public void AddCardPayment_ValidPipeline_SuccessfullySavesToDatabase()
        {
            var repository = new PaymentRepository();
            var userService = new UserService();

            var gameService = new GameService();
            var requestService = new RequestService(gameService);
            var receiptService = new ReceiptService(userService, requestService, gameService);

            var paymentService = new CardPaymentService(repository, userService, receiptService, requestService);

            int clientId = 1;
            int ownerId = 2;
            int requestId = 1;
            decimal price = 100m;

            var result = paymentService.AddCardPayment(requestId, clientId, ownerId, price);
            var retrievedPayment = paymentService.GetCardPayment(result.TransactionIdentifier);

            Assert.NotNull(result);
            Assert.Equal("CARD", result.PaymentMethod);

            Assert.NotNull(retrievedPayment);
            Assert.Equal(price, retrievedPayment.Amount);
            Assert.Equal(clientId, retrievedPayment.ClientIdentifier);

        }
    }
}