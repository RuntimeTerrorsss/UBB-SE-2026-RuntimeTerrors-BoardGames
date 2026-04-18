using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Repository;

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

        [Fact]
        public void GetAllPayments_SomePayments_HaveNonNullFields()
        {
            var result = repositoryPayment.GetAllPayments();
            Assert.Contains(result, p => p.DateOfTransaction != null);
            Assert.Contains(result, p => p.DateConfirmedBuyer != null);
            Assert.Contains(result, p => p.DateConfirmedSeller != null);
            Assert.Contains(result, p => p.FilePath != null);
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

        [Fact]
        public void GetPaymentById_ExistingId_WithDateOfTransactionr()
        {
            var all = repositoryPayment.GetAllPayments();
            var withTransaction = all.FirstOrDefault(p => p.DateOfTransaction != null);
            if (withTransaction == null)
            {
                return;
            }

            var result = repositoryPayment.GetPaymentById(withTransaction.Tid);
            Assert.NotNull(result.DateOfTransaction);
        }

        [Fact]
        public void GetPaymentById_ExistingId_WithDateConfirmedSeller()
        {
            var all = repositoryPayment.GetAllPayments();
            var withSeller = all.FirstOrDefault(p => p.DateConfirmedSeller != null);
            if (withSeller == null)
            {
                return;
            }

            var result = repositoryPayment.GetPaymentById(withSeller.Tid);
            Assert.NotNull(result.DateConfirmedSeller);
        }

        [Fact]
        public void GetPaymentById_ExistingId_WithDateConfirmedBuyer()
        {
            var all = repositoryPayment.GetAllPayments();
            var withBuyer = all.FirstOrDefault(p => p.DateConfirmedBuyer != null);
            if (withBuyer == null)
            {
                return;
            }

            var result = repositoryPayment.GetPaymentById(withBuyer.Tid);
            Assert.NotNull(result.DateConfirmedBuyer);
        }

        [Fact]
        public void GetPaymentById_ExistingId_WithFilePath()
        {
            var all = repositoryPayment.GetAllPayments();
            var withFile = all.FirstOrDefault(p => p.FilePath != null);
            if (withFile == null)
            {
                return;
            }

            var result = repositoryPayment.GetPaymentById(withFile.Tid);
            Assert.NotNull(result.FilePath);
        }
    }
}
