namespace BookingBoardgamesILoveBan.Src.PaymentCash.Model
{
	public class CashPaymentDto
	{
		public int Id { get; set; }
		public int RequestId { get; set; }
		public int Requestd
		{
			get => this.RequestId;
			set => this.RequestId = value;
		}
		public int ClientId { get; set; }
		public int OwnerId { get; set; }
		public decimal Amount { get; set; }

		public CashPaymentDto(int paymentId, int requestId, int clientId, int ownerId, decimal amount)
		{
			this.Id = paymentId;
			this.RequestId = requestId;
			this.ClientId = clientId;
			this.OwnerId = ownerId;
			this.Amount = amount;
		}
	}
}
