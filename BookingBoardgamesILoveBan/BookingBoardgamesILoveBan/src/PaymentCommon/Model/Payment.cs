using System;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Constants;

namespace BookingBoardgamesILoveBan.Src.PaymentCommon.Model
{
	public class Payment
	{
		public int Tid { get; set; }
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

		public Payment(int id, int requestId, int clientId, int ownerId, decimal amount, string method)
		{
			Tid = id;
			RequestId = requestId;
			ClientId = clientId;
			OwnerId = ownerId;
			Amount = amount;
			PaymentMethod = method;

			DateOfTransaction = DateTime.Now;
			DateConfirmedBuyer = null;
			DateConfirmedSeller = null;

			State = PaymentConstrants.StatePending;

			FilePath = null;
		}
        public Payment()
		{
		}
    }
}
