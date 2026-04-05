using System;

namespace BookingBoardgamesILoveBan.src.PaymentHistory.DTO
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public string DateText { get; set; }
        public string ProductName { get; set; }
        public string ReceiverName { get; set; }

        /// <summary>
        /// Numeric amount used strictly for service-level total calculations.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Formatted amount string for display.
        /// </summary>
        public string AmountText => $"{Amount:C}";

        public string PaymentMethod { get; set; }
        public string FilePath { get; set; }
    }
}
