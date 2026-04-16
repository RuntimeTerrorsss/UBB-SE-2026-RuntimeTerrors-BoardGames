using BookingBoardgamesILoveBan.Src.PaymentHistory.DTO;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Enums;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Service;
using BookingBoardgamesILoveBan.Src.PaymentHistory.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardgamesLoveBan.Tests.PaymentHistory
{
    public class PaymentHistoryViewModelTests // unit tests
    {
        // ================================ fakes ======================================
        private class FakeServicePayment : IServicePayment
        {
            public List<PaymentDto> PaymentsToReturn { get; set; } = new();
            public int TotalCount { get; set; } = 0;

            public List<PaymentDto> GetAllPaymentsForUI()
            {
                return PaymentsToReturn;
            }

            public PagedResult<PaymentDto> GetFilteredPayments(
                FilterType filter,
                PaymentMethod paymentMethod = PaymentMethod.ALL,
                string searchQuery = "",
                int pageNumber = 1,
                int pageSize = 10)
            {
                return new PagedResult<PaymentDto>
                {
                    Items = PaymentsToReturn,
                    TotalCount = TotalCount == 0 ? PaymentsToReturn.Count : TotalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }

            public decimal CalculateTotalAmount(IEnumerable<PaymentDto> payments) =>
                payments?.Sum(p => p.Amount) ?? 0;

            public string GetReceiptDocumentPath(int paymentId)
            {
                return $"C:\\receipts\\receipt_{paymentId}.pdf";
            }
        }

        // ================================ setup ======================================

        private readonly IServicePayment service;
        private readonly PaymentHistoryViewModel viewModel;

        public PaymentHistoryViewModelTests()
        {
            service = new FakeServicePayment();
            viewModel = new PaymentHistoryViewModel(service);
        }

        // ================================ Constructor ======================================

        [Fact]
        public void Constructor_CheckPaymentsFiltersDefaultsAndPage()
        {
            Assert.Empty(viewModel.Payments);
            Assert.Equal(0, viewModel.TotalAmount);

            Assert.NotEmpty(viewModel.FilterOptions);

            Assert.Equal(FilterType.AllTime, viewModel.SelectedFilterOption.Type);
            Assert.Equal(PaymentMethod.ALL, viewModel.SelectedPaymentMethod);

            var types = viewModel.FilterOptions.Select(f => f.Type).ToList();
            Assert.Contains(FilterType.AllTime, types);
            Assert.Contains(FilterType.Last3Months, types);
            Assert.Contains(FilterType.Last6Months, types);
            Assert.Contains(FilterType.Last9Months, types);
            Assert.Contains(FilterType.Newest, types);
            Assert.Contains(FilterType.Oldest, types);
            Assert.Contains(FilterType.AlphabeticalAsc, types);
            Assert.Contains(FilterType.AlphabeticalDesc, types);

            Assert.Equal(1, viewModel.CurrentPage);
        }

        // ================================ SelectedFilterOption ======================================

        [Fact]
        public void SelectedFilterOption_WhenChanged_ResetsToPageOne()
        {
            viewModel.CurrentPage = 3;
            viewModel.SelectedFilterOption = viewModel.FilterOptions.First(f => f.Type == FilterType.Newest);

            Assert.Equal(1, viewModel.CurrentPage);
        }

        // ================================ SelectedPaymentMethod ======================================

        [Fact]
        public void SelectedPaymentMethod_WhenChanged_ResetsToPageOne()
        {
            viewModel.CurrentPage = 3;
            viewModel.SelectedPaymentMethod = PaymentMethod.CARD;

            Assert.Equal(1, viewModel.CurrentPage);
        }

        // ================================ NextPageCommand ======================================

        [Fact]
        public void NextPageCommand_CanExecute_TrueWhenNotOnLastPage()
        {
            viewModel.TotalPages = 3;
            viewModel.CurrentPage = 1;

            Assert.True(viewModel.NextPageCommand.CanExecute(null));
        }

        [Fact]
        public void NextPage_WhenNotOnLastPage_IncrementsPage()
        {
            viewModel.TotalPages = 3;
            viewModel.CurrentPage = 1;

            viewModel.NextPageCommand.Execute(null);

            Assert.Equal(2, viewModel.CurrentPage);
        }

        [Fact]
        public void NextPageCommand_CanExecute_FalseOnLastPage()
        {
            viewModel.TotalPages = 1;
            viewModel.CurrentPage = 1;

            Assert.False(viewModel.NextPageCommand.CanExecute(null));
        }

        // ================================ PreviousPageCommand ======================================

        [Fact]
        public void PreviousPageCommand_CanExecute_TrueWhenNotOnFirstPage()
        {
            viewModel.TotalPages = 3;
            viewModel.CurrentPage = 2;

            Assert.True(viewModel.PreviousPageCommand.CanExecute(null));
        }

        [Fact]
        public void PreviousPage_WhenNotOnFirstPage_DecrementsPage()
        {
            viewModel.TotalPages = 3;
            viewModel.CurrentPage = 2;

            viewModel.PreviousPageCommand.Execute(null);

            Assert.Equal(1, viewModel.CurrentPage);
        }

        [Fact]
        public void PreviousPageCommand_CanExecute_FalseOnFirstPage()
        {
            viewModel.CurrentPage = 1;

            Assert.False(viewModel.PreviousPageCommand.CanExecute(null));
        }

        // ================================ TotalPages ======================================

        [Fact]
        public void TotalPages_WhenResultIsZero_DefaultsToOne()
        {
            viewModel.SelectedFilterOption = viewModel.FilterOptions.First(f => f.Type == FilterType.AllTime);

            Assert.Equal(1, viewModel.TotalPages);
        }
    }
}
