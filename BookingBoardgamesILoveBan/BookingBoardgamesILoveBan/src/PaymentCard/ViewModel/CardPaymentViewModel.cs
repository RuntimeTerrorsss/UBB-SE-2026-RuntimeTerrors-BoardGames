using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.PaymentCard.Commands;
using BookingBoardgamesILoveBan.Src.PaymentCard.Constants;
using BookingBoardgamesILoveBan.Src.PaymentCard.Service;
using BookingBoardgamesILoveBan.Src.Chat.Service;
using BookingBoardgamesILoveBan.Src.Mocks.RequestMock;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;

namespace BookingBoardgamesILoveBan.Src.PaymentCard.ViewModel
{
    public class CardPaymentViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly CardPaymentService cardPaymentService;
        private readonly IUserService userService;
        private readonly System.Timers.Timer inactivityTimer;
        private readonly System.Timers.Timer balanceRefreshTimer;
        private readonly SynchronizationContext synchronizationContext;

        public int RequestIdentifier { get; init; }
        public int ClientIdentifier { get; init; }
        public int OwnerIdentifier { get; init; }
        public string GameName { get; init; }
        public string OwnerName { get; init; }
        public string ClientName { get; init; }
        public string DeliveryAddress { get; init; }
        public string DeliveryDate { get; init; }
        public string RequestDates { get; init; }
        public decimal Price { get; init; }
        public int BookingMessageIdentifier { get; init; }
        public ConversationService ConversationService { get; init; }

        private decimal balanceAmount;
        public decimal BalanceAmount
        {
            get => balanceAmount;
            set
            {
                if (balanceAmount == value)
                {
                    return;
                }

                balanceAmount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPaymentButtonEnabled));
                OnPropertyChanged(nameof(IsWarningMessageVisible));
            }
        }

        private bool areTermsAccepted;
        public bool AreTermsAccepted
        {
            get => areTermsAccepted;
            set
            {
                if (areTermsAccepted == value)
                {
                    return;
                }

                areTermsAccepted = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPaymentButtonEnabled));
                FinishPaymentCommand.NotifyCanExecuteChanged();
            }
        }

        private bool isCurrentlyLoading;
        public bool IsCurrentlyLoading
        {
            get => isCurrentlyLoading;
            set
            {
                if (isCurrentlyLoading == value)
                {
                    return;
                }

                isCurrentlyLoading = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPaymentButtonEnabled));
            }
        }

        private string currentStatusMessage = string.Empty;
        public string CurrentStatusMessage
        {
            get => currentStatusMessage;
            set
            {
                currentStatusMessage = value;
                OnPropertyChanged();
            }
        }

        private bool isPaymentSuccessful;
        public bool IsPaymentSuccessful
        {
            get => isPaymentSuccessful;
            set
            {
                isPaymentSuccessful = value;
                OnPropertyChanged();
            }
        }

        private string cardNumber = string.Empty;
        public string CardNumber
        {
            get => cardNumber;
            set
            {
                if (cardNumber == value)
                {
                    return;
                }

                cardNumber = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPaymentButtonEnabled));
                FinishPaymentCommand.NotifyCanExecuteChanged();
            }
        }

        private string cardholderName = string.Empty;
        public string CardholderName
        {
            get => cardholderName;
            set
            {
                if (cardholderName == value)
                {
                    return;
                }

                cardholderName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPaymentButtonEnabled));
                FinishPaymentCommand.NotifyCanExecuteChanged();
            }
        }

        private string expiryDate = string.Empty;
        public string ExpiryDate
        {
            get => expiryDate;
            set
            {
                if (expiryDate == value)
                {
                    return;
                }

                expiryDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPaymentButtonEnabled));
                FinishPaymentCommand.NotifyCanExecuteChanged();
            }
        }

        private string cvv = string.Empty;
        public string Cvv
        {
            get => cvv;
            set
            {
                if (cvv == value)
                {
                    return;
                }

                cvv = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPaymentButtonEnabled));
                FinishPaymentCommand.NotifyCanExecuteChanged();
            }
        }

        public bool IsPaymentButtonEnabled =>
            BalanceAmount >= Price &&
            AreTermsAccepted &&
            !IsCurrentlyLoading &&
            !string.IsNullOrWhiteSpace(CardNumber) &&
            !string.IsNullOrWhiteSpace(CardholderName) &&
            !string.IsNullOrWhiteSpace(ExpiryDate) &&
            !string.IsNullOrWhiteSpace(Cvv);

        public bool IsWarningMessageVisible => BalanceAmount < Price;

        public RelayCommand FinishPaymentCommand { get; }
        public RelayCommand ExitCommand { get; }
        public RelayCommand ResetInactivityCommand { get; }

        public Action NavigateBackwardsAction { get; set; }
        public Action NavigateToExitAction { get; set; }

        public CardPaymentViewModel(
            CardPaymentService cardPaymentService,
            IUserService userService,
            int requestId,
            string deliveryAddress,
            int bookingMessageIdentifier,
            ConversationService conversationService)
        {
            this.cardPaymentService = cardPaymentService;
            this.userService = userService;

            RequestIdentifier = requestId;
            DeliveryAddress = deliveryAddress;
            BookingMessageIdentifier = bookingMessageIdentifier;
            RequestDto requestDataTransferObject = this.cardPaymentService.GetRequestDataTransferObject(requestId);
            ConversationService = conversationService;

            ClientIdentifier = requestDataTransferObject.ClientId;
            OwnerIdentifier = requestDataTransferObject.OwnerId;
            GameName = requestDataTransferObject.GameName;
            OwnerName = requestDataTransferObject.OwnerName;
            ClientName = requestDataTransferObject.ClientName;
            RequestDates = requestDataTransferObject.StartDate.ToShortDateString() + " to " + requestDataTransferObject.EndDate.Date.Date.ToShortDateString();
            Price = requestDataTransferObject.Price;
            DeliveryDate = requestDataTransferObject.StartDate.ToShortDateString();

            FinishPaymentCommand = new RelayCommand(() => _ = FinishPaymentAsync(), () => IsPaymentButtonEnabled);
            ExitCommand = new RelayCommand(() => NavigateBackwardsAction?.Invoke());
            ResetInactivityCommand = new RelayCommand(ResetInactivityTimer);

            balanceRefreshTimer = new System.Timers.Timer(CardPaymentConstants.TimerForRefreshingBalance);
            balanceRefreshTimer.Elapsed += (timerSender, timerEventArguments) => RefreshBalance();
            balanceRefreshTimer.AutoReset = true;

            inactivityTimer = new System.Timers.Timer(CardPaymentConstants.TimerBeforeClosingPayment);
            inactivityTimer.Elapsed += OnSessionExpired;
            inactivityTimer.AutoReset = false;

            synchronizationContext = SynchronizationContext.Current;
        }

        private bool isPageCurrentlyActive = false;

        public void OnPageActivated()
        {
            isPageCurrentlyActive = true;
            RefreshBalance();
            balanceRefreshTimer.Start();
            inactivityTimer.Start();
        }

        public void OnPageDeactivated()
        {
            balanceRefreshTimer.Stop();
            balanceRefreshTimer?.Dispose();
            inactivityTimer.Stop();
            inactivityTimer?.Dispose();
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RefreshBalance()
        {
            if (!isPageCurrentlyActive)
            {
                return;
            }

            decimal newBalance = userService.GetUserBalance(ClientIdentifier);
            synchronizationContext.Post(threadState =>
            {
                BalanceAmount = newBalance;
                FinishPaymentCommand.NotifyCanExecuteChanged();
            }, null);
        }

        private async Task FinishPaymentAsync()
        {
            IsCurrentlyLoading = true;
            CurrentStatusMessage = string.Empty;
            FinishPaymentCommand.NotifyCanExecuteChanged();

            await Task.Delay(CardPaymentConstants.LoadingTime);

            try
            {
                await Task.Run(() =>
                    cardPaymentService.AddCardPayment(RequestIdentifier, ClientIdentifier, OwnerIdentifier, Price));

                this.ConversationService.OnCardPaymentSelected(this.BookingMessageIdentifier);
                RefreshBalance();
                IsPaymentSuccessful = true;
                CurrentStatusMessage = "Payment successful!";
                balanceRefreshTimer.Stop();
                inactivityTimer.Stop();
            }
            catch (Exception paymentException)
            {
                CurrentStatusMessage = $"Payment failed: {paymentException.Message}";
            }
            finally
            {
                IsCurrentlyLoading = false;
                FinishPaymentCommand.NotifyCanExecuteChanged();
            }
        }

        private void OnSessionExpired(object timerSender, System.Timers.ElapsedEventArgs elapsedEventArguments)
        {
            if (!isPageCurrentlyActive)
            {
                return;
            }

            balanceRefreshTimer.Stop();
            CurrentStatusMessage = "Session expired due to inactivity.";
            synchronizationContext.Post(threadState =>
            {
                if (!isPageCurrentlyActive)
                {
                    return;
                }

                NavigateToExitAction?.Invoke();
            }, null);
        }

        private void ResetInactivityTimer()
        {
            inactivityTimer.Stop();
            inactivityTimer.Start();
        }
    }
}