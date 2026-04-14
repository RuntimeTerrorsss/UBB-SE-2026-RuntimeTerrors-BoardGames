using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardgamesLoveBan.Tests.PaymentCommon
{
    public class PaymentRepositoryTests // integration tests idk how
    {
        private readonly IPaymentRepository paymentRepository;

        public PaymentRepositoryTests()
        {
            paymentRepository = new PaymentRepository();
        }

        private Payment CreatePayment()
        {
            return new Payment
            {
                RequestId = 1,
                ClientId = 1,
                OwnerId = 2,
                Amount = 99.99m,
                PaymentMethod = "Card",
                State = 0,
                DateOfTransaction = DateTime.Now,
                DateConfirmedBuyer = null,
                DateConfirmedSeller = null,
                FilePath = null
            };
        }

    }
}
