using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.Src.PaymentCash.Model;

namespace BookingBoardgamesILoveBan.Src.PaymentCash.Mapper
{
	public class CashPaymentMapper : ICashPaymentMapper
	{
        private const string CashPaymentMethod = "CASH";

		public Payment ToEntity(CashPaymentDto paymentDto)
		{
			return new Payment(
                paymentDto.Id,
                paymentDto.RequestId,
                paymentDto.ClientId,
                paymentDto.OwnerId,
                paymentDto.Amount,
                CashPaymentMethod);
		}

		public CashPaymentDto ToDto(Payment payment)
		{
			return new CashPaymentDto(payment.Tid, payment.RequestId, payment.ClientId, payment.OwnerId, payment.Amount);
		}
	}
}
