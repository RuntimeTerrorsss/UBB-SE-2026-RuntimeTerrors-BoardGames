using BookingBoardgamesILoveBan.src.PaymentCommon.Model;


namespace BookingBoardgamesILoveBan.src.Receipt.Service
{
	public interface IReceiptService
	{
		public string GenerateReceiptRelativePath(int rentalId);
		public string GetReceiptDocument(PaymentCommon.Model.Payment payment);
	}
}
