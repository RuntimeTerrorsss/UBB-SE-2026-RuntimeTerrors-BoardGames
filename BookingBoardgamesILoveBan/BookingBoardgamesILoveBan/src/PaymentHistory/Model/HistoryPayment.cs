using System;

namespace BookingBoardgamesILoveBan.Src.PaymentHistory.Model
{
    public class HistoryPayment : BookingBoardgamesILoveBan.Src.PaymentCommon.Model.Payment
    {
        public string GameName { get; set; }
        public string OwnerName { get; set; }

        public HistoryPayment(int id, int requestId, int renterId, int ownerId, string method, decimal amount) : base(id, requestId, renterId, ownerId, amount, method)
        {
        }
    }
}
