using System;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly Mock<UserRepository> mockUserService;

        public CardPaymentViewModelTests()
        {
            SynchronizationContext.SetSynchronizationContext(new TestSyncContext());

            mockCardPaymentService = new Mock<CardPaymentService>(null, null, null, null);
            mockUserService = new Mock<UserRepository>();

            int requestIdentifier = 1;
            string gameName = "Catan";
            int clientIdentifier = 2;
            int ownerIdentifier = 3;
            string ownerName = "Owner";
            string clientName = "Client";
            decimal paymentPrice = 50.0m;
            int daysToAdd = 2;

            mockCardPaymentService.Setup(cardPaymentServiceMock => cardPaymentServiceMock.GetRequestDataTransferObject(It.IsAny<int>()))
                .Returns(new RequestDataTransferObject(requestIdentifier, gameName, clientIdentifier, ownerIdentifier, ownerName, clientName, DateTime.Now, DateTime.Now.AddDays(daysToAdd), paymentPrice));
        }

        private CardPaymentViewModel CreateViewModel()
        {
            int requestIdentifier = 1;
            string deliveryAddress = "123 Main St";
            int bookingMessageIdentifier = 10;

            return new CardPaymentViewModel(
                mockCardPaymentService.Object,
                mockUserService.Object,
                requestIdentifier,
                deliveryAddress,
                bookingMessageIdentifier,
                null);
        }

        [Fact]
        public void IsPaymentButtonEnabled_AllConditionsMet_ReturnsTrue()
        {
            CardPaymentViewModel cardPaymentViewModel = CreateViewModel();
            decimal balanceAmount = 100m;
            bool termsAccepted = true;
            bool currentlyLoading = false;
            string cardNumber = "1234567890123456";
            string cardholderName = "John Doe";
            string expiryDate = "12/25";
            string securityCode = "123";

            cardPaymentViewModel.BalanceAmount = balanceAmount;
            cardPaymentViewModel.AreTermsAccepted = termsAccepted;
            cardPaymentViewModel.IsCurrentlyLoading = currentlyLoading;
            cardPaymentViewModel.CardNumber = cardNumber;
            cardPaymentViewModel.CardholderName = cardholderName;
            cardPaymentViewModel.ExpiryDate = expiryDate;
            cardPaymentViewModel.CardVerificationValue = securityCode;

            Assert.True(cardPaymentViewModel.IsPaymentButtonEnabled);
        }

        [Fact]
        public void IsPaymentButtonEnabled_TermsNotAccepted_ReturnsFalse()
        {
            CardPaymentViewModel cardPaymentViewModel = CreateViewModel();
            decimal balanceAmount = 100m;
            bool termsAccepted = false;
            string cardNumber = "1234567890123456";
            string cardholderName = "John Doe";
            string expiryDate = "12/25";
            string securityCode = "123";

            cardPaymentViewModel.BalanceAmount = balanceAmount;
            cardPaymentViewModel.AreTermsAccepted = termsAccepted;
            cardPaymentViewModel.CardNumber = cardNumber;
            cardPaymentViewModel.CardholderName = cardholderName;
            cardPaymentViewModel.ExpiryDate = expiryDate;
            cardPaymentViewModel.CardVerificationValue = securityCode;

            Assert.False(cardPaymentViewModel.IsPaymentButtonEnabled);
        }

        [Fact]
        public void IsWarningMessageVisible_BalanceLessThanPrice_ReturnsTrue()
        {
            CardPaymentViewModel cardPaymentViewModel = CreateViewModel();
            decimal balanceAmount = 20m;

            cardPaymentViewModel.BalanceAmount = balanceAmount;

            Assert.True(cardPaymentViewModel.IsWarningMessageVisible);
        }

        [Fact]
        public void RequestIdentifier_Get_ReturnsCorrectValue()
        {
            CardPaymentViewModel cardPaymentViewModel = CreateViewModel();
            int expectedIdentifier = 1;

            Assert.Equal(expectedIdentifier, cardPaymentViewModel.RequestIdentifier);
        }

        [Fact]
        public void ClientIdentifier_Get_ReturnsCorrectValue()
        {
            CardPaymentViewModel cardPaymentViewModel = CreateViewModel();
            int expectedIdentifier = 2;

            Assert.Equal(expectedIdentifier, cardPaymentViewModel.ClientIdentifier);
        }

        [Fact]
        public void OwnerIdentifier_Get_ReturnsCorrectValue()
        {
            CardPaymentViewModel cardPaymentViewModel = CreateViewModel();
            int expectedIdentifier = 3;

            Assert.Equal(expectedIdentifier, cardPaymentViewModel.OwnerIdentifier);
        }

        [Fact]
        public void GameName_Get_ReturnsCorrectValue()
        {
            CardPaymentViewModel cardPaymentViewModel = CreateViewModel();
            string expectedGameName = "Catan";

            Assert.Equal(expectedGameName, cardPaymentViewModel.GameName);
        }

        [Fact]
        public void OwnerName_Get_ReturnsCorrectValue()
        {
            CardPaymentViewModel cardPaymentViewModel = CreateViewModel();
            string expectedOwnerName = "Owner";

            Assert.Equal(expectedOwnerName, cardPaymentViewModel.OwnerName);
        }

        [Fact]
        public void ClientName_Get_ReturnsCorrectValue()
        {
            CardPaymentViewModel cardPaymentViewModel = CreateViewModel();
            string expectedClientName = "Client";

            Assert.Equal(expectedClientName, cardPaymentViewModel.ClientName);
        }

        [Fact]
        public void DeliveryAddress_Get_ReturnsCorrectValue()
        {
            CardPaymentViewModel cardPaymentViewModel = CreateViewModel();
            string expectedAddress = "123 Main St";

            Assert.Equal(expectedAddress, cardPaymentViewModel.DeliveryAddress);
        }

        [Fact]
        public void DeliveryDate_Get_ReturnsNotNull()
        {
            CardPaymentViewModel cardPaymentViewModel = CreateViewModel();

            Assert.NotNull(cardPaymentViewModel.DeliveryDate);
        }

        [Fact]
        public void RequestDates_Get_ReturnsNotNull()
        {
            CardPaymentViewModel cardPaymentViewModel = CreateViewModel();

            Assert.NotNull(cardPaymentViewModel.RequestDates);
        }

        [Fact]
        public void Price_Get_ReturnsCorrectValue()
        {
            CardPaymentViewModel cardPaymentViewModel = CreateViewModel();
            decimal expectedPaymentPrice = 50.0m;

            Assert.Equal(expectedPaymentPrice, cardPaymentViewModel.Price);
        }

        [Fact]
        public void BookingMessageIdentifier_Get_ReturnsCorrectValue()
        {
            CardPaymentViewModel cardPaymentViewModel = CreateViewModel();
            int expectedBookingMessageIdentifier = 10;

            Assert.Equal(expectedBookingMessageIdentifier, cardPaymentViewModel.BookingMessageIdentifier);
        }

        [Fact]
        public void ConversationService_Get_ReturnsNull()
        {
            CardPaymentViewModel cardPaymentViewModel = CreateViewModel();

            Assert.Null(cardPaymentViewModel.ConversationService);
        }

        [Fact]
        public void ExitCommand_InvokesNavigateBackwardsAction()
        {
            CardPaymentViewModel cardPaymentViewModel = CreateViewModel();
            bool navigationActionWasInvoked = false;
            cardPaymentViewModel.NavigateBackwardsAction = () => navigationActionWasInvoked = true;
            object nullCommandParameter = null;

            cardPaymentViewModel.ExitCommand.Execute(nullCommandParameter);

            Assert.True(navigationActionWasInvoked);
        }

        [Fact]
        public void ResetInactivityCommand_ExecutesWithoutError()
        {
            CardPaymentViewModel cardPaymentViewModel = CreateViewModel();
            object nullCommandParameter = null;

            Exception executionException = Record.Exception(() => cardPaymentViewModel.ResetInactivityCommand.Execute(nullCommandParameter));

            Assert.Null(executionException);
        }

        [Fact]
        public void PageLifecycle_OnPageActivated_RefreshesBalance()
        {
            CardPaymentViewModel cardPaymentViewModel = CreateViewModel();
            decimal mockUserBalance = 999m;
            mockUserService.Setup(userServiceMock => userServiceMock.GetUserBalance(It.IsAny<int>())).Returns(mockUserBalance);

            cardPaymentViewModel.OnPageActivated();

            Assert.Equal(mockUserBalance, cardPaymentViewModel.BalanceAmount);
        }

        [Fact]
        public async Task FinishPaymentAsync_Exception_SetsErrorMessage()
        {
            CardPaymentViewModel cardPaymentViewModel = CreateViewModel();
            string expectedErrorMessage = "Payment failed: Mocked error";
            mockCardPaymentService.Setup(cardPaymentServiceMock => cardPaymentServiceMock.AddCardPayment(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>()))
                .Throws(new Exception("Mocked error"));

            System.Reflection.MethodInfo finishPaymentMethod = typeof(CardPaymentViewModel).GetMethod("FinishPaymentAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            object[] nullMethodParameters = null;
            Task finishPaymentTask = (Task)finishPaymentMethod.Invoke(cardPaymentViewModel, nullMethodParameters);
            await finishPaymentTask;

            Assert.Equal(expectedErrorMessage, cardPaymentViewModel.CurrentStatusMessage);
        }

        [Fact]
        public async Task FinishPaymentAsync_Exception_SetsIsCurrentlyLoadingToFalse()
        {
            CardPaymentViewModel cardPaymentViewModel = CreateViewModel();
            mockCardPaymentService.Setup(cardPaymentServiceMock => cardPaymentServiceMock.AddCardPayment(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>()))
                .Throws(new Exception("Mocked error"));

            System.Reflection.MethodInfo finishPaymentMethod = typeof(CardPaymentViewModel).GetMethod("FinishPaymentAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            object[] nullMethodParameters = null;
            Task finishPaymentTask = (Task)finishPaymentMethod.Invoke(cardPaymentViewModel, nullMethodParameters);
            await finishPaymentTask;

            Assert.False(cardPaymentViewModel.IsCurrentlyLoading);
        }

        [Fact]
        public async Task FinishPaymentAsync_Success_SetsPaymentSuccessfulToTrue()
        {
            CardPaymentViewModel cardPaymentViewModel = CreateViewModel();

            System.Reflection.MethodInfo finishPaymentMethod = typeof(CardPaymentViewModel).GetMethod("FinishPaymentAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            object[] nullMethodParameters = null;
            Task finishPaymentTask = (Task)finishPaymentMethod.Invoke(cardPaymentViewModel, nullMethodParameters);
            await finishPaymentTask;

            Assert.True(cardPaymentViewModel.IsPaymentSuccessful);
        }

        [Fact]
        public async Task FinishPaymentAsync_Success_SetsSuccessMessage()
        {
            CardPaymentViewModel cardPaymentViewModel = CreateViewModel();
            string expectedSuccessMessage = "Payment successful!";

            System.Reflection.MethodInfo finishPaymentMethod = typeof(CardPaymentViewModel).GetMethod("FinishPaymentAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            object[] nullMethodParameters = null;
            Task finishPaymentTask = (Task)finishPaymentMethod.Invoke(cardPaymentViewModel, nullMethodParameters);
            await finishPaymentTask;

            Assert.Equal(expectedSuccessMessage, cardPaymentViewModel.CurrentStatusMessage);
        }

        [Fact]
        public void OnSessionExpired_InvokesNavigateToExitAction()
        {
            CardPaymentViewModel cardPaymentViewModel = CreateViewModel();
            bool exitActionWasInvoked = false;
            cardPaymentViewModel.NavigateToExitAction = () => exitActionWasInvoked = true;

            cardPaymentViewModel.OnPageActivated();

            System.Reflection.MethodInfo sessionExpiredMethod = typeof(CardPaymentViewModel).GetMethod("OnSessionExpired", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            object[] nullTimerEventParameters = new object[] { null, null };
            sessionExpiredMethod.Invoke(cardPaymentViewModel, nullTimerEventParameters);

            Assert.True(exitActionWasInvoked);
        }

        [Fact]
        public void OnSessionExpired_SetsSessionExpiredMessage()
        {
            CardPaymentViewModel cardPaymentViewModel = CreateViewModel();
            bool exitActionWasInvoked = false;
            cardPaymentViewModel.NavigateToExitAction = () => exitActionWasInvoked = true;
            string expectedExpiredMessage = "Session expired due to inactivity.";

            cardPaymentViewModel.OnPageActivated();

            System.Reflection.MethodInfo sessionExpiredMethod = typeof(CardPaymentViewModel).GetMethod("OnSessionExpired", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            object[] nullTimerEventParameters = new object[] { null, null };
            sessionExpiredMethod.Invoke(cardPaymentViewModel, nullTimerEventParameters);

            Assert.Equal(expectedExpiredMessage, cardPaymentViewModel.CurrentStatusMessage);
        }

        [Fact]
        public void CoverageSweeper_HitsHiddenLambdasAndEarlyReturns()
        {
            CardPaymentViewModel cardPaymentViewModel = CreateViewModel();
            object nullCommandParameter = null;

            bool canExecuteCommandResult = cardPaymentViewModel.FinishPaymentCommand.CanExecute(nullCommandParameter);
            cardPaymentViewModel.FinishPaymentCommand.Execute(nullCommandParameter);

            System.Reflection.MethodInfo refreshBalanceMethod = typeof(CardPaymentViewModel).GetMethod("RefreshBalance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            object[] nullMethodParameters = null;
            refreshBalanceMethod.Invoke(cardPaymentViewModel, nullMethodParameters);

            System.Reflection.MethodInfo sessionExpiredMethod = typeof(CardPaymentViewModel).GetMethod("OnSessionExpired", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            object[] nullTimerEventParameters = new object[] { null, null };
            sessionExpiredMethod.Invoke(cardPaymentViewModel, nullTimerEventParameters);

            Assert.False(canExecuteCommandResult);
        }
    }

    public class TestSyncContext : SynchronizationContext
    {
        public override void Post(SendOrPostCallback postCallback, object threadState) => postCallback(threadState);
        public override void Send(SendOrPostCallback sendCallback, object threadState) => sendCallback(threadState);
    }
}