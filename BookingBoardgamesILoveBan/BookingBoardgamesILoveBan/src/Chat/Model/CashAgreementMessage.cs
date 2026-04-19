using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.Enum;

namespace BookingBoardgamesILoveBan.Src.Model
{
    public class CashAgreementMessage : Message
    {
        public int CashPaymentId { get; set; }
        public bool IsCashAgreementResolved { get; set; }
        public bool IsCashAgreementAcceptedByBuyer { get; set; }
        public bool IsCashAgreementAcceptedBySeller { get; set; }

        public CashAgreementMessage(int id, int conversationId, int sellerId, int buyerId, int paymentId, DateTime sentAt, string content,
            bool isResolved = false, bool isAcceptedByBuyer = false, bool isAcceptedBySeller = false)
            : base(id, conversationId, sellerId, buyerId, sentAt, content, MessageType.MessageCashAgreement)
        {
            IsCashAgreementResolved = isResolved;
            IsCashAgreementAcceptedByBuyer = isAcceptedByBuyer;
            IsCashAgreementAcceptedBySeller = isAcceptedBySeller;
            CashPaymentId = paymentId;
        }
    }
}
