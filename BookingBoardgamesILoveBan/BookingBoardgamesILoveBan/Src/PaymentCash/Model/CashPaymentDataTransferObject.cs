namespace BookingBoardgamesILoveBan.Src.PaymentCash.Model
{
	public class CashPaymentDataTransferObject
	{
		public int Id { get; set; }
		public int RequestId { get; set; }
		public int ClientId { get; set; }
		public int OwnerId { get; set; }
		public decimal PaidAmount { get; set; }

		public CashPaymentDataTransferObject(int paymentId, int requestId, int clientId, int ownerId, decimal amount)
		{
			this.Id = paymentId;
			this.RequestId = requestId;
			this.ClientId = clientId;
			this.OwnerId = ownerId;
			this.PaidAmount = amount;
		}
	}
}
