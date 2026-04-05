using BookingBoardgamesILoveBan.src.PaymentCommon.Model;
using System.Collections.Generic;


namespace BookingBoardgamesILoveBan.src.PaymentCommon.Repository
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
