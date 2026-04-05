using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.src.PaymentHistory.DTO;
using BookingBoardgamesILoveBan.src.PaymentHistory.Enums;
using BookingBoardgamesILoveBan.src.PaymentHistory.Service;

namespace BookingBoardgamesILoveBan.src.PaymentHistory.ViewModel
{
    public class FilterOption
    {
        public FilterType Type { get; set; }
        public string DisplayName { get; set; }
    }

    public class PaymentHistoryViewModel : ViewModelBase
    {
        private readonly IServicePayment _service;
        private FilterOption _selectedFilterOption;
        private PaymentMethod _selectedPaymentMethod;
        private string _searchText = string.Empty;
        private CancellationTokenSource _searchCts;
        private decimal _totalAmount;

        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalPages = 1;

        public ObservableCollection<PaymentDto> Payments { get; set; }

        public RelayCommand<PaymentDto> OpenReceiptCommand { get; }
        public RelayCommandNoParam NextPageCommand { get; }
        public RelayCommandNoParam PreviousPageCommand { get; }

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (SetProperty(ref _currentPage, value))
                {
                    NextPageCommand?.RaiseCanExecuteChanged();
                    PreviousPageCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public int TotalPages
        {
            get => _totalPages;
            set
            {
                if (SetProperty(ref _totalPages, value))
                {
                    NextPageCommand?.RaiseCanExecuteChanged();
                    PreviousPageCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<FilterOption> FilterOptions { get; }
        public IEnumerable<PaymentMethod> PaymentMethodOptions { get; } = System.Enum.GetValues(typeof(PaymentMethod)).Cast<PaymentMethod>();

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    _searchCts?.Cancel();
                    _searchCts = new CancellationTokenSource();
                    DebounceSearch(_searchCts.Token);
                }
            }
        }

        private async void DebounceSearch(CancellationToken token)
        {
            try
            {
                await Task.Delay(500, token);
                if (!token.IsCancellationRequested)
                {
                    ApplyFilter(resetPage: true);
                }
            }
            catch (TaskCanceledException)
            {

            }
        }

        public FilterOption SelectedFilterOption
        {
            get => _selectedFilterOption;
            set
            {
                if (SetProperty(ref _selectedFilterOption, value))
                {
                    ApplyFilter(resetPage: true);
                }
            }
        }

        public PaymentMethod SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set
            {
                if (SetProperty(ref _selectedPaymentMethod, value))
                {
                    ApplyFilter(resetPage: true);
                }
            }
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            private set => SetProperty(ref _totalAmount, value);
        }

        public PaymentHistoryViewModel(IServicePayment service)
        {
            _service = service;
            Payments = new ObservableCollection<PaymentDto>();

            FilterOptions = new ObservableCollection<FilterOption>
            {
                new FilterOption { Type = FilterType.AllTime, DisplayName = "All Time" },
                new FilterOption { Type = FilterType.Last3Months, DisplayName = "Last 3 Months" },
                new FilterOption { Type = FilterType.Last6Months, DisplayName = "Last 6 Months" },
                new FilterOption { Type = FilterType.Last9Months, DisplayName = "Last 9 Months" },
                new FilterOption { Type = FilterType.Newest, DisplayName = "Date: Newest First" },
                new FilterOption { Type = FilterType.Oldest, DisplayName = "Date: Oldest First" },
                new FilterOption { Type = FilterType.AlphabeticalAsc, DisplayName = "Alphabetical (A-Z)" },
                new FilterOption { Type = FilterType.AlphabeticalDesc, DisplayName = "Alphabetical (Z-A)" }
            };

            OpenReceiptCommand = new RelayCommand<PaymentDto>(OpenReceipt);
            NextPageCommand = new RelayCommandNoParam(OnNextPage, () => CurrentPage < TotalPages);
            PreviousPageCommand = new RelayCommandNoParam(OnPreviousPage, () => CurrentPage > 1);

            // Default to display all
            SelectedFilterOption = FilterOptions.First(f => f.Type == FilterType.AllTime);
            SelectedPaymentMethod = PaymentMethod.ALL;
        }

        private void OnNextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                ApplyFilter(resetPage: false);
            }
        }

        private void OnPreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                ApplyFilter(resetPage: false);
            }
        }

        private async void OpenReceipt(PaymentDto paymentDto)
        {
            if (paymentDto == null) return;
            string path = _service.GetReceiptDocumentPath(paymentDto.Id);

            try 
            {
                var fileInfo = new System.IO.FileInfo(path);
                if (fileInfo.Exists)
                {
                    // windows storage file reference to launch safely
                    var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(fileInfo.FullName);
                    await Windows.System.Launcher.LaunchFileAsync(file);
                }
            } 
            catch (System.Exception) 
            {
            }
        }

        private void ApplyFilter(bool resetPage = false)
        {
            if (_selectedFilterOption == null) return;

            if (resetPage)
            {
                CurrentPage = 1;
            }

            var pagedResult = _service.GetFilteredPayments(_selectedFilterOption.Type, _selectedPaymentMethod, _searchText, CurrentPage, _pageSize);

            Payments.Clear();
            foreach (var paymentDto in pagedResult.Items)
            {
                Payments.Add(paymentDto);
            }

            TotalPages = pagedResult.TotalPages == 0 ? 1 : pagedResult.TotalPages;

           
            TotalAmount = _service.CalculateTotalAmount(pagedResult.Items);
        }
    }
}
