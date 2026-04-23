using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.Src.PaymentHistory.DTO;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Enums;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Model;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Repository;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Service;
using BookingBoardgamesILoveBan.Src.Receipt.Service;
using Moq;
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
        private Mock<IRepositoryPayment> repositoryPaymentMock;
        private Mock<IReceiptService> receiptServiceMock;
        private IServicePayment servicePayment;

        private void InitializeService(List<HistoryPayment> payments)
        {
            repositoryPaymentMock = new Mock<IRepositoryPayment>();
            receiptServiceMock = new Mock<IReceiptService>();

            repositoryPaymentMock
                .Setup(repository => repository.GetAllPayments())
                .Returns(payments);

            repositoryPaymentMock
                .Setup(repository => repository.GetPaymentById(It.IsAny<int>()))
                .Returns((int searchedId) => payments.FirstOrDefault(payment => payment.TransactionIdentifier == searchedId));

            receiptServiceMock
                .Setup(service => service.GenerateReceiptRelativePath(It.IsAny<int>()))
                .Returns((int paymentId) => $"receipts\\receipt_{paymentId}_test.pdf");

            receiptServiceMock
                .Setup(service => service.GetReceiptDocument(It.IsAny<Payment>()))
                .Returns((Payment payment) => $"C:\\Documents\\receipt_{payment.RequestId}.pdf");

            servicePayment = new ServicePayment(
                repositoryPaymentMock.Object,
                receiptServiceMock.Object
            );
        }

        private HistoryPayment MakePayment(int paymentId, string gameName, string ownerName, string method, decimal amount, DateTime? date = null)
        {
            var createdPayment = new HistoryPayment(paymentId, 1, 1, 2, method, amount)
            {
                GameName = gameName,
                OwnerName = ownerName,
                DateOfTransaction = date ?? DateTime.Now
            };
            return createdPayment;
        }

        // ================================ GetAllPaymentsForUI ======================================
        [Fact]
        public void GetAllPaymentsForUI_EmptyRepository_ReturnsEmptyList()
        {
            InitializeService(new List<HistoryPayment>());
            var result = servicePayment.GetAllPaymentsForUI();

            Assert.Empty(result);
        }

        [Fact]
        public void GetAllPaymentsForUI_NonEmptyRepository_ReturnsAllPayments()
        {
            InitializeService(new List<HistoryPayment> { MakePayment(1, "Game1", "Name1", "Card", 10), MakePayment(2, "Game2", "Name2", "Cash", 20) });
            var returnedPayments = servicePayment.GetAllPaymentsForUI();

            Assert.Equal(2, returnedPayments.Count);
            Assert.Equal("Game1", returnedPayments[0].ProductName);
            Assert.Equal("Game2", returnedPayments[1].ProductName);
        }

        [Fact]
        public void GetAllPaymentsForUI_NonEmptyRepository_ReturnsMatchingNames()
        {
            InitializeService(new List<HistoryPayment> { MakePayment(1, "Game1", "Name1", "Card", 10), MakePayment(2, "Game2", "Name2", "Cash", 20) });
            var returnedPayments = servicePayment.GetAllPaymentsForUI();

            Assert.Equal("Game1", returnedPayments[0].ProductName);
            Assert.Equal("Game2", returnedPayments[1].ProductName);
        }

        [Fact]
        public void GetAllPaymentsForUI_NullInput_UsesDefaultValues()
        {
            var payments = new List<HistoryPayment> { MakePayment(1, null, null, "Card", 10) };
            InitializeService(payments);

            var returnedPayments = servicePayment.GetAllPaymentsForUI();

            Assert.Equal("Unknown Game", returnedPayments[0].ProductName);
            Assert.Equal("Unknown Owner", returnedPayments[0].ReceiverName);
        }

        // ================================ CalculateTotalAmount ======================================
        [Fact]
        public void CalculateTotalAmount_NullInput_ReturnsZero()
        {
            InitializeService(new List<HistoryPayment>());
            var totalAmount = servicePayment.CalculateTotalAmount(null);

            Assert.Equal(0, totalAmount);
        }

        [Fact]
        public void CalculateTotalAmount_EmptyList_ReturnsZero()
        {
            InitializeService(new List<HistoryPayment>());
            var totalAmount = servicePayment.CalculateTotalAmount(new List<PaymentDataTransferObject>());

            Assert.Equal(0, totalAmount);
        }

        [Fact]
        public void CalculateTotalAmount_NonEmptyList_SumsAmountsCorrectly()
        {
            InitializeService(new List<HistoryPayment>());
            var payments = new List<PaymentDataTransferObject>
                {
                    new PaymentDataTransferObject { Amount = 10.50m },
                    new PaymentDataTransferObject { Amount = 20.00m },
                    new PaymentDataTransferObject { Amount = 5.25m }
                };
            var totalAmount = servicePayment.CalculateTotalAmount(payments);

            Assert.Equal(35.75m, totalAmount);
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

            var filteredPayments = servicePayment.GetFilteredPayments(FilterType.AllTime, PaymentMethod.CARD);

            Assert.All(filteredPayments.Items, payment => Assert.Equal("Card", payment.PaymentMethod));
            Assert.Equal(2, filteredPayments.TotalCount);
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

            var filteredPayments = servicePayment.GetFilteredPayments(FilterType.AllTime, PaymentMethod.CASH);

            Assert.Single(filteredPayments.Items);
            Assert.Equal("Cash", filteredPayments.Items.ElementAt(0).PaymentMethod);
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

            var filteredPayments = servicePayment.GetFilteredPayments(FilterType.AllTime, searchQuery: "chess");

            Assert.Equal(2, filteredPayments.TotalCount);
        }

        [Fact]
        public void GetFilteredPayments_SearchNoMatch_ReturnsEmpty()
        {
            var payments = new List<HistoryPayment>
                {
                    MakePayment(1, "Chess", "Alice", "Card", 10)
                };
            InitializeService(payments);
            var filteredPayments = servicePayment.GetFilteredPayments(FilterType.AllTime, searchQuery: "monopoly");

            Assert.Empty(filteredPayments.Items);
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

            var filteredPayments = servicePayment.GetFilteredPayments(FilterType.AlphabeticalAsc);

            Assert.Equal("Catan", filteredPayments.Items.ElementAt(0).ProductName);
            Assert.Equal("Chess", filteredPayments.Items.ElementAt(1).ProductName);
            Assert.Equal("Risk", filteredPayments.Items.ElementAt(2).ProductName);
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

            var filteredPayments = servicePayment.GetFilteredPayments(FilterType.AlphabeticalDesc);

            Assert.Equal("Risk", filteredPayments.Items.ElementAt(0).ProductName);
            Assert.Equal("Chess", filteredPayments.Items.ElementAt(1).ProductName);
            Assert.Equal("Catan", filteredPayments.Items.ElementAt(2).ProductName);
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

            var filteredPayments = servicePayment.GetFilteredPayments(FilterType.Newest);

            Assert.Equal("Risk", filteredPayments.Items.ElementAt(0).ProductName);
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

            var filteredPayments = servicePayment.GetFilteredPayments(FilterType.Oldest);

            Assert.Equal("Risk", filteredPayments.Items.ElementAt(0).ProductName);
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

            var filteredPayments = servicePayment.GetFilteredPayments(FilterType.Last3Months);

            Assert.Single(filteredPayments.Items);
            Assert.Equal("Chess", filteredPayments.Items.ElementAt(0).ProductName);
        }

        [Fact]
        public void GetFilteredPayments_Last6Months_ExcludesOlderPayments()
        {
            var payments = new List<HistoryPayment>
                {
                    MakePayment(1, "Chess", "Alice", "Card", 10, DateTime.Now.AddMonths(-2)),
                    MakePayment(2, "Risk", "Bob", "Card", 20, DateTime.Now.AddMonths(-7))
                };
            InitializeService(payments);

            var filteredPayments = servicePayment.GetFilteredPayments(FilterType.Last6Months);

            Assert.Single(filteredPayments.Items);
            Assert.Equal("Chess", filteredPayments.Items.ElementAt(0).ProductName);
        }

        [Fact]
        public void GetFilteredPayments_Last9Months_ExcludesOlderPayments()
        {
            var payments = new List<HistoryPayment>
                {
                    MakePayment(1, "Chess", "Alice", "Card", 10, DateTime.Now.AddMonths(-2)),
                    MakePayment(2, "Risk", "Bob", "Card", 20, DateTime.Now.AddMonths(-10))
                };
            InitializeService(payments);

            var filteredPayments = servicePayment.GetFilteredPayments(FilterType.Last9Months);

            Assert.Single(filteredPayments.Items);
            Assert.Equal("Chess", filteredPayments.Items.ElementAt(0).ProductName);
        }

        [Fact]
        public void GetFilteredPayments_Pagination_ReturnsCorrectPage()
        {
            var payments = Enumerable.Range(1, 25)
                .Select(i => MakePayment(i, $"Game{i}", "Alice", "Card", i * 10))
                .ToList();
            InitializeService(payments);

            var filteredPayments = servicePayment.GetFilteredPayments(FilterType.AllTime, pageNumber: 2, pageSize: 10);

            Assert.Equal(10, filteredPayments.Items.Count());
            Assert.Equal(2, filteredPayments.PageNumber);
        }

        [Fact]
        public void GetFilteredPayments_Pagination_LastPageHasRemainingItems()
        {
            var payments = Enumerable.Range(1, 25)
                .Select(i => MakePayment(i, $"Game{i}", "Alice", "Card", i * 10))
                .ToList();
            InitializeService(payments);

            var filteredPayments = servicePayment.GetFilteredPayments(FilterType.AllTime, pageNumber: 3, pageSize: 10);

            Assert.Equal(5, filteredPayments.Items.Count());
            Assert.Equal(3, filteredPayments.PageNumber);
        }

        // ================================ GetReceiptDocumentPath ======================================
        [Fact]
        public void GetReceiptDocumentPath_NullFilePath_GeneratesNewPath()
        {
            var payment = new HistoryPayment(1, 1, 1, 2, "Card", 50) { ReceiptFilePath = null };
            var payments = new List<HistoryPayment> { payment };
            InitializeService(payments);

            var documentPath = servicePayment.GetReceiptDocumentPath(1);
            Assert.NotNull(documentPath);
            Assert.NotEmpty(documentPath);
        }

        [Fact]
        public void GetReceiptDocumentPath_FilePathWithoutBackslashes_AddsBackslashes()
        {
            var payment = new HistoryPayment(1, 1, 1, 2, "Card", 50) { ReceiptFilePath = "receipt_1_test.pdf" };
            var payments = new List<HistoryPayment> { payment };
            InitializeService(payments);

            var documentPath = servicePayment.GetReceiptDocumentPath(1);
            Assert.NotNull(documentPath);
        }

        [Fact]
        public void GetReceiptDocumentPath_FilePathWithBackslashes_ReturnsDocument()
        {
            var payment = new HistoryPayment(1, 1, 1, 2, "Card", 50) { ReceiptFilePath = "receipts\\receipt_1_test.pdf" };
            var payments = new List<HistoryPayment> { payment };
            InitializeService(payments);

            var documentPath = servicePayment.GetReceiptDocumentPath(1);
            Assert.NotNull(documentPath);
        }
    }
}
