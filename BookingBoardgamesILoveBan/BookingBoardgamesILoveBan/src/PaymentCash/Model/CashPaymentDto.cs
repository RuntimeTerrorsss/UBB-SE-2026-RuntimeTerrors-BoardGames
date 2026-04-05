namespace BookingBoardgamesILoveBan.src.PaymentCash.Model
{
	public class CashPaymentDto
	{
		public int Id { get; set; }
		public int Requestd { get; set; }
		public int ClientId { get; set; }
		public int OwnerId { get; set; }
		public decimal Amount { get; set; }

		public CashPaymentDto(int id, int RequestId, int clientId, int ownerId, decimal amount) {
			this.Id = id;
			this.Requestd = RequestId;
			this.ClientId = clientId;
			this.OwnerId = ownerId;
			this.Amount = amount;
		}
	}
}
