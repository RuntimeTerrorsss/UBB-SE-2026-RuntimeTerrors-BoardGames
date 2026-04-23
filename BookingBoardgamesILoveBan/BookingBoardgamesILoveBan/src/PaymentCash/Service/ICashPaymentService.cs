using BookingBoardgamesILoveBan.Src.PaymentCash.Model;

namespace BookingBoardgamesILoveBan.Src.PaymentCash.Service
{
	public interface ICashPaymentService
	{
		public int AddCashPayment(CashPaymentDataTransferObject paymentDto);
		public CashPaymentDataTransferObject GetCashPayment(int paymentId);
		public void ConfirmDelivery(int paymentId);
		public void ConfirmPayment(int paymentId);
		public bool IsAllConfirmed(int paymentId);
		public bool IsDeliveryConfirmed(int paymentId);
		public bool IsPaymentConfirmed(int paymentId);
	}
}
