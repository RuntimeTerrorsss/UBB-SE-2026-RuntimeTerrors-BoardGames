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
        private readonly UserService userService;
        private readonly System.Timers.Timer inactivityTimer;
        private readonly System.Timers.Timer balanceRefreshTimer;
        private readonly SynchronizationContext syncContext;

        // Booking info
        public int RequestId { get; init; }
        public int ClientId { get; init; }
        public int OwnerId { get; init; }
        public string GameName { get; init; }
        public string OwnerName { get; init; }

        public string ClientName { get; init; }
        public string DeliveryAddress { get; init; }

        public string DeliveryDate { get; init; }
        public string RequestDates { get; init; }
        public decimal Price { get; init; }
        public int BookingMessageId { get; init; }
        public ConversationService ConversationService { get; init; }

        // Observable state
        private decimal balance;
        public decimal Balance
        {
            get => balance;
            set
            {
                if (balance == value)
                {
                    return;
                }
                balance = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPayButtonEnabled));
                OnPropertyChanged(nameof(IsWarningVisible));
            }
        }

        private bool termsAccepted;
        public bool TermsAccepted
        {
            get => termsAccepted;
            set
            {
                if (termsAccepted == value)
                {
                    return;
                }
                termsAccepted = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPayButtonEnabled));
                FinishPaymentCommand.NotifyCanExecuteChanged(); // add this
            }
        }

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set
            {
                if (isLoading == value)
                {
                    return;
                }
                isLoading = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPayButtonEnabled));
            }
        }

        private string statusMessage = string.Empty;
        public string StatusMessage
        {
            get => statusMessage;
            set
            {
                statusMessage = value;
                OnPropertyChanged();
            }
        }

        private bool isSuccess;
        public bool IsSuccess
        {
            get => isSuccess;
            set
            {
                isSuccess = value;
                OnPropertyChanged();
            }
        }

        // Properties
        public bool IsPayButtonEnabled => Balance >= Price && TermsAccepted && !IsLoading;
        public bool IsWarningVisible => Balance < Price;

        // Commands
        public RelayCommand FinishPaymentCommand { get; }
        public RelayCommand ExitCommand { get; }
        public RelayCommand ResetInactivityCommand { get; }

        // Navigation
        public Action NavigateBack { get; set; }
        public Action NavigateToExit { get; set; }

        public CardPaymentViewModel(
            CardPaymentService cardPaymentService,
            UserService userService,
            int requestId,
            string deliveryAddress,
            int bookingMessageId,
            ConversationService conversationService)
        {
            this.cardPaymentService = cardPaymentService;
            this.userService = userService;

            RequestId = requestId;
            DeliveryAddress = deliveryAddress;
            BookingMessageId = bookingMessageId;
            RequestDto requestDto = this.cardPaymentService.GetRequestDto(requestId);
            ConversationService = conversationService;

            ClientId = requestDto.ClientId;
            OwnerId = requestDto.OwnerId;
            GameName = requestDto.GameName;
            OwnerName = requestDto.OwnerName;
            ClientName = requestDto.ClientName;
            RequestDates = requestDto.StartDate.ToShortDateString() + " to " + requestDto.EndDate.Date.Date.ToShortDateString();
            Price = requestDto.Price;
            DeliveryDate = requestDto.StartDate.ToShortDateString();

            FinishPaymentCommand = new RelayCommand(() => _ = FinishPaymentAsync(), () => IsPayButtonEnabled);
            ExitCommand = new RelayCommand(() => NavigateBack?.Invoke());
            ResetInactivityCommand = new RelayCommand(ResetInactivityTimer);

            // Balance refresh every 4 seconds
            balanceRefreshTimer = new System.Timers.Timer(CardPaymentConstants.TimerForRefreshingBalance);
            balanceRefreshTimer.Elapsed += (_, _) => RefreshBalance();
            balanceRefreshTimer.AutoReset = true;

            // Session timeout after 2 minutes
            inactivityTimer = new System.Timers.Timer(CardPaymentConstants.TimerBeforeClosingPayment);
            inactivityTimer.Elapsed += OnSessionExpired;
            inactivityTimer.AutoReset = false;

            syncContext = SynchronizationContext.Current;
        }

        // Lifecycle
        private bool isPageActive = false;
        public void OnActivated()
        {
            isPageActive = true;
            RefreshBalance();
            balanceRefreshTimer.Start();
            inactivityTimer.Start();
        }

        public void OnDeactivated()
        {
            balanceRefreshTimer.Stop();
            balanceRefreshTimer?.Dispose();
            inactivityTimer.Stop();
            inactivityTimer?.Dispose();
        }

        // Helpers
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RefreshBalance()
        {
            if (!isPageActive)
            {
                return;
            }
            decimal newBalance = userService.GetUserBalance(ClientId);
            syncContext.Post(_ =>
            {
                Balance = newBalance;
                FinishPaymentCommand.NotifyCanExecuteChanged();
            }, null);
        }

        private async Task FinishPaymentAsync()
        {
            IsLoading = true;
            StatusMessage = string.Empty;
            FinishPaymentCommand.NotifyCanExecuteChanged();

            await Task.Delay(CardPaymentConstants.LoadingTime);

            try
            {
                await Task.Run(() =>
                    cardPaymentService.AddCardPayment(RequestId, ClientId, OwnerId, Price));

                this.ConversationService.OnCardPaymentSelected(this.BookingMessageId);
                RefreshBalance();
                IsSuccess = true;
                StatusMessage = "Payment successful!";
                balanceRefreshTimer.Stop();
                inactivityTimer.Stop();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Payment failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                FinishPaymentCommand.NotifyCanExecuteChanged();
            }
        }

        private void OnSessionExpired(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!isPageActive)
            {
                return;
            }
            balanceRefreshTimer.Stop();
            StatusMessage = "Session expired due to inactivity.";
            syncContext.Post(_ =>
            {
                if (!isPageActive)
                {
                    return;
                }
                NavigateToExit?.Invoke();
            }, null);
        }

        private void ResetInactivityTimer()
        {
            inactivityTimer.Stop();
            inactivityTimer.Start();
        }
    }
}