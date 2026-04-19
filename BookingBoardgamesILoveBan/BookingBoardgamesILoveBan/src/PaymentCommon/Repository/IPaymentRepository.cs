using System.Collections.Generic;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;

namespace BookingBoardgamesILoveBan.Src.PaymentCommon.Repository
{
	public interface IPaymentRepository
	{
		public IReadOnlyList<Model.Payment> GetAll();
		public Model.Payment GetById(int paymentId);
		public int AddPayment(Model.Payment payment);
		public bool DeletePayment(Model.Payment payment);
		public Model.Payment UpdatePayment(Model.Payment payment);
	}
}
