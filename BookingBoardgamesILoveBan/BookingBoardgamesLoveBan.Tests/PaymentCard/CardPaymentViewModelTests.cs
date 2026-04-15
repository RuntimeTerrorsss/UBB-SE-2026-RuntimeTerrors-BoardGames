using System;
using Xunit;
using Moq;
using BookingBoardgamesILoveBan.Src.PaymentCard.ViewModel;
using BookingBoardgamesILoveBan.Src.PaymentCard.Service;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using BookingBoardgamesILoveBan.Src.Mocks.RequestMock;

namespace BookingBoardgamesILoveBan.Tests.PaymentCard.ViewModel
{
    public class CardPaymentViewModelTests
    {
        private readonly Mock<CardPaymentService> mockCardPaymentService;
        private readonly Mock<UserService> mockUserService;

        public CardPaymentViewModelTests()
        {
            mockCardPaymentService = new Mock<CardPaymentService>(null, null, null, null);
            mockUserService = new Mock<UserService>();
            mockCardPaymentService.Setup(s => s.GetRequestDataTransferObject(It.IsAny<int>()))
                .Returns(new RequestDto(1, "Catan", 2, 3, "Owner", "Client", DateTime.Now, DateTime.Now.AddDays(2), 50.0m));
        }

        private CardPaymentViewModel CreateViewModel()
        {
            return new CardPaymentViewModel(
                mockCardPaymentService.Object,
                mockUserService.Object,
                requestIdentifier: 1,
                deliveryAddress: "123 Main St",
                bookingMessageIdentifier: 10,
                conversationService: null);
        }

        [Fact]
        public void IsPaymentButtonEnabled_AllConditionsMet_ReturnsTrue()
        {
            var viewModel = CreateViewModel();
            viewModel.BalanceAmount = 100m;
            viewModel.AreTermsAccepted = true;
            viewModel.IsCurrentlyLoading = false;

            viewModel.CardNumber = "1234567890123456";
            viewModel.CardholderName = "John Doe";
            viewModel.ExpiryDate = "12/25";
            viewModel.Cvv = "123";

            bool canExecute = viewModel.IsPaymentButtonEnabled;

            Assert.True(canExecute);
        }

        [Fact]
        public void IsPaymentButtonEnabled_TermsNotAccepted_ReturnsFalse()
        {
            var viewModel = CreateViewModel();
            viewModel.BalanceAmount = 100m;
            viewModel.AreTermsAccepted = false;
            viewModel.CardNumber = "1234567890123456";
            viewModel.CardholderName = "John Doe";
            viewModel.ExpiryDate = "12/25";
            viewModel.Cvv = "123";

            bool canExecute = viewModel.IsPaymentButtonEnabled;

            Assert.False(canExecute);
        }

        [Fact]
        public void IsWarningMessageVisible_BalanceLessThanPrice_ReturnsTrue()
        {
            var viewModel = CreateViewModel();
            viewModel.BalanceAmount = 20m; 

            bool isWarningVisible = viewModel.IsWarningMessageVisible;

            Assert.True(isWarningVisible);
        }
    }
}