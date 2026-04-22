using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.PaymentHistory.DTO;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Enums;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Service;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Constants;

namespace BookingBoardgamesILoveBan.Src.PaymentHistory.ViewModel
{
    public class FilterOption
    {
        public FilterType Type { get; set; }
        public string DisplayName { get; set; }
    }

    public class PaymentHistoryViewModel : ViewModelBase
    {
        private readonly IServicePayment paymentService;
        private FilterOption selectedFilterOption;
        private PaymentMethod selectedPaymentMethod;
        private string searchText = string.Empty;
        private CancellationTokenSource searchCancellationTokenSource;
        private decimal totalAmount;

        private int currentPage = PaymentHistoryViewModelConstants.FirstPage;
        private int pageSize = PaymentHistoryViewModelConstants.DefaultPageSize;
        private int totalPages = PaymentHistoryViewModelConstants.StartupTotalPagesCount;

        private const int MinimumPageCount = PaymentHistoryViewModelConstants.MinimumPagesCount;

        public ObservableCollection<PaymentDataTransferObject> Payments { get; set; }

        public RelayCommand<PaymentDataTransferObject> OpenReceiptCommand { get; }
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
                    searchCancellationTokenSource?.Cancel();
                    searchCancellationTokenSource = new CancellationTokenSource();
                    DebounceSearch(searchCancellationTokenSource.Token);
                }
            }
        }

        private async void DebounceSearch(CancellationToken searchCancellationToken)
        {
            try
            {
                await Task.Delay(PaymentHistoryViewModelConstants.TaskDelayTime, searchCancellationToken);
                if (!searchCancellationToken.IsCancellationRequested)
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
            this.paymentService = paymentService;
            Payments = new ObservableCollection<PaymentDataTransferObject>();

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

            OpenReceiptCommand = new RelayCommand<PaymentDataTransferObject>(OpenReceipt);
            NextPageCommand = new RelayCommandNoParam(OnNextPage, () => CurrentPage < TotalPages);
            PreviousPageCommand = new RelayCommandNoParam(OnPreviousPage, () => CurrentPage > PaymentHistoryViewModelConstants.FirstPage);

            // Default to display all
            SelectedFilterOption = FilterOptions.First(filter => filter.Type == FilterType.AllTime);
            SelectedPaymentMethod = PaymentMethod.ALL;
        }

        private bool OnLastPage()
        {
            return CurrentPage == TotalPages;
        }

        private void OnNextPage()
        {
            if (!OnLastPage())
            {
                CurrentPage++;
                ApplyFilter(resetPage: false);
            }
        }

        private bool OnFirstPage()
        {
            return CurrentPage == PaymentHistoryViewModelConstants.FirstPage;
        }

        private void OnPreviousPage()
        {
            if (!OnFirstPage())
            {
                CurrentPage--;
                ApplyFilter(resetPage: false);
            }
        }

        private async void OpenReceipt(PaymentDataTransferObject selectedPayment)
        {
            if (selectedPayment == null)
            {
                return;
            }
            string receiptFilePath = paymentService.GetReceiptDocumentPath(selectedPayment.PaymentId);

            try
            {
                var receiptFileInfo = new System.IO.FileInfo(receiptFilePath);
                if (receiptFileInfo.Exists)
                {
                    // windows storage file reference to launch safely
                    var storageFile = await Windows.Storage.StorageFile.GetFileFromPathAsync(receiptFileInfo.FullName);
                    await Windows.System.Launcher.LaunchFileAsync(storageFile);
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
                CurrentPage = PaymentHistoryViewModelConstants.FirstPage;
            }

            var pagedResult = paymentService.GetFilteredPayments(selectedFilterOption.Type, selectedPaymentMethod, searchText, CurrentPage, pageSize);

            Payments.Clear();
            foreach (var payment in pagedResult.Items)
            {
                Payments.Add(payment);
            }

            TotalPages = pagedResult.TotalPages == PaymentHistoryViewModelConstants.NoPages ? MinimumPageCount : pagedResult.TotalPages;

            TotalAmount = paymentService.CalculateTotalAmount(pagedResult.Items);
        }
    }
}
