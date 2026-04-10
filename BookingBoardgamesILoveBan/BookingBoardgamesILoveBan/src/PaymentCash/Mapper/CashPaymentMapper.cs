using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.Src.PaymentCash.Model;

namespace BookingBoardgamesILoveBan.Src.PaymentCash.Mapper
{
	public class CashPaymentMapper : ICashPaymentMapper
	{
		public PaymentCommon.Model.Payment ToEntity(CashPaymentDto paymentDto)
		{
			return new PaymentCommon.Model.Payment(paymentDto.Id, paymentDto.Requestd, paymentDto.ClientId, paymentDto.OwnerId,  paymentDto.Amount, "CASH");
		}

		public CashPaymentDto ToDto(PaymentCommon.Model.Payment payment)
		{
			return new CashPaymentDto(payment.Tid, payment.RequestId, payment.ClientId, payment.OwnerId, payment.Amount);
		}
	}
}
