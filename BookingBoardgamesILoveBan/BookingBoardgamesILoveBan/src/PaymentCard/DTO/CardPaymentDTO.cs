using System;

namespace BookingBoardgamesILoveBan.Src.PaymentCard.DataTransferObjects
{
    public class CardPaymentDataTransferObject
    {
        public int TransactionIdentifier { get; set; }
        public int RequestIdentifier { get; set; }
        public int ClientIdentifier { get; set; }
        public int OwnerIdentifier { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateOfTransaction { get; set; }
        public string PaymentMethod { get; set; }

        public CardPaymentDataTransferObject(
            int transactionIdentifier,
            int requestIdentifier,
            int clientIdentifier,
            int ownerIdentifier,
            decimal amount,
            DateTime dateOfTransaction,
            string paymentMethod)
        {
            TransactionIdentifier = transactionIdentifier;
            RequestIdentifier = requestIdentifier;
            ClientIdentifier = clientIdentifier;
            OwnerIdentifier = ownerIdentifier;
            Amount = amount;
            DateOfTransaction = dateOfTransaction;
            PaymentMethod = paymentMethod;
        }
    }
}