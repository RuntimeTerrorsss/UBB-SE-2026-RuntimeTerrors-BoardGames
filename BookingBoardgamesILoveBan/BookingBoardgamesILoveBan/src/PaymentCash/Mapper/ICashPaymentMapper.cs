using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.Src.PaymentCash.Model;

namespace BookingBoardgamesILoveBan.Src.PaymentCash.Mapper
{
	public interface ICashPaymentMapper
	{
		public Payment TurnDataTransferObjectIntoEntity(CashPaymentDataTransferObject paymentDto);
		public CashPaymentDataTransferObject TurnEntityIntoDataTransferObject(Payment payment);
	}
}
