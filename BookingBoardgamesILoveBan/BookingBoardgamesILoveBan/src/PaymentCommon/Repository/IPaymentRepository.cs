using System.Collections.Generic;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;

namespace BookingBoardgamesILoveBan.Src.PaymentCommon.Repository
{
	public interface IPaymentRepository
	{
		public IReadOnlyList<Model.Payment> GetAll();
		public Model.Payment GetById(int tid);
		public int AddPayment(Model.Payment transaction);
		public bool DeletePayment(Model.Payment transaction);
		public Model.Payment UpdatePayment(Model.Payment transaction);
	}
}
