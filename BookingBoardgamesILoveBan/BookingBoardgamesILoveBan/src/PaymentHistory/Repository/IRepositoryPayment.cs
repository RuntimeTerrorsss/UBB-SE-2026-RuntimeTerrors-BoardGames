using System.Collections.Generic;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Model;

namespace BookingBoardgamesILoveBan.Src.PaymentHistory.Repository
{
    public interface IRepositoryPayment
    {
        IReadOnlyList<HistoryPayment> GetAllPayments();
        HistoryPayment GetPaymentById(int id);
    }
}
