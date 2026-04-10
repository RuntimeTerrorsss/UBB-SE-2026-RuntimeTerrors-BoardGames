using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardgamesILoveBan.Src.PaymentCard.DTO
{
    public class CardPaymentDTO
    {
        public int Tid { get; set; }
        public int RequestId { get; set; }
        public int ClientId { get; set; }
        public int OwnerId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateOfTransaction { get; set; }
        public string PaymentMethod { get; set; }

        public CardPaymentDTO(int tid, int requestId, int clientId, int ownerId, decimal amount, DateTime dateOfTransaction, string paymentMethod)
        {
            Tid = tid;
            RequestId = requestId;
            ClientId = clientId;
            OwnerId = ownerId;
            Amount = amount;
            DateOfTransaction = dateOfTransaction;
            PaymentMethod = paymentMethod;
        }
    }
}
