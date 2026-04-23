using System;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Constants;

namespace BookingBoardgamesILoveBan.Src.PaymentCommon.Model
{
	public class Payment
	{
		public int TransactionIdentifier { get; set; }

		public int RequestId { get; set; }
		public int ClientId { get; set; }
		public int OwnerId { get; set; }
		public decimal PaidAmount { get; set; }
		public string PaymentMethod { get; set; }
		public DateTime? DateOfTransaction { get; set; }
		public DateTime? DateConfirmedBuyer { get; set; }
		public DateTime? DateConfirmedSeller { get; set; }

		public int PaymentState;
		public string? ReceiptFilePath { get; set; }

		public Payment(int paymentId, int requestId, int clientId, int ownerId, decimal paidAmount, string paymentMethod)
		{
			this.TransactionIdentifier = paymentId;
			this.RequestId = requestId;
			this.ClientId = clientId;
			this.OwnerId = ownerId;
			this.PaidAmount = paidAmount;
			this.PaymentMethod = paymentMethod;

			this.DateOfTransaction = DateTime.Now;
			this.DateConfirmedBuyer = null;
			this.DateConfirmedSeller = null;

			this.PaymentState = PaymentConstrants.StatePending;

			this.ReceiptFilePath = null;
		}
        public Payment()
		{
		}
    }
}
