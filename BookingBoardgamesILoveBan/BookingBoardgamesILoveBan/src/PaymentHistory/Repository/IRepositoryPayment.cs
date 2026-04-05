using System.Collections.Generic;
using BookingBoardgamesILoveBan.src.PaymentHistory.Model;

namespace BookingBoardgamesILoveBan.src.PaymentHistory.Repository
{
    public interface IRepositoryPayment
    {
        IReadOnlyList<HistoryPayment> GetAllPayments();
        HistoryPayment GetPaymentById(int id);
    }
}
