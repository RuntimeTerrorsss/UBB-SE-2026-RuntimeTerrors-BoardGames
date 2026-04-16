using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.Src.PaymentHistory.DTO;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Enums;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Model;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Repository;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Service;
using BookingBoardgamesILoveBan.Src.Receipt.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace BookingBoardgamesLoveBan.Tests.PaymentHistory
{
    public class ServicePaymentTests // unit tests
    {
        // ================================ fakes ======================================
        private class FakeRepositoryPayment : IRepositoryPayment
        {
            private readonly List<HistoryPayment> payments;

            public FakeRepositoryPayment(List<HistoryPayment> payments)
            {
                this.payments = payments;
            }

            public IReadOnlyList<HistoryPayment> GetAllPayments()
            {
                return payments;
            }

            public HistoryPayment GetPaymentById(int id)
            {
                foreach (var payment in payments)
                {
                    if (payment.Tid == id)
                    {
                        return payment;
                    }
                }
                throw new Exception("payment not found");
            }
        }

        private class FakeReceiptService : IReceiptService
        {
            public string GenerateReceiptRelativePath(int requestId) =>
                $"receipts\\receipt_{requestId}_test.pdf";

            public string GetReceiptDocument(Payment payment) =>
                $"C:\\Documents\\BookingBoardgames\\receipts\\receipt_{payment.RequestId}_test.pdf";
        }

        // ================================ setup ======================================

        private IRepositoryPayment repositoryPayment;
        private IReceiptService receiptService;
        private IServicePayment servicePayment;
        private void InitializeService(List<HistoryPayment> payments)
        {
            repositoryPayment = new FakeRepositoryPayment(payments);
            receiptService = new FakeReceiptService();
            servicePayment = new ServicePayment(repositoryPayment, receiptService);
        }

        private HistoryPayment MakePayment(int id, string gameName, string ownerName, string method, decimal amount, DateTime? date = null)
        {
            var p = new HistoryPayment(id, 1, 1, 2, method, amount)
            {
                GameName = gameName,
                OwnerName = ownerName,
                DateOfTransaction = date ?? DateTime.Now
            };
            return p;
        }

        // ================================ GetAllPaymentsForUI ======================================

        [Fact]
        public void GetAllPaymentsForUI_EmptyRepo_ReturnsEmptyList()
        {
            InitializeService(new List<HistoryPayment>());
            var result = servicePayment.GetAllPaymentsForUI();
            Assert.Empty(result);
        }

        [Fact]
        public void GetAllPaymentsForUI_ReturnsAllPayments()
        {
            InitializeService(new List<HistoryPayment> {
                MakePayment(1, "Game1", "Name1", "Card", 10),
                MakePayment(2, "Game2", "Name2", "Cash", 20)
            });
            var result = servicePayment.GetAllPaymentsForUI();

            Assert.Equal(2, result.Count);
            Assert.Equal("Game1", result[0].ProductName);
            Assert.Equal("Game2", result[1].ProductName);
        }

        [Fact]
        public void GetAllPaymentsForUI_NullInput()
        {
            var payments = new List<HistoryPayment>
                {
                    MakePayment(1, null, null, "Card", 10)
                };
            InitializeService(payments);

            var result = servicePayment.GetAllPaymentsForUI();

            Assert.Equal("Unknown Game", result[0].ProductName);
            Assert.Equal("Unknown Owner", result[0].ReceiverName);
        }

        // ================================ CalculateTotalAmount ======================================

        [Fact]
        public void CalculateTotalAmount_NullInput_ReturnsZero()
        {
            InitializeService(new List<HistoryPayment>());
            var result = servicePayment.CalculateTotalAmount(null);

            Assert.Equal(0, result);
        }

        [Fact]
        public void CalculateTotalAmount_EmptyList_ReturnsZero()
        {
            InitializeService(new List<HistoryPayment>());
            var result = servicePayment.CalculateTotalAmount(new List<PaymentDto>());

            Assert.Equal(0, result);
        }

        [Fact]
        public void CalculateTotalAmount_SumsAmountsCorrectly()
        {
            InitializeService(new List<HistoryPayment>());
            var dtos = new List<PaymentDto>
                {
                    new PaymentDto { Amount = 10.50m },
                    new PaymentDto { Amount = 20.00m },
                    new PaymentDto { Amount = 5.25m }
                };

            var result = servicePayment.CalculateTotalAmount(dtos);

            Assert.Equal(35.75m, result);
        }

        // ================================ GetFilteredPayments ======================================

        [Fact]
        public void GetFilteredPayments_FilterByCard_ReturnsOnlyCardPayments()
        {
            var payments = new List<HistoryPayment>
                {
                    MakePayment(1, "Chess", "Alice", "Card", 10),
                    MakePayment(2, "Risk", "Bob", "Cash", 20),
                    MakePayment(3, "Catan", "Carol", "Card", 30)
                };
            InitializeService(payments);

            var result = servicePayment.GetFilteredPayments(FilterType.AllTime, PaymentMethod.CARD);

            Assert.All(result.Items, p => Assert.Equal("Card", p.PaymentMethod));
            Assert.Equal(2, result.TotalCount);
        }

        [Fact]
        public void GetFilteredPayments_FilterByCash_ReturnsOnlyCashPayments()
        {
            var payments = new List<HistoryPayment>
                {
                    MakePayment(1, "Chess", "Alice", "Card", 10),
                    MakePayment(2, "Risk", "Bob", "Cash", 20)
                };
            InitializeService(payments);

            var result = servicePayment.GetFilteredPayments(FilterType.AllTime, PaymentMethod.CASH);

            Assert.Single(result.Items);
            Assert.Equal("Cash", result.Items.ElementAt(0).PaymentMethod);
        }

        [Fact]
        public void GetFilteredPayments_SearchByGameName_ReturnsMatchingPayments()
        {
            var payments = new List<HistoryPayment>
                {
                    MakePayment(1, "Chess", "Alice", "Card", 10),
                    MakePayment(2, "Risk", "Bob", "Cash", 20),
                    MakePayment(3, "Chess Deluxe", "Carol", "Card", 30)
                };
            InitializeService(payments);

            var result = servicePayment.GetFilteredPayments(FilterType.AllTime, searchQuery: "chess");

            Assert.Equal(2, result.TotalCount);
        }

        [Fact]
        public void GetFilteredPayments_SearchNoMatch_ReturnsEmpty()
        {
            var payments = new List<HistoryPayment>
                {
                    MakePayment(1, "Chess", "Alice", "Card", 10)
                };
            InitializeService(payments);

            var result = servicePayment.GetFilteredPayments(FilterType.AllTime, searchQuery: "monopoly");

            Assert.Empty(result.Items);
        }

        [Fact]
        public void GetFilteredPayments_AlphabeticalAsc_ReturnsSortedAZ()
        {
            var payments = new List<HistoryPayment>
                {
                    MakePayment(1, "Risk", "Alice", "Card", 10),
                    MakePayment(2, "Chess", "Bob", "Card", 20),
                    MakePayment(3, "Catan", "Carol", "Card", 30)
                };
            InitializeService(payments);

            var result = servicePayment.GetFilteredPayments(FilterType.AlphabeticalAsc);

            Assert.Equal("Catan", result.Items.ElementAt(0).ProductName);
            Assert.Equal("Risk", result.Items.ElementAt(2).ProductName);
        }

        [Fact]
        public void GetFilteredPayments_AlphabeticalDesc_ReturnsSortedZA()
        {
            var payments = new List<HistoryPayment>
                {
                    MakePayment(1, "Chess", "Alice", "Card", 10),
                    MakePayment(2, "Risk", "Bob", "Card", 20),
                    MakePayment(3, "Catan", "Carol", "Card", 30)
                };
            InitializeService(payments);

            var result = servicePayment.GetFilteredPayments(FilterType.AlphabeticalDesc);

            Assert.Equal("Risk", result.Items.ElementAt(0).ProductName);
            Assert.Equal("Catan", result.Items.ElementAt(2).ProductName);
        }

        [Fact]
        public void GetFilteredPayments_Newest_ReturnsMostRecentFirst()
        {
            var payments = new List<HistoryPayment>
                {
                    MakePayment(1, "Chess", "Alice", "Card", 10, DateTime.Now.AddDays(-10)),
                    MakePayment(2, "Risk", "Bob", "Card", 20, DateTime.Now.AddDays(-1)),
                    MakePayment(3, "Catan", "Carol", "Card", 30, DateTime.Now.AddDays(-5))
                };
            InitializeService(payments);

            var result = servicePayment.GetFilteredPayments(FilterType.Newest);

            Assert.Equal("Risk", result.Items.ElementAt(0).ProductName);
        }

        [Fact]
        public void GetFilteredPayments_Oldest_ReturnsOldestFirst()
        {
            var payments = new List<HistoryPayment>
                {
                    MakePayment(1, "Chess", "Alice", "Card", 10, DateTime.Now.AddDays(-1)),
                    MakePayment(2, "Risk", "Bob", "Card", 20, DateTime.Now.AddDays(-10)),
                    MakePayment(3, "Catan", "Carol", "Card", 30, DateTime.Now.AddDays(-5))
                };
            InitializeService(payments);

            var result = servicePayment.GetFilteredPayments(FilterType.Oldest);

            Assert.Equal("Risk", result.Items.ElementAt(0).ProductName);
        }

        [Fact]
        public void GetFilteredPayments_Last3Months_ExcludesOlderPayments()
        {
            var payments = new List<HistoryPayment>
                {
                    MakePayment(1, "Chess", "Alice", "Card", 10, DateTime.Now.AddMonths(-1)),
                    MakePayment(2, "Risk", "Bob", "Card", 20, DateTime.Now.AddMonths(-5))
                };
            InitializeService(payments);

            var result = servicePayment.GetFilteredPayments(FilterType.Last3Months);

            Assert.Single(result.Items);
            Assert.Equal("Chess", result.Items.ElementAt(0).ProductName);
        }

        [Fact]
        public void GetFilteredPayments_Pagination_ReturnsCorrectPage()
        {
            var payments = Enumerable.Range(1, 20)
                .Select(i => MakePayment(i, $"Game{i}", "Alice", "Card", i * 10))
                .ToList();
            InitializeService(payments);

            var result = servicePayment.GetFilteredPayments(FilterType.AllTime, pageNumber: 2, pageSize: 10);

            Assert.Equal(20, result.TotalCount);
            Assert.Equal(2, result.PageNumber);
        }

    }
}
