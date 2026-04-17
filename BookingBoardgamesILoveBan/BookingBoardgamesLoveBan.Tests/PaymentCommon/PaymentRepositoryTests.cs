using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardgamesLoveBan.Tests.PaymentCommon
{
    public class PaymentRepositoryTests // integration tests
    {
        private readonly IPaymentRepository paymentRepository;

        public PaymentRepositoryTests()
        {
            DatabaseBootstrap.Initialize();
            paymentRepository = new PaymentRepository();
        }

        private Payment CreatePayment()
        {
            return new Payment
            {
                RequestId = 1,
                ClientId = 1,
                OwnerId = 2,
                Amount = 100,
                PaymentMethod = "Card",
                State = 0,
                DateOfTransaction = DateTime.Now,
                DateConfirmedBuyer = null,
                DateConfirmedSeller = null,
                FilePath = null
            };
        }


        // ================================ GetAll ======================================

        [Fact]
        public void GetAll_ReturnsNonNullList()
        {
            var result = paymentRepository.GetAll();
            Assert.NotNull(result);
        }


        // ================================ AddPayment ======================================

        [Fact]
        public void AddPayment_ValidPayment_ReturnsPositiveId()
        {
            var payment = CreatePayment();

            int newId = paymentRepository.AddPayment(payment);

            Assert.True(newId > 0);

            // cleannnn
            payment.Tid = newId;
            paymentRepository.DeletePayment(payment);
        }


        // ================================ GetById ======================================

        [Fact]
        public void GetById_NonExistingId_ReturnsNull()
        {
            var result = paymentRepository.GetById(-1);

            Assert.Null(result);
        }

        [Fact]
        public void GetById_ExistingPayment_ReturnsCorrectPayment()
        {
            var payment = CreatePayment();
            int newId = paymentRepository.AddPayment(payment);
            payment.Tid = newId;

            var result = paymentRepository.GetById(newId);

            Assert.NotNull(result);
            Assert.Equal(payment.Amount, result.Amount);
            Assert.Equal(payment.PaymentMethod, result.PaymentMethod);
            Assert.Equal(payment.ClientId, result.ClientId);

            // clean
            paymentRepository.DeletePayment(payment);
        }

        // ================================ DeletePayment ======================================

        [Fact]
        public void DeletePayment_ExistingPayment_ReturnsTrue()
        {
            // first insert
            var payment = CreatePayment();
            int newId = paymentRepository.AddPayment(payment);
            payment.Tid = newId;

            bool result = paymentRepository.DeletePayment(payment);

            Assert.True(result);
        }

        [Fact]
        public void DeletePayment_NonExistingPayment_ReturnsFalse()
        {
            var payment = new Payment { Tid = -1 };

            bool result = paymentRepository.DeletePayment(payment);

            Assert.False(result);
        }

        // ================================ UpdatePayment ======================================

        [Fact]
        public void UpdatePayment_ExistingPayment_ReturnsOldPayment()
        {
            // first insert
            var payment = CreatePayment();
            int newId = paymentRepository.AddPayment(payment);
            payment.Tid = newId;

            var updatedPayment = new Payment
            {
                Tid = newId,
                FilePath = "new/path.pdf",
                DateOfTransaction = DateTime.Now,
                DateConfirmedBuyer = DateTime.Now,
                DateConfirmedSeller = null
            };
            var oldPayment = paymentRepository.UpdatePayment(updatedPayment);
            System.Diagnostics.Debug.WriteLine(oldPayment);
            Assert.NotNull(oldPayment);
            Assert.Equal("", oldPayment.FilePath); // not null because of AddPayment function

            // clean
            paymentRepository.DeletePayment(payment);
        }
    }
}
