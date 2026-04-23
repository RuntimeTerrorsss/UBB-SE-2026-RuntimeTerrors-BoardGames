using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.Src.PaymentCash.Model;

namespace BookingBoardgamesILoveBan.Src.PaymentCash.Mapper
{
	public class CashPaymentMapper : ICashPaymentMapper
	{
        private const string CashPaymentMethod = "CASH";

		public Payment TurnDataTransferObjectIntoEntity(CashPaymentDataTransferObject paymentDto)
		{
			return new Payment(
                paymentDto.Id,
                paymentDto.RequestId,
                paymentDto.ClientId,
                paymentDto.OwnerId,
                paymentDto.PaidAmount,
                CashPaymentMethod);
		}

		public CashPaymentDataTransferObject TurnEntityIntoDataTransferObject(Payment payment)
		{
			return new CashPaymentDataTransferObject(payment.TransactionIdentifier, payment.RequestId, payment.ClientId, payment.OwnerId, payment.PaidAmount);
		}
	}
}
