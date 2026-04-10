using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.Src.PaymentCash.Model;

namespace BookingBoardgamesILoveBan.Src.PaymentCash.Mapper
{
	public interface ICashPaymentMapper
	{
		public PaymentCommon.Model.Payment ToEntity(CashPaymentDto paymentDto);
		public CashPaymentDto ToDto(PaymentCommon.Model.Payment payment);
	}
}
