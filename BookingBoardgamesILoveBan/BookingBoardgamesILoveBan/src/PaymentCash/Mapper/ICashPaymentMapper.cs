using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.Src.PaymentCash.Model;

namespace BookingBoardgamesILoveBan.Src.PaymentCash.Mapper
{
	public interface ICashPaymentMapper
	{
		public Payment ToEntity(CashPaymentDto paymentDto);
		public CashPaymentDto ToDto(Payment payment);
	}
}
