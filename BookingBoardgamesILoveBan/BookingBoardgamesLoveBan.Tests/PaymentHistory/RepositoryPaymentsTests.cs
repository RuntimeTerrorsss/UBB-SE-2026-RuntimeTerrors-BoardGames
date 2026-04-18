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
        public void GetAllPayments_SomePayments_HaveDateOfTransaction()
        {
            var result = repositoryPayment.GetAllPayments();
            Assert.Contains(result, p => p.DateOfTransaction != null);
        }

        [Fact]
        public void GetAllPayments_SomePayments_HaveDateConfirmedBuyer()
        {
            var result = repositoryPayment.GetAllPayments();
            Assert.Contains(result, p => p.DateConfirmedBuyer != null);
        }

        [Fact]
        public void GetAllPayments_SomePayments_HaveDateConfirmedSeller()
        {
            var result = repositoryPayment.GetAllPayments();
            Assert.Contains(result, p => p.DateConfirmedSeller != null);
        }

        [Fact]
        public void GetAllPayments_SomePayments_HaveFilePath()
        {
            var result = repositoryPayment.GetAllPayments();
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
            var allPayments = repositoryPayment.GetAllPayments();
            if (!allPayments.Any())
            {
                return;
            }

            var firstPayment = allPayments[0];
            int existingId = firstPayment.Tid;
            var result = repositoryPayment.GetPaymentById(existingId);

            Assert.NotNull(result);
            Assert.Equal(existingId, result.Tid);
        }

        [Fact]
        public void GetPaymentById_ExistingId_HasMatchingGameName()
        {
            var allPayments = repositoryPayment.GetAllPayments();
            if (!allPayments.Any())
            {
                return;
            }

            var firstPayment = allPayments[0];
            int existingId = firstPayment.Tid;
            var result = repositoryPayment.GetPaymentById(existingId);

            Assert.Equal(firstPayment.GameName, result.GameName);
        }

        [Fact]
        public void GetPaymentById_ExistingId_HasMatchingOwnerName()
        {
            var allPayments = repositoryPayment.GetAllPayments();
            if (!allPayments.Any())
            {
                return;
            }

            var firstPayment = allPayments[0];
            int existingId = firstPayment.Tid;
            var result = repositoryPayment.GetPaymentById(existingId);

            Assert.Equal(firstPayment.OwnerName, result.OwnerName);
        }

        [Fact]
        public void GetPaymentById_ExistingId_WithDateOfTransactionr()
        {
            var allPayments = repositoryPayment.GetAllPayments();
            var paymentWithDateOfTransaction = allPayments.FirstOrDefault(payment => payment.DateOfTransaction != null);
            if (paymentWithDateOfTransaction == null)
            {
                return;
            }

            var result = repositoryPayment.GetPaymentById(paymentWithDateOfTransaction.Tid);
            Assert.NotNull(result.DateOfTransaction);
            Assert.Equal(paymentWithDateOfTransaction.DateOfTransaction, result.DateOfTransaction);
        }

        [Fact]
        public void GetPaymentById_ExistingId_WithDateConfirmedSeller()
        {
            var allPayments = repositoryPayment.GetAllPayments();
            var paymentWithDateConfirmedSeller = allPayments.FirstOrDefault(payment => payment.DateConfirmedSeller != null);
            if (paymentWithDateConfirmedSeller == null)
            {
                return;
            }

            var result = repositoryPayment.GetPaymentById(paymentWithDateConfirmedSeller.Tid);
            Assert.NotNull(result.DateConfirmedSeller);
            Assert.Equal(paymentWithDateConfirmedSeller.DateConfirmedSeller, result.DateConfirmedSeller);
        }

        [Fact]
        public void GetPaymentById_ExistingId_WithDateConfirmedBuyer()
        {
            var allPayments = repositoryPayment.GetAllPayments();
            var paymentWithDateConfirmedBuyer = allPayments.FirstOrDefault(payment => payment.DateConfirmedBuyer != null);
            if (paymentWithDateConfirmedBuyer == null)
            {
                return;
            }

            var result = repositoryPayment.GetPaymentById(paymentWithDateConfirmedBuyer.Tid);
            Assert.NotNull(result.DateConfirmedBuyer);
            Assert.Equal(paymentWithDateConfirmedBuyer.DateConfirmedBuyer, result.DateConfirmedBuyer);
        }

        [Fact]
        public void GetPaymentById_ExistingId_WithFilePath()
        {
            var allPayments = repositoryPayment.GetAllPayments();
            var paymentWithFilePath = allPayments.FirstOrDefault(payment => payment.FilePath != null);
            if (paymentWithFilePath == null)
            {
                return;
            }

            var result = repositoryPayment.GetPaymentById(paymentWithFilePath.Tid);
            Assert.NotNull(result.FilePath);
            Assert.Equal(paymentWithFilePath.FilePath, result.FilePath);
        }
    }
}
