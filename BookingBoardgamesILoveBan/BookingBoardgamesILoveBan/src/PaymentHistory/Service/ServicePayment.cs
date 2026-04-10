using System;
using System.Collections.Generic;
using System.Linq;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.Src.PaymentHistory.DTO;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Enums;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Model;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Repository;
using BookingBoardgamesILoveBan.Src.Receipt.Service;

namespace BookingBoardgamesILoveBan.Src.PaymentHistory.Service
{
    /// <summary>
    /// Service responsible for business logic, mapping, computing totals, and filtering transactions for the Payment History view.
    /// </summary>
    public class ServicePayment : IServicePayment
    {
        private readonly IRepositoryPayment repository;
        private readonly ReceiptService receiptservice;

        /// <summary>
        /// Initializes a new instance of the ServiceTransactions class.
        /// </summary>
        /// <param name="repository">The transactions repository providing data access.</param>
        /// <param name="receiptService">Service to handle generating and opening receipts.</param>
        public ServicePayment(IRepositoryPayment paymentRepository, ReceiptService receiptService)
        {
            repository = paymentRepository;
            receiptservice = receiptService;
        }

        /// <summary>
        /// Retrieves all transactions without any filtering, mapped to DTOs for UI display.
        /// </summary>
        /// <returns>A list of all mapped TransactionDto objects.</returns>
        public List<PaymentDto> GetAllPaymentsForUI()
        {
            var payments = repository.GetAllPayments();
            return MapToDto(payments);
        }

        /// <summary>
        /// Retrieves transactions mapped to DTOs, filtered or sorted by the given criteria and payment method.
        /// </summary>
        /// <param name="filter">The chosen filter or sort type (e.g. Last3Months, Newest, AlphabeticalAsc).</param>
        /// <param name="paymentMethod">The chosen payment method filter.</param>
        /// <param name="searchQuery">Text used to search by Product Name.</param>
        /// <returns>A filtered/sorted list of mapped TransactionDto objects.</returns>
        public PagedResult<PaymentDto> GetFilteredPayments(FilterType filter, PaymentMethod paymentMethod = PaymentMethod.ALL, string searchQuery = "", int pageNumber = 1, int pageSize = 10)
        {
            var payments = repository.GetAllPayments().AsEnumerable();

            if (paymentMethod != PaymentMethod.ALL)
            {
                string pMethodString = paymentMethod.ToString().ToLower(); // "cash" or "card"
                payments = payments.Where(t => t.PaymentMethod?.ToLower() == pMethodString);
            }

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                payments = payments.Where(t =>
                {
                    string gName = t.GameName ?? string.Empty;
                    return gName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase);
                });
            }

            DateTime now = DateTime.Now;

            switch (filter)
            {
                case FilterType.AlphabeticalAsc:
                    payments = payments.OrderBy(t => t.GameName ?? "z");
                    break;
                case FilterType.AlphabeticalDesc:
                    payments = payments.OrderByDescending(t => t.GameName ?? "a");
                    break;
                case FilterType.Newest:
                    payments = payments.OrderByDescending(t => t.DateOfTransaction ?? DateTime.MinValue);
                    break;
                case FilterType.Oldest:
                    payments = payments.OrderBy(t => t.DateOfTransaction ?? DateTime.MinValue);
                    break;
                case FilterType.Last3Months:
                    payments = payments.Where(t => t.DateOfTransaction.HasValue && t.DateOfTransaction.Value >= now.AddMonths(-3));
                    break;
                case FilterType.Last6Months:
                    payments = payments.Where(t => t.DateOfTransaction.HasValue && t.DateOfTransaction.Value >= now.AddMonths(-6));
                    break;
                case FilterType.Last9Months:
                    payments = payments.Where(t => t.DateOfTransaction.HasValue && t.DateOfTransaction.Value >= now.AddMonths(-9));
                    break;
                case FilterType.AllTime:
                default:
                    // Return all as is
                    break;
            }

            int totalCount = payments.Count();
            var pagedSource = payments
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new PagedResult<PaymentDto>
            {
                Items = MapToDto(pagedSource),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Computes the sum total amount from a given sequence of displayed transactions.
        /// </summary>
        /// <param name="displayedPayments">The sequence to sum.</param>
        /// <returns>The total raw sum.</returns>
        public decimal CalculateTotalAmount(IEnumerable<PaymentDto> displayedPayments)
        {
            if (displayedPayments == null)
            {
                return 0;
            }
            return displayedPayments.Sum(t => t.Amount);
        }

        /// <summary>
        /// Retrieves the full file path of the receipt, ensuring it exists.
        /// </summary>
        /// <param name="paymentId">The ID of the transaction.</param>
        /// <returns>The string file path to the Receipt PDF.</returns>
        public string GetReceiptDocumentPath(int paymentId)
        {
            PaymentCommon.Model.Payment payment = repository.GetPaymentById(paymentId);

            if (string.IsNullOrEmpty(payment.FilePath))
            {
                payment.FilePath = receiptservice.GenerateReceiptRelativePath(payment.RequestId);
            }
            else if (!payment.FilePath.Contains("\\"))
            {
                payment.FilePath = "receipts\\" + payment.FilePath;
            }

            return receiptservice.GetReceiptDocument(payment);
        }

        /// <summary>
        /// Maps domain Transaction models into UI-friendly TransactionDto objects.
        /// </summary>
        /// <param name="payments">The collection of transactions to map.</param>
        /// <returns>A mapped list of TransactionDto objects.</returns>
        private List<PaymentDto> MapToDto(IEnumerable<HistoryPayment> payments)
        {
            return payments.Select(t =>
            {
                return new PaymentDto
                {
                    Id = t.Tid,
                    DateText = t.DateOfTransaction?.ToString("d") ?? "Pending",
                    ProductName = !string.IsNullOrWhiteSpace(t.GameName) ? t.GameName : "Unknown Game",
                    ReceiverName = !string.IsNullOrWhiteSpace(t.OwnerName) ? t.OwnerName : "Unknown Owner",
                    Amount = t.Amount,
                    PaymentMethod = t.PaymentMethod,
                    FilePath = t.FilePath
                };
            }).ToList();
        }
    }
}
