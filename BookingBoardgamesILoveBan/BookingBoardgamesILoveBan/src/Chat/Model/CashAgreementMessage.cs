using BookingBoardgamesILoveBan.src.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardgamesILoveBan.src.Model
{
    public class CashAgreementMessage : Message
    {
        public int PaymentId { get; set; }
        public bool IsResolved { get; set; }
        public bool IsAcceptedByBuyer { get; set; }
        public bool IsAcceptedBySeller { get; set; }

        public CashAgreementMessage(int id, int conversationId, int sellerId, int buyerId, int paymentId,DateTime sentAt, string content,
            bool isResolved = false, bool isAcceptedByBuyer = false, bool isAcceptedBySeller = false)
            : base(id, conversationId, sellerId, buyerId, sentAt, content, MessageType.CashAgreement)
        {
            IsResolved = isResolved;
            IsAcceptedByBuyer = isAcceptedByBuyer;
            IsAcceptedBySeller = isAcceptedBySeller;
            PaymentId = paymentId;
        }
    }
}
