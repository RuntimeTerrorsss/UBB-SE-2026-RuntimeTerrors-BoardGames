using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.PaymentHistory.DTO;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Enums;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Service;

namespace BookingBoardgamesILoveBan.Src.PaymentHistory.ViewModel
{
    public class FilterOption
    {
        public FilterType Type { get; set; }
        public string DisplayName { get; set; }
    }

    public class PaymentHistoryViewModel : ViewModelBase
    {
        private readonly IServicePayment service;
        private FilterOption selectedFilterOption;
        private PaymentMethod selectedPaymentMethod;
        private string searchText = string.Empty;
        private CancellationTokenSource searchCts;
        private decimal totalAmount;

        private int currentPage = 1;
        private int pageSize = 10;
        private int totalPages = 1;

        public ObservableCollection<PaymentDto> Payments { get; set; }

        public RelayCommand<PaymentDto> OpenReceiptCommand { get; }
        public RelayCommandNoParam NextPageCommand { get; }
        public RelayCommandNoParam PreviousPageCommand { get; }

        public int CurrentPage
        {
            get => currentPage;
            set
            {
                if (SetProperty(ref currentPage, value))
                {
                    NextPageCommand?.RaiseCanExecuteChanged();
                    PreviousPageCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public int TotalPages
        {
            get => totalPages;
            set
            {
                if (SetProperty(ref totalPages, value))
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
            get => searchText;
            set
            {
                if (SetProperty(ref searchText, value))
                {
                    searchCts?.Cancel();
                    searchCts = new CancellationTokenSource();
                    DebounceSearch(searchCts.Token);
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
            get => selectedFilterOption;
            set
            {
                if (SetProperty(ref selectedFilterOption, value))
                {
                    ApplyFilter(resetPage: true);
                }
            }
        }

        public PaymentMethod SelectedPaymentMethod
        {
            get => selectedPaymentMethod;
            set
            {
                if (SetProperty(ref selectedPaymentMethod, value))
                {
                    ApplyFilter(resetPage: true);
                }
            }
        }

        public decimal TotalAmount
        {
            get => totalAmount;
            private set => SetProperty(ref totalAmount, value);
        }

        public PaymentHistoryViewModel(IServicePayment paymentService)
        {
            service = paymentService;
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
            if (paymentDto == null)
            {
                return;
            }
            string path = service.GetReceiptDocumentPath(paymentDto.Id);

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
            if (selectedFilterOption == null)
            {
                return;
            }

            if (resetPage)
            {
                CurrentPage = 1;
            }

            var pagedResult = service.GetFilteredPayments(selectedFilterOption.Type, selectedPaymentMethod, searchText, CurrentPage, pageSize);

            Payments.Clear();
            foreach (var paymentDto in pagedResult.Items)
            {
                Payments.Add(paymentDto);
            }

            TotalPages = pagedResult.TotalPages == 0 ? 1 : pagedResult.TotalPages;

            TotalAmount = service.CalculateTotalAmount(pagedResult.Items);
        }
    }
}
