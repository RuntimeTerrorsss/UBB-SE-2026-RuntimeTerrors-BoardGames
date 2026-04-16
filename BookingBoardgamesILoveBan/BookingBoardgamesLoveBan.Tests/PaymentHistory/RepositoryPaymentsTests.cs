using BookingBoardgamesILoveBan.Src.PaymentHistory.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardgamesLoveBan.Tests.PaymentHistory
{
    public class RepositoryPaymentsTests // integration tests
    {
        private readonly IRepositoryPayment repositoryPayment;

        public RepositoryPaymentsTests()
        {
            DatabaseBootstrap.Initialize();
            repositoryPayment = new RepositoryPayment();
        }

        // ================================ GetAllPayments ======================================

        [Fact]
        public void GetAllPayments_ReturnsNonNullList()
        {
            var result = repositoryPayment.GetAllPayments();
            Assert.NotNull(result);
        }

        [Fact]
        public void GetAllPayments_ValidIds()
        {
            var result = repositoryPayment.GetAllPayments();

            foreach (var payment in result)
            {
                Assert.True(payment.Tid > 0);
            }
        }

        [Fact]
        public void GetAllPayments_ValidAmounts()
        {
            var result = repositoryPayment.GetAllPayments();

            foreach (var payment in result)
            {
                Assert.True(payment.Amount >= 0);
            }
        }

        [Fact]
        public void GetAllPayments_ValidGameName()
        {
            var result = repositoryPayment.GetAllPayments();

            foreach (var payment in result)
            {
                Assert.NotNull(payment.GameName);
                Assert.NotEmpty(payment.GameName);
            }
        }

        [Fact]
        public void GetAllPayments_ValidOwnerName()
        {
            var result = repositoryPayment.GetAllPayments();

            foreach (var payment in result)
            {
                Assert.NotNull(payment.OwnerName);
                Assert.NotEmpty(payment.OwnerName);
            }
        }

        // ================================ GetPaymentById ======================================

        [Fact]
        public void GetPaymentById_NonExistingId()
        {
            var result = repositoryPayment.GetPaymentById(-1);
            Assert.Null(result);
        }

        [Fact]
        public void GetPaymentById_ExistingId()
        {
            var all = repositoryPayment.GetAllPayments();
            if (!all.Any())
            {
                return;
            }

            int existingId = all[0].Tid;
            var result = repositoryPayment.GetPaymentById(existingId);

            Assert.NotNull(result);
            Assert.Equal(existingId, result.Tid);
        }

        [Fact]
        public void GetPaymentById_ExistingId_HasMatchingGameName()
        {
            var all = repositoryPayment.GetAllPayments();
            if (!all.Any())
            {
                return;
            }

            int existingId = all[0].Tid;
            var result = repositoryPayment.GetPaymentById(existingId);

            Assert.Equal(all[0].GameName, result.GameName);
        }

        [Fact]
        public void GetPaymentById_ExistingId_HasMatchingOwnerName()
        {
            var all = repositoryPayment.GetAllPayments();
            if (!all.Any())
            {
                return;
            }

            int existingId = all[0].Tid;
            var result = repositoryPayment.GetPaymentById(existingId);

            Assert.Equal(all[0].OwnerName, result.OwnerName);
        }
    }
}
