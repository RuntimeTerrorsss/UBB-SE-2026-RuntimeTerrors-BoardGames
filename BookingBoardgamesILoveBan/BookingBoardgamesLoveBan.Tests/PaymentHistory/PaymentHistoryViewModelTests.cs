using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.PaymentHistory.DTO;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Enums;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Service;
using BookingBoardgamesILoveBan.Src.PaymentHistory.ViewModel;

namespace BookingBoardgamesLoveBan.Tests.PaymentHistory
{
    public class PaymentHistoryViewModelTests // unit tests
    {
        // ================================ fakes ======================================
        private class FakeServicePayment : IServicePayment
        {
            public List<PaymentDto> PaymentsToReturn { get; set; } = new ();
            public int TotalCount { get; set; } = 0;

            public List<PaymentDto> GetAllPaymentsForUI()
            {
                return PaymentsToReturn;
            }

            public PagedResult<PaymentDto> GetFilteredPayments(FilterType filter, PaymentMethod paymentMethod = PaymentMethod.ALL, string searchQuery = "", int pageNumber = 1, int pageSize = 10)
            {
                return new PagedResult<PaymentDto>
                {
                    Items = PaymentsToReturn,
                    TotalCount = TotalCount == 0 ? PaymentsToReturn.Count : TotalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }

            public decimal CalculateTotalAmount(IEnumerable<PaymentDto> payments)
            {
                return payments?.Sum(p => p.Amount) ?? 0;
            }

            public string GetReceiptDocumentPath(int paymentId)
            {
                return $"C:\\receipts\\receipt_{paymentId}.pdf";
            }
        }

        // ================================ setup ======================================
        private FakeServicePayment paymentService;
        private PaymentHistoryViewModel viewModel;

        private void InitializeViewModel(List<PaymentDto>? payments = null)
        {
            paymentService = new FakeServicePayment();
            if (payments != null)
            {
                paymentService.PaymentsToReturn = payments;
                paymentService.TotalCount = payments.Count;
            }
            viewModel = new PaymentHistoryViewModel(paymentService);
        }

        private PaymentDto MakeDto(int id, string gameName, decimal amount, string method = "Card")
        {
            return new PaymentDto { Id = id, ProductName = gameName, Amount = amount, PaymentMethod = method };
        }

        // ================================ Constructor ======================================
        [Fact]
        public void Constructor_FilterOptionsArePopulated()
        {
            InitializeViewModel();
            Assert.NotEmpty(viewModel.FilterOptions);
        }

        [Fact]
        public void Constructor_HasEightFilterOptions()
        {
            InitializeViewModel();
            Assert.Equal(8, viewModel.FilterOptions.Count);
        }

        [Fact]
        public void Constructor_DefaultFilterIsAllTime()
        {
            InitializeViewModel();
            Assert.Equal(FilterType.AllTime, viewModel.SelectedFilterOption.Type);
        }

        [Fact]
        public void Constructor_DefaultPaymentMethodIsAll()
        {
            InitializeViewModel();
            Assert.Equal(PaymentMethod.ALL, viewModel.SelectedPaymentMethod);
        }

        [Fact]
        public void Constructor_DefaultPageIsOne()
        {
            InitializeViewModel();
            Assert.Equal(1, viewModel.CurrentPage);
        }

        [Fact]
        public void Constructor_FilterOptionsContainsAllTime()
        {
            InitializeViewModel();
            var types = viewModel.FilterOptions.Select(filter => filter.Type).ToList();

            Assert.Contains(FilterType.AllTime, types);
        }

        [Fact]
        public void Constructor_FilterOptionsContainsLast3Months()
        {
            InitializeViewModel();
            var types = viewModel.FilterOptions.Select(filter => filter.Type).ToList();

            Assert.Contains(FilterType.Last3Months, types);
        }

        [Fact]
        public void Constructor_FilterOptionsContainsLast6Months()
        {
            InitializeViewModel();
            var types = viewModel.FilterOptions.Select(filter => filter.Type).ToList();

            Assert.Contains(FilterType.Last6Months, types);
        }

        [Fact]
        public void Constructor_FilterOptionsContainsLast9Months()
        {
            InitializeViewModel();
            var types = viewModel.FilterOptions.Select(filter => filter.Type).ToList();

            Assert.Contains(FilterType.Last9Months, types);
        }

        [Fact]
        public void Constructor_FilterOptionsContainsNewest()
        {
            InitializeViewModel();
            var types = viewModel.FilterOptions.Select(filter => filter.Type).ToList();

            Assert.Contains(FilterType.Newest, types);
        }

        [Fact]
        public void Constructor_FilterOptionsContainsOldest()
        {
            InitializeViewModel();
            var types = viewModel.FilterOptions.Select(filter => filter.Type).ToList();

            Assert.Contains(FilterType.Oldest, types);
        }

        [Fact]
        public void Constructor_FilterOptionsContainsAlphabeticalAsc()
        {
            InitializeViewModel();
            var types = viewModel.FilterOptions.Select(filter => filter.Type).ToList();

            Assert.Contains(FilterType.AlphabeticalAsc, types);
        }

        [Fact]
        public void Constructor_FilterOptionsContainsAlphabeticalDesc()
        {
            InitializeViewModel();
            var types = viewModel.FilterOptions.Select(filter => filter.Type).ToList();

            Assert.Contains(FilterType.AlphabeticalDesc, types);
        }

        [Fact]
        public void Constructor_EmptyPayments_PaymentsCollectionIsEmpty()
        {
            InitializeViewModel(new List<PaymentDto>());
            Assert.Empty(viewModel.Payments);
        }

        [Fact]
        public void Constructor_LoadsPaymentsOnInit()
        {
            var payments = new List<PaymentDto>
                {
                    MakeDto(1, "Chess", 10),
                    MakeDto(2, "Risk", 20)
                };
            InitializeViewModel(payments);

            Assert.Equal(2, viewModel.Payments.Count);
        }

        [Fact]
        public void Constructor_TotalAmountIsCalculatedCorrectly()
        {
            var payments = new List<PaymentDto>
                {
                    MakeDto(1, "Chess", 10),
                    MakeDto(2, "Risk", 20)
                };
            InitializeViewModel(payments);

            Assert.Equal(30, viewModel.TotalAmount);
        }

        [Fact]
        public void Constructor_EmptyPayments_TotalAmountIsZero()
        {
            InitializeViewModel(new List<PaymentDto>());
            Assert.Equal(0, viewModel.TotalAmount);
        }

        // ================================ SearchText ======================================
        [Fact]
        public void SearchText_WhenSet_UpdatesProperty()
        {
            InitializeViewModel();
            viewModel.SearchText = "Chess";

            Assert.Equal("Chess", viewModel.SearchText);
        }

        [Fact]
        public void SearchText_WhenSetToSameValue_DoesNotTriggerDebounce()
        {
            InitializeViewModel();
            viewModel.SearchText = "Chess";
            bool fired = false;
            viewModel.PropertyChanged += (sender, eventArguments) =>
            {
                if (eventArguments.PropertyName == nameof(viewModel.SearchText))
                {
                    fired = true;
                }
            };

            viewModel.SearchText = "Chess";

            Assert.False(fired);
        }

        [Fact]
        public void SearchText_WhenSetToDifferentValue_TriggersDebounce()
        {
            InitializeViewModel();
            viewModel.SearchText = "Chess";
            bool fired = false;
            viewModel.PropertyChanged += (sender, eventArguments) =>
            {
                if (eventArguments.PropertyName == nameof(viewModel.SearchText))
                {
                    fired = true;
                }
            };

            viewModel.SearchText = "Catan";

            Assert.True(fired);
        }

        [Fact]
        public async Task SearchText_CancelsPreviousSearch()
        {
            try
            {
                InitializeViewModel();
                viewModel.SearchText = "a";
                viewModel.SearchText = "b";
                await Task.Delay(600);
            }
            catch
            {
                Assert.True(false); // must not run
            }
        }

        // ================================ OpenReceipt ======================================
        [Fact]
        public void OpenReceiptCommand_IsInitialized()
        {
            InitializeViewModel();
            Assert.NotNull(viewModel.OpenReceiptCommand);
        }

        [Fact]
        public void OpenReceipt_IfNull_DoNothing()
        {
            try
            {
                InitializeViewModel();
                viewModel.OpenReceiptCommand.Execute(null);
            }
            catch
            {
                Assert.True(false);
            }
        }

        [Fact]
        public void OpenReceipt_InexistentFile_DoNotThrow()
        {
            try
            {
                InitializeViewModel();
                var payment = new PaymentDto { Id = 999 };
                viewModel.OpenReceiptCommand.Execute(payment);
            }
            catch
            {
                Assert.True(false);
            }
        }

        // idk for existent

        // ================================ SelectedFilterOption ======================================
        [Fact]
        public void SelectedFilterOption_WhenChanged_ResetsToPageOne()
        {
            InitializeViewModel();
            viewModel.CurrentPage = 3;
            viewModel.SelectedFilterOption = viewModel.FilterOptions.First(filter => filter.Type == FilterType.Newest);

            Assert.Equal(1, viewModel.CurrentPage);
        }

        [Fact]
        public void SelectedFilterOption_WhenChanged_UpdatesPayments()
        {
            var payments = new List<PaymentDto> { MakeDto(1, "Chess", 10) };
            InitializeViewModel(payments);

            paymentService.PaymentsToReturn = new List<PaymentDto> { MakeDto(2, "Risk", 20) };
            viewModel.SelectedFilterOption = viewModel.FilterOptions.First(filter => filter.Type == FilterType.Newest);

            Assert.Equal("Risk", viewModel.Payments[0].ProductName);
        }

        // ================================ ApplyFilter ======================================
        [Fact]
        public void ApplyFilter_IfSelectedFilterIsNull_DoNothing()
        {
            try
            {
                InitializeViewModel();

                // set filter to null (break it intentionally)
                typeof(PaymentHistoryViewModel).GetField("selectedFilterOption", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(viewModel, null);

                viewModel.SearchText = "test";
            }
            catch
            {
                Assert.True(false);
            }
        }

        [Fact]
        public void ApplyFilter_WithItems_PopulatesPayments()
        {
            var payments = new List<PaymentDto>
                {
                    MakeDto(1, "Chess", 25m),
                    MakeDto(2, "Monopoly", 50m)
                };
            InitializeViewModel(payments);

            viewModel.SelectedFilterOption = viewModel.FilterOptions.First(filter => filter.Type == FilterType.Newest);

            Assert.Equal(2, viewModel.Payments.Count);
            Assert.Equal(75m, viewModel.TotalAmount);
        }

        // ================================ SelectedPaymentMethod ======================================
        [Fact]
        public void SelectedPaymentMethod_WhenChanged_ResetsToPageOne()
        {
            InitializeViewModel();
            viewModel.CurrentPage = 3;
            viewModel.SelectedPaymentMethod = PaymentMethod.CARD;

            Assert.Equal(1, viewModel.CurrentPage);
        }

        // ================================ NextPageCommand ======================================
        [Fact]
        public void NextPageCommand_CanExecute_TrueWhenNotOnLastPage()
        {
            InitializeViewModel();
            viewModel.TotalPages = 3;
            viewModel.CurrentPage = 1;

            Assert.True(viewModel.NextPageCommand.CanExecute(null));
        }

        [Fact]
        public void NextPage_WhenNotOnLastPage_IncrementsPage()
        {
            InitializeViewModel();
            viewModel.TotalPages = 3;
            viewModel.CurrentPage = 1;

            viewModel.NextPageCommand.Execute(null);

            Assert.Equal(2, viewModel.CurrentPage);
        }

        [Fact]
        public void NextPage_WhenOnLastPage_DoesNotIncrement()
        {
            InitializeViewModel();
            viewModel.TotalPages = 1;
            viewModel.CurrentPage = 1;

            viewModel.NextPageCommand.Execute(null);

            Assert.Equal(1, viewModel.CurrentPage);
        }

        [Fact]
        public void NextPageCommand_CanExecute_FalseOnLastPage()
        {
            InitializeViewModel();
            viewModel.TotalPages = 1;
            viewModel.CurrentPage = 1;

            Assert.False(viewModel.NextPageCommand.CanExecute(null));
        }

        // ================================ PreviousPageCommand ======================================
        [Fact]
        public void PreviousPageCommand_CanExecute_TrueWhenNotOnFirstPage()
        {
            InitializeViewModel();
            viewModel.TotalPages = 3;
            viewModel.CurrentPage = 2;

            Assert.True(viewModel.PreviousPageCommand.CanExecute(null));
        }

        [Fact]
        public void PreviousPage_WhenNotOnFirstPage_DecrementsPage()
        {
            InitializeViewModel();
            viewModel.TotalPages = 3;
            viewModel.CurrentPage = 2;

            viewModel.PreviousPageCommand.Execute(null);

            Assert.Equal(1, viewModel.CurrentPage);
        }

        [Fact]
        public void PreviousPage_WhenOnFirstPage_DoesNotDecrement()
        {
            InitializeViewModel();
            viewModel.CurrentPage = 1;

            viewModel.PreviousPageCommand.Execute(null);

            Assert.Equal(1, viewModel.CurrentPage);
        }

        [Fact]
        public void PreviousPageCommand_CanExecute_FalseOnFirstPage()
        {
            InitializeViewModel();
            viewModel.CurrentPage = 1;

            Assert.False(viewModel.PreviousPageCommand.CanExecute(null));
        }

        // ================================ TotalPages ======================================
        [Fact]
        public void TotalPages_WhenResultIsZero_DefaultsToOne()
        {
            InitializeViewModel();
            viewModel.SelectedFilterOption = viewModel.FilterOptions.First(filter => filter.Type == FilterType.AllTime);

            Assert.Equal(1, viewModel.TotalPages);
        }
    }
}
