using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;

namespace BookingBoardgamesILoveBan.Src.Receipt.Service
{
	public interface IReceiptService
	{
		public string GenerateReceiptRelativePath(int rentalId);
		public string GetReceiptDocument(PaymentCommon.Model.Payment payment);
	}
}
