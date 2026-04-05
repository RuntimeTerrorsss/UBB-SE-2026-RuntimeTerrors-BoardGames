using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.src.PaymentCard.Commands;
using BookingBoardgamesILoveBan.src.PaymentCard.Constants;
using BookingBoardgamesILoveBan.src.PaymentCard.Service;
using BookingBoardgamesILoveBan.src.Chat.Service;
using BookingBoardgamesILoveBan.src.Mocks.RequestMock;
using BookingBoardgamesILoveBan.src.Mocks.UserMock;

namespace BookingBoardgamesILoveBan.src.PaymentCard.ViewModel
{
    public class CardPaymentViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly CardPaymentService _cardPaymentService;
        private readonly UserService _userService;
        private readonly System.Timers.Timer _inactivityTimer;
        private readonly System.Timers.Timer _balanceRefreshTimer;
        private readonly SynchronizationContext _syncContext;

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
        public int BookingMessageId {  get; init; }
        public ConversationService ConversationService { get; init; }

        // Observable state 
        private decimal balance;
        public decimal Balance
        {
            get => balance;
            set
            {
                if (balance == value) return;
                balance = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPayButtonEnabled));
                OnPropertyChanged(nameof(IsWarningVisible));
            }
        }

        private bool _termsAccepted;
        public bool TermsAccepted
        {
            get => _termsAccepted;
            set
            {
                if (_termsAccepted == value) return;
                _termsAccepted = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPayButtonEnabled));
                FinishPaymentCommand.NotifyCanExecuteChanged(); // add this
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading == value) return;
                _isLoading = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPayButtonEnabled));
            }
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        private bool _isSuccess;
        public bool IsSuccess
        {
            get => _isSuccess;
            set { _isSuccess = value; OnPropertyChanged(); }
        }

        // Properties
        public bool IsPayButtonEnabled => Balance >= Price && TermsAccepted && !IsLoading;
        public bool IsWarningVisible => Balance < Price;

        //  Commands
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
            this._cardPaymentService = cardPaymentService;
            this._userService = userService;

            RequestId = requestId; 
            DeliveryAddress = deliveryAddress; 
            BookingMessageId = bookingMessageId;
            RequestDto requestDto = this._cardPaymentService.GetRequestDto(requestId);
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
            _balanceRefreshTimer = new System.Timers.Timer(CardPaymentConstants.TimerForRefreshingBalance);
            _balanceRefreshTimer.Elapsed += (_, _) => RefreshBalance();
            _balanceRefreshTimer.AutoReset = true;

            // Session timeout after 2 minutes
            _inactivityTimer = new System.Timers.Timer(CardPaymentConstants.TimerBeforeClosingPayment);
            _inactivityTimer.Elapsed += OnSessionExpired;
            _inactivityTimer.AutoReset = false;

            _syncContext = SynchronizationContext.Current;
        }

        // Lifecycle 
        private bool _isPageActive = false;
        public void OnActivated()
        {
            _isPageActive = true;
            RefreshBalance();
            _balanceRefreshTimer.Start();
            _inactivityTimer.Start();
        }

        public void OnDeactivated()
        {
            _balanceRefreshTimer.Stop();
            _balanceRefreshTimer?.Dispose();
            _inactivityTimer.Stop();
            _inactivityTimer?.Dispose();
        }

        // Helpers
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RefreshBalance()
        {
            if (!_isPageActive) return;
            decimal newBalance = _userService.GetUserBalance(ClientId);
            _syncContext.Post(_ =>
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
                    _cardPaymentService.AddCardPayment(RequestId, ClientId, OwnerId, Price));

                this.ConversationService.OnCardPaymentSelected(this.BookingMessageId);
                RefreshBalance();
                IsSuccess = true;
                StatusMessage = "Payment successful!";
                _balanceRefreshTimer.Stop();
                _inactivityTimer.Stop();
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
            if (!_isPageActive) return;
            _balanceRefreshTimer.Stop();
            StatusMessage = "Session expired due to inactivity.";
            _syncContext.Post(_ =>
            {
                if (!_isPageActive) return;
                NavigateToExit?.Invoke();
            }, null);
        }

        private void ResetInactivityTimer()
        {
            _inactivityTimer.Stop();
            _inactivityTimer.Start();
        }
    }
}