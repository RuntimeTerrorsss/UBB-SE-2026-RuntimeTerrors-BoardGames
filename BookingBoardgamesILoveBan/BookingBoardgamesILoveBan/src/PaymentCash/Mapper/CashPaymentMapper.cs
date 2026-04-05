using BookingBoardgamesILoveBan.src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.src.PaymentCash.Model;


namespace BookingBoardgamesILoveBan.src.PaymentCash.Mapper
{
	public class CashPaymentMapper : ICashPaymentMapper
	{
		public PaymentCommon.Model.Payment toEntity(CashPaymentDto paymentDto) {
			return new PaymentCommon.Model.Payment(paymentDto.Id, paymentDto.Requestd, paymentDto.ClientId, paymentDto.OwnerId,  paymentDto.Amount, "CASH");
		} 

		public CashPaymentDto toDto(PaymentCommon.Model.Payment payment) {
			return new CashPaymentDto(payment.tid, payment.RequestId, payment.ClientId, payment.OwnerId, payment.Amount);
		}
	}
}
