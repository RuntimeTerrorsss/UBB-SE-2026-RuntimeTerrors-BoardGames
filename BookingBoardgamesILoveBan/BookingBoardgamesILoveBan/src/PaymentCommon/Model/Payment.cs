using System;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Constants;

namespace BookingBoardgamesILoveBan.Src.PaymentCommon.Model
{
	public class Payment
	{
		public int Tid { get; set; }
		public int TransactionId
		{
			get => this.Tid;
			set => this.Tid = value;
		}

		public int RequestId { get; set; }
		public int ClientId { get; set; }
		public int OwnerId { get; set; }
		public decimal Amount { get; set; }
		public string PaymentMethod { get; set; }
		public DateTime? DateOfTransaction { get; set; }
		public DateTime? DateConfirmedBuyer { get; set; }
		public DateTime? DateConfirmedSeller { get; set; }

		public int State;
		public string? FilePath { get; set; }

		public Payment(int paymentId, int requestId, int clientId, int ownerId, decimal amount, string paymentMethod)
		{
			this.Tid = paymentId;
			this.RequestId = requestId;
			this.ClientId = clientId;
			this.OwnerId = ownerId;
			this.Amount = amount;
			this.PaymentMethod = paymentMethod;

			this.DateOfTransaction = DateTime.Now;
			this.DateConfirmedBuyer = null;
			this.DateConfirmedSeller = null;

			this.State = PaymentConstrants.StatePending;

			this.FilePath = null;
		}
        public Payment()
		{
		}
    }
}
