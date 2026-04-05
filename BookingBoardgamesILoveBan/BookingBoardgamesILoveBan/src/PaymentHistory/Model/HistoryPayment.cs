using System;

namespace BookingBoardgamesILoveBan.src.PaymentHistory.Model
{
    public class HistoryPayment : BookingBoardgamesILoveBan.src.PaymentCommon.Model.Payment
    {
        public string GameName { get; set; }
        public string OwnerName { get; set; }

        public HistoryPayment(int id, int requestId, int renterId, int ownerId, string method, decimal amount) 
            : base(id, requestId, renterId, ownerId, amount, method)
        {
        }
    }
}
