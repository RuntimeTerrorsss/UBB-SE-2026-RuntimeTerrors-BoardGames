using System.Collections.Generic;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;

namespace BookingBoardgamesILoveBan.Src.PaymentCommon.Repository
{
	public interface IPaymentRepository
	{
		public IReadOnlyList<Model.Payment> GetAllPayments();
		public Model.Payment GetPaymentByIdentifier(int paymentId);
		public int AddPayment(Model.Payment payment);
		public bool DeletePayment(Model.Payment payment);
		public Model.Payment UpdatePayment(Model.Payment payment);
	}
}
