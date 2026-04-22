using System;

namespace BookingBoardgamesILoveBan.Src.PaymentHistory.Model
{
    public class HistoryPayment : BookingBoardgamesILoveBan.Src.PaymentCommon.Model.Payment
    {
        public string GameName { get; set; }
        public string OwnerName { get; set; }

        public HistoryPayment(int paymentId, int requestId, int renterId, int ownerId, string method, decimal amount) : base(paymentId, requestId, renterId, ownerId, amount, method)
        {
        }
    }
}
