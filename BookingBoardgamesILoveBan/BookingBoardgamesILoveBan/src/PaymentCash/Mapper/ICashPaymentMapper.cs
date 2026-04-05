using BookingBoardgamesILoveBan.src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.src.PaymentCash.Model;


namespace BookingBoardgamesILoveBan.src.PaymentCash.Mapper
{
	public interface ICashPaymentMapper
	{
		public PaymentCommon.Model.Payment toEntity(CashPaymentDto paymentDto);
		public CashPaymentDto toDto(PaymentCommon.Model.Payment payment);
	}
}
