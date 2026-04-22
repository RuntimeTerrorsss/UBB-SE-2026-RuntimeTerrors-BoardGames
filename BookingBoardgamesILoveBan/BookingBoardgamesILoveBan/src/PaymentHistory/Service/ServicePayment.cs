using System;
using System.Collections.Generic;
using System.Linq;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.Src.PaymentHistory.DTO;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Enums;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Model;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Repository;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Constants;
using BookingBoardgamesILoveBan.Src.Receipt.Service;

namespace BookingBoardgamesILoveBan.Src.PaymentHistory.Service
{
    /// <summary>
    /// Service responsible for business logic, mapping, computing totals, and filtering transactions for the Payment History view.
    /// </summary>
    public class ServicePayment : IServicePayment
    {
        private readonly IRepositoryPayment paymentRepository;
        private readonly IReceiptService receiptService;

        /// <summary>
        /// Initializes a new instance of the ServiceTransactions class.
        /// </summary>
        /// <param name="repository">The transactions repository providing data access.</param>
        /// <param name="receiptService">Service to handle generating and opening receipts.</param>
        public ServicePayment(IRepositoryPayment paymentRepository, IReceiptService receiptService)
        {
            this.paymentRepository = paymentRepository;
            this.receiptService = receiptService;
        }

        /// <summary>
        /// Retrieves all transactions without any filtering, mapped to DTOs for UI display.
        /// </summary>
        /// <returns>A list of all mapped TransactionDto objects.</returns>
        public List<PaymentDataTransferObject> GetAllPaymentsForUI()
        {
            var allPayments = paymentRepository.GetAllPayments();
            return MapToDataTransferObject(allPayments);
        }

        private bool IsPaymentMethodFilterApplied(PaymentMethod paymentMethod)
        {
            return paymentMethod != PaymentMethod.ALL;
        }

        private IEnumerable<HistoryPayment> FilterPaymentsByPaymentMethod(string paymentMethod, IEnumerable<HistoryPayment> payments)
        {
            return payments.Where(transaction => transaction.PaymentMethod?.ToLower() == paymentMethod);
        }

        private bool IsUserSearching(string searchQuery)
        {
            return !string.IsNullOrWhiteSpace(searchQuery);
        }

        private IEnumerable<HistoryPayment> FilterPaymentsBySearchQuery(string searchQuery, IEnumerable<HistoryPayment> payments)
        {
            return payments.Where(transaction =>
            {
                string gameName = transaction.GameName ?? string.Empty;
                return gameName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase);
            });
        }

        private IEnumerable<HistoryPayment> ApplyDateFilters(IEnumerable<HistoryPayment> payments, FilterType filter)
        {
            DateTime currentDateTime = DateTime.Now;

            switch (filter)
            {
                case FilterType.Last3Months:
                    payments = payments.Where(transaction => transaction.DateOfTransaction.HasValue && transaction.DateOfTransaction.Value >= currentDateTime.AddMonths(-3));
                    break;
                case FilterType.Last6Months:
                    payments = payments.Where(transaction => transaction.DateOfTransaction.HasValue && transaction.DateOfTransaction.Value >= currentDateTime.AddMonths(-6));
                    break;
                case FilterType.Last9Months:
                    payments = payments.Where(transaction => transaction.DateOfTransaction.HasValue && transaction.DateOfTransaction.Value >= currentDateTime.AddMonths(-9));
                    break;
                case FilterType.AllTime:
                default:
                    // Return all as is
                    break;
            }

            return payments;
        }

        private IEnumerable<HistoryPayment> ApplyFilters(IEnumerable<HistoryPayment> payments, PaymentMethod paymentMethod, string searchQuery, FilterType filter)
        {
            if (IsPaymentMethodFilterApplied(paymentMethod))
            {
                string paymentMethodString = paymentMethod.ToString().ToLower();
                payments = FilterPaymentsByPaymentMethod(paymentMethodString, payments);
            }

            if (IsUserSearching(searchQuery))
            {
                payments = FilterPaymentsBySearchQuery(searchQuery, payments);
            }

            payments = ApplyDateFilters(payments, filter);

            return payments;
        }

        private IEnumerable<HistoryPayment> ApplySorting(IEnumerable<HistoryPayment> payments, FilterType filter)
        {
            switch (filter)
            {
                case FilterType.AlphabeticalAsc:
                    payments = payments.OrderBy(transaction => transaction.GameName ?? "z");
                    break;
                case FilterType.AlphabeticalDesc:
                    payments = payments.OrderByDescending(transaction => transaction.GameName ?? "a");
                    break;
                case FilterType.Newest:
                    payments = payments.OrderByDescending(transaction => transaction.DateOfTransaction ?? DateTime.MinValue);
                    break;
                case FilterType.Oldest:
                    payments = payments.OrderBy(transaction => transaction.DateOfTransaction ?? DateTime.MinValue);
                    break;
                default:
                    // Return all as is
                    break;
            }

            return payments;
        }

        private PagedResult<PaymentDataTransferObject> GetPagedResult(IEnumerable<HistoryPayment> payments, int pageSize, int pageNumber)
        {
            int totalCount = payments.Count();

            var pagedSource = payments
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new PagedResult<PaymentDataTransferObject>
            {
                Items = MapToDataTransferObject(pagedSource),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Retrieves transactions mapped to DTOs, filtered or sorted by the given criteria and payment method.
        /// </summary>
        /// <param name="filter">The chosen filter or sort type (e.g. Last3Months, Newest, AlphabeticalAsc).</param>
        /// <param name="paymentMethod">The chosen payment method filter.</param>
        /// <param name="searchQuery">Text used to search by Product Name.</param>
        /// <returns>A filtered/sorted list of mapped TransactionDto objects.</returns>
        public PagedResult<PaymentDataTransferObject> GetFilteredPayments(FilterType filter, PaymentMethod paymentMethod = PaymentMethod.ALL, string searchQuery = "", int pageNumber = 1, int pageSize = 10)
        {
            var payments = paymentRepository.GetAllPayments().AsEnumerable();

            payments = ApplyFilters(payments, paymentMethod, searchQuery, filter);
            payments = ApplySorting(payments, filter);

            return GetPagedResult(payments, pageSize, pageNumber);
        }

        /// <summary>
        /// Computes the sum total amount from a given sequence of displayed transactions.
        /// </summary>
        /// <param name="displayedPayments">The sequence to sum.</param>
        /// <returns>The total raw sum.</returns>
        public decimal CalculateTotalAmount(IEnumerable<PaymentDataTransferObject> displayedPayments)
        {
            if (displayedPayments == null)
            {
                return PaymentHistoryConstants.NullAmountDefaultValue;
            }
            return displayedPayments.Sum(transaction => transaction.Amount);
        }

        /// <summary>
        /// Retrieves the full file path of the receipt, ensuring it exists.
        /// </summary>
        /// <param name="paymentId">The ID of the transaction.</param>
        /// <returns>The string file path to the Receipt PDF.</returns>
        public string GetReceiptDocumentPath(int paymentId)
        {
            PaymentCommon.Model.Payment foundPayment = paymentRepository.GetPaymentById(paymentId);

            if (string.IsNullOrEmpty(foundPayment.FilePath))
            {
                foundPayment.FilePath = receiptService.GenerateReceiptRelativePath(foundPayment.RequestId);
            }
            else if (!foundPayment.FilePath.Contains("\\"))
            {
                foundPayment.FilePath = "receipts\\" + foundPayment.FilePath;
            }

            return receiptService.GetReceiptDocument(foundPayment);
        }

        /// <summary>
        /// Maps domain Transaction models into UI-friendly TransactionDto objects.
        /// </summary>
        /// <param name="payments">The collection of transactions to map.</param>
        /// <returns>A mapped list of TransactionDto objects.</returns>
        private List<PaymentDataTransferObject> MapToDataTransferObject(IEnumerable<HistoryPayment> payments)
        {
            return payments.Select(transaction =>
            {
                return new PaymentDataTransferObject
                {
                    PaymentId = transaction.Tid,
                    DateText = transaction.DateOfTransaction?.ToString("d") ?? PaymentHistoryConstants.NullDateOfTransactionDefaultValue,
                    ProductName = !string.IsNullOrWhiteSpace(transaction.GameName) ? transaction.GameName : PaymentHistoryConstants.NullGameNameDefaultValue,
                    ReceiverName = !string.IsNullOrWhiteSpace(transaction.OwnerName) ? transaction.OwnerName : PaymentHistoryConstants.NullOwnerNameDefaultValue,
                    Amount = transaction.Amount,
                    PaymentMethod = transaction.PaymentMethod,
                    FilePath = transaction.FilePath
                };
            }).ToList();
        }
    }
}
