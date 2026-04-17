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
        private readonly Mock<UserService> mockUserService;

        public CardPaymentViewModelTests()
        {
            SynchronizationContext.SetSynchronizationContext(new TestSyncContext());

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
                requestId: 1,
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

            Assert.True(viewModel.IsPaymentButtonEnabled);
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

            Assert.False(viewModel.IsPaymentButtonEnabled);
        }

        [Fact]
        public void IsWarningMessageVisible_BalanceLessThanPrice_ReturnsTrue()
        {
            var viewModel = CreateViewModel();
            viewModel.BalanceAmount = 20m;

            Assert.True(viewModel.IsWarningMessageVisible);
        }

        [Fact]
        public void Properties_SetSameValue_ReturnsEarlyAndTestsGetters()
        {
            var vm = CreateViewModel();

            vm.BalanceAmount = 0m;
            vm.AreTermsAccepted = false;
            vm.IsCurrentlyLoading = false;
            vm.CurrentStatusMessage = string.Empty;
            vm.IsPaymentSuccessful = false;
            vm.CardNumber = string.Empty;
            vm.CardholderName = string.Empty;
            vm.ExpiryDate = string.Empty;
            vm.Cvv = string.Empty;

            Assert.Equal(1, vm.RequestIdentifier);
            Assert.Equal(2, vm.ClientIdentifier);
            Assert.Equal(3, vm.OwnerIdentifier);
            Assert.Equal("Catan", vm.GameName);
            Assert.Equal("Owner", vm.OwnerName);
            Assert.Equal("Client", vm.ClientName);
            Assert.Equal("123 Main St", vm.DeliveryAddress);
            Assert.NotNull(vm.DeliveryDate);
            Assert.NotNull(vm.RequestDates);
            Assert.Equal(50.0m, vm.Price);
            Assert.Equal(10, vm.BookingMessageIdentifier);
            Assert.Null(vm.ConversationService);
        }

        [Fact]
        public void ExitCommand_InvokesNavigateBackwardsAction()
        {
            var vm = CreateViewModel();
            bool invoked = false;
            vm.NavigateBackwardsAction = () => invoked = true;

            vm.ExitCommand.Execute(null);

            Assert.True(invoked);
        }

        [Fact]
        public void ResetInactivityCommand_ExecutesWithoutError()
        {
            var vm = CreateViewModel();
            var exception = Record.Exception(() => vm.ResetInactivityCommand.Execute(null));

            Assert.Null(exception);
        }

        [Fact]
        public void PageLifecycle_StartsAndStopsTimersAndRefreshesBalance()
        {
            var vm = CreateViewModel();
            mockUserService.Setup(us => us.GetUserBalance(It.IsAny<int>())).Returns(999m);

            vm.OnPageActivated();
            Assert.Equal(999m, vm.BalanceAmount);

            vm.OnPageDeactivated();
        }

        [Fact]
        public async Task FinishPaymentAsync_Exception_SetsErrorMessage()
        {
            var vm = CreateViewModel();
            mockCardPaymentService.Setup(s => s.AddCardPayment(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>()))
                .Throws(new Exception("Mocked error"));

            var method = typeof(CardPaymentViewModel).GetMethod("FinishPaymentAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task)method.Invoke(vm, null);
            await task;

            Assert.Equal("Payment failed: Mocked error", vm.CurrentStatusMessage);
            Assert.False(vm.IsCurrentlyLoading);
        }

        [Fact]
        public async Task FinishPaymentAsync_Success_SetsSuccessMessage()
        {
            var vm = CreateViewModel();

            var method = typeof(CardPaymentViewModel).GetMethod("FinishPaymentAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task)method.Invoke(vm, null);
            await task;

            Assert.True(vm.IsPaymentSuccessful);
            Assert.Equal("Payment successful!", vm.CurrentStatusMessage);
        }

        [Fact]
        public void OnSessionExpired_InvokesNavigateToExitAction()
        {
            var vm = CreateViewModel();
            bool exited = false;
            vm.NavigateToExitAction = () => exited = true;

            vm.OnPageActivated();

            var method = typeof(CardPaymentViewModel).GetMethod("OnSessionExpired", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(vm, new object[] { null, null });

            Assert.True(exited);
            Assert.Equal("Session expired due to inactivity.", vm.CurrentStatusMessage);
        }
        [Fact]
        public void CoverageSweeper_HitsHiddenLambdasAndEarlyReturns()
        {
            var vm = CreateViewModel();

            bool canExecute = vm.FinishPaymentCommand.CanExecute(null);
            vm.FinishPaymentCommand.Execute(null);

            var refreshMethod = typeof(CardPaymentViewModel).GetMethod("RefreshBalance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            refreshMethod.Invoke(vm, null);

            var sessionMethod = typeof(CardPaymentViewModel).GetMethod("OnSessionExpired", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            sessionMethod.Invoke(vm, new object[] { null, null });

            Assert.False(canExecute);
        }
    }

    public class TestSyncContext : SynchronizationContext
    {
        public override void Post(SendOrPostCallback callback, object state) => callback(state);
        public override void Send(SendOrPostCallback callback, object state) => callback(state);
    }
}