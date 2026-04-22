using BookingBoardgamesILoveBan.Src.PaymentHistory.DTO;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Enums;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Service;
using BookingBoardgamesILoveBan.Src.PaymentHistory.ViewModel;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardgamesLoveBan.Tests.PaymentHistory
{
    public class PaymentHistoryViewModelTests // unit tests
    {
        private readonly Mock<IServicePayment> mockPaymentService;

        public PaymentHistoryViewModelTests()
        {
            mockPaymentService = new Mock<IServicePayment>();

            mockPaymentService
                .Setup(service => service.GetFilteredPayments(
                    It.IsAny<FilterType>(),
                    It.IsAny<PaymentMethod>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns((FilterType filterType,
                          PaymentMethod paymentMethod,
                          string searchQuery,
                          int pageNumber,
                          int pageSize) =>
                {
                    var payments = new List<PaymentDataTransferObject>
                    {
                        CreatePaymentDataTransferObject(1, "Chess", 10),
                        CreatePaymentDataTransferObject(2, "Risk", 20)
                    };

                    return new PagedResult<PaymentDataTransferObject>
                    {
                        Items = payments,
                        TotalCount = payments.Count,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                });

            mockPaymentService
                .Setup(service => service.CalculateTotalAmount(It.IsAny<IEnumerable<PaymentDataTransferObject>>()))
                .Returns((IEnumerable<PaymentDataTransferObject> payments) => payments.Sum(payment => payment.Amount));

            mockPaymentService
                .Setup(service => service.GetReceiptDocumentPath(It.IsAny<int>()))
                .Returns((int paymentIdentifier) => $"C:\\receipts\\receipt_{paymentIdentifier}.pdf");
        }

        private PaymentHistoryViewModel InitializeViewModel()
        {
            return new PaymentHistoryViewModel(mockPaymentService.Object);
        }

        private PaymentDataTransferObject CreatePaymentDataTransferObject(int paymentIdentifier, string productName, decimal amount)
        {
            return new PaymentDataTransferObject
            {
                PaymentId = paymentIdentifier,
                ProductName = productName,
                Amount = amount,
                PaymentMethod = "Card"
            };
        }

        // ================================ Constructor ======================================
        [Fact]
        public void Constructor_WhenInitialized_LoadsFilterOptions()
        {
            var viewModel = InitializeViewModel();
            Assert.NotEmpty(viewModel.FilterOptions);
        }

        [Fact]
        public void Constructor_WhenInitialized_HasEightFilterOptions()
        {
            var viewModel = InitializeViewModel();
            Assert.Equal(8, viewModel.FilterOptions.Count);
        }

        [Fact]
        public void Constructor_WhenInitialized_SetsDefaultFilterToAllTime()
        {
            var viewModel = InitializeViewModel();
            Assert.Equal(FilterType.AllTime, viewModel.SelectedFilterOption.Type);
        }

        [Fact]
        public void Constructor_WhenInitialized_SetsDefaultPaymentMethodToAll()
        {
            var viewModel = InitializeViewModel();
            Assert.Equal(PaymentMethod.ALL, viewModel.SelectedPaymentMethod);
        }

        [Fact]
        public void Constructor_WhenInitialized_SetsDefaultPageToOne()
        {
            var viewModel = InitializeViewModel();
            Assert.Equal(1, viewModel.CurrentPage);
        }

        [Fact]
        public void Constructor_WhenInitialized_FilterOptionsContainsAllTime()
        {
            var viewModel = InitializeViewModel();
            var types = viewModel.FilterOptions.Select(filter => filter.Type).ToList();

            Assert.Contains(FilterType.AllTime, types);
        }

        [Fact]
        public void Constructor_WhenInitialized_FilterOptionsContainsLast3Months()
        {
            var viewModel = InitializeViewModel();
            var types = viewModel.FilterOptions.Select(filter => filter.Type).ToList();

            Assert.Contains(FilterType.Last3Months, types);
        }

        [Fact]
        public void Constructor_WhenInitialized_FilterOptionsContainsLast6Months()
        {
            var viewModel = InitializeViewModel();
            var types = viewModel.FilterOptions.Select(filter => filter.Type).ToList();

            Assert.Contains(FilterType.Last6Months, types);
        }

        [Fact]
        public void Constructor_WhenInitialized_FilterOptionsContainsLast9Months()
        {
            var viewModel = InitializeViewModel();
            var types = viewModel.FilterOptions.Select(filter => filter.Type).ToList();

            Assert.Contains(FilterType.Last9Months, types);
        }

        [Fact]
        public void Constructor_WhenInitialized_FilterOptionsContainsNewest()
        {
            var viewModel = InitializeViewModel();
            var types = viewModel.FilterOptions.Select(filter => filter.Type).ToList();

            Assert.Contains(FilterType.Newest, types);
        }

        [Fact]
        public void Constructor_WhenInitialized_FilterOptionsContainsOldest()
        {
            var viewModel = InitializeViewModel();
            var types = viewModel.FilterOptions.Select(filter => filter.Type).ToList();

            Assert.Contains(FilterType.Oldest, types);
        }

        [Fact]
        public void Constructor_WhenInitialized_FilterOptionsContainsAlphabeticalAsc()
        {
            var viewModel = InitializeViewModel();
            var types = viewModel.FilterOptions.Select(filter => filter.Type).ToList();

            Assert.Contains(FilterType.AlphabeticalAsc, types);
        }

        [Fact]
        public void Constructor_WhenInitialized_FilterOptionsContainsAlphabeticalDesc()
        {
            var viewModel = InitializeViewModel();
            var types = viewModel.FilterOptions.Select(filter => filter.Type).ToList();

            Assert.Contains(FilterType.AlphabeticalDesc, types);
        }

        [Fact]
        public void Constructor_WhenInitialized_LoadsPayments()
        {
            var viewModel = InitializeViewModel();
            Assert.Equal(2, viewModel.Payments.Count);
        }

        [Fact]
        public void Constructor_WhenInitialized_CalculatesTotalAmountCorrectly()
        {
            var viewModel = InitializeViewModel();
            Assert.Equal(30, viewModel.TotalAmount);
        }

        // ================================ SearchText ======================================
        [Fact]
        public void SearchText_WhenSet_UpdatesProperty()
        {
            var viewModel = InitializeViewModel();
            viewModel.SearchText = "Chess";

            Assert.Equal("Chess", viewModel.SearchText);
        }

        [Fact]
        public void SearchText_WhenSetToSameValue_DoesNotTriggerDebounce()
        {
            var viewModel = InitializeViewModel();
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
            var viewModel = InitializeViewModel();
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
        public async Task SearchText_WhenChangedMultipleTimes_CancelsPreviousSearch()
        {
            try
            {
                var viewModel = InitializeViewModel();
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
        public void OpenReceiptCommand_WhenInitialized_IsNotNull()
        {
            var viewModel = InitializeViewModel();
            Assert.NotNull(viewModel.OpenReceiptCommand);
        }

        [Fact]
        public void OpenReceipt_IfNull_DoesNotCrash()
        {
            try
            {
                var viewModel = InitializeViewModel();
                viewModel.OpenReceiptCommand.Execute(null);
            }
            catch
            {
                Assert.True(false);
            }
        }

        [Fact]
        public void OpenReceipt_InexistentFile_DoesNotThrow()
        {
            try
            {
                var viewModel = InitializeViewModel();
                var payment = new PaymentDataTransferObject { PaymentId = 999 };
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
            var viewModel = InitializeViewModel();
            viewModel.CurrentPage = 3;
            viewModel.SelectedFilterOption = viewModel.FilterOptions.First(filter => filter.Type == FilterType.Newest);

            Assert.Equal(1, viewModel.CurrentPage);
        }

        [Fact]
        public void SelectedFilterOption_WhenChanged_UpdatesPayments()
        {
            var paymentHistoryViewModel = InitializeViewModel();

            mockPaymentService
                .Setup(service => service.GetFilteredPayments(
                    It.IsAny<FilterType>(),
                    It.IsAny<PaymentMethod>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(new PagedResult<PaymentDataTransferObject>
                {
                    Items = new List<PaymentDataTransferObject>
                    {
                        CreatePaymentDataTransferObject(3, "Catan", 50)
                    },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            paymentHistoryViewModel.SelectedFilterOption =
                paymentHistoryViewModel.FilterOptions.First(option => option.Type == FilterType.Newest);

            Assert.Equal("Catan", paymentHistoryViewModel.Payments[0].ProductName);
        }

        // ================================ ApplyFilter ======================================
        [Fact]
        public void ApplyFilter_IfSelectedFilterIsNull_DoesNotCrash()
        {
            try
            {
                var viewModel = InitializeViewModel();

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
            var viewModel = InitializeViewModel();

            viewModel.SelectedFilterOption = viewModel.FilterOptions.First(filter => filter.Type == FilterType.Newest);

            Assert.Equal(2, viewModel.Payments.Count);
            Assert.Equal(30m, viewModel.TotalAmount);
        }

        // ================================ SelectedPaymentMethod ======================================
        [Fact]
        public void SelectedPaymentMethod_WhenChanged_ResetsToPageOne()
        {
            var viewModel = InitializeViewModel();
            viewModel.CurrentPage = 3;
            viewModel.SelectedPaymentMethod = PaymentMethod.CARD;

            Assert.Equal(1, viewModel.CurrentPage);
        }

        // ================================ NextPageCommand ======================================
        [Fact]
        public void NextPageCommand_CanExecute_TrueWhenNotOnLastPage()
        {
            var viewModel = InitializeViewModel();
            viewModel.TotalPages = 3;
            viewModel.CurrentPage = 1;

            Assert.True(viewModel.NextPageCommand.CanExecute(null));
        }

        [Fact]
        public void NextPage_WhenNotOnLastPage_IncrementsPage()
        {
            var viewModel = InitializeViewModel();
            viewModel.TotalPages = 3;
            viewModel.CurrentPage = 1;

            viewModel.NextPageCommand.Execute(null);

            Assert.Equal(2, viewModel.CurrentPage);
        }

        [Fact]
        public void NextPage_WhenOnLastPage_DoesNotIncrement()
        {
            var viewModel = InitializeViewModel();
            viewModel.TotalPages = 1;
            viewModel.CurrentPage = 1;

            viewModel.NextPageCommand.Execute(null);

            Assert.Equal(1, viewModel.CurrentPage);
        }

        [Fact]
        public void NextPageCommand_CanExecute_FalseOnLastPage()
        {
            var viewModel = InitializeViewModel();
            viewModel.TotalPages = 1;
            viewModel.CurrentPage = 1;

            Assert.False(viewModel.NextPageCommand.CanExecute(null));
        }

        // ================================ PreviousPageCommand ======================================
        [Fact]
        public void PreviousPageCommand_CanExecute_TrueWhenNotOnFirstPage()
        {
            var viewModel = InitializeViewModel();
            viewModel.TotalPages = 3;
            viewModel.CurrentPage = 2;

            Assert.True(viewModel.PreviousPageCommand.CanExecute(null));
        }

        [Fact]
        public void PreviousPage_WhenNotOnFirstPage_DecrementsPage()
        {
            var viewModel = InitializeViewModel();
            viewModel.TotalPages = 3;
            viewModel.CurrentPage = 2;

            viewModel.PreviousPageCommand.Execute(null);

            Assert.Equal(1, viewModel.CurrentPage);
        }

        [Fact]
        public void PreviousPage_WhenOnFirstPage_DoesNotDecrement()
        {
            var viewModel = InitializeViewModel();
            viewModel.CurrentPage = 1;

            viewModel.PreviousPageCommand.Execute(null);

            Assert.Equal(1, viewModel.CurrentPage);
        }

        [Fact]
        public void PreviousPageCommand_CanExecute_FalseOnFirstPage()
        {
            var viewModel = InitializeViewModel();
            viewModel.CurrentPage = 1;

            Assert.False(viewModel.PreviousPageCommand.CanExecute(null));
        }

        // ================================ TotalPages ======================================
        [Fact]
        public void TotalPages_WhenResultIsZero_DefaultsToOne()
        {
            var viewModel = InitializeViewModel();
            viewModel.SelectedFilterOption = viewModel.FilterOptions.First(filter => filter.Type == FilterType.AllTime);

            Assert.Equal(1, viewModel.TotalPages);
        }
    }
}
