using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Repository;

namespace BookingBoardgamesLoveBan.Tests.PaymentCommon
{
    public class PaymentRepositoryTests
    {
        private readonly IPaymentRepository paymentRepository;

        public PaymentRepositoryTests()
        {
            DatabaseBootstrap.Initialize();
            paymentRepository = new PaymentRepository();
        }

        [Fact]
        public void AddPayment_ReturnsPositiveIdentifier()
        {
            var paymentEntity = BuildPaymentEntity();

            try
            {
                var paymentIdentifier = paymentRepository.AddPayment(paymentEntity);

                Assert.True(paymentIdentifier > 0);
                paymentEntity.Tid = paymentIdentifier;
            }
            finally
            {
                if (paymentEntity.Tid > 0)
                {
                    paymentRepository.DeletePayment(paymentEntity);
                }
            }
        }

        [Fact]
        public void GetById_ForMissingPayment_ReturnsNull()
        {
            var persistedPayment = paymentRepository.GetById(-1);

            Assert.Null(persistedPayment);
        }

        [Fact]
        public void GetById_ForExistingPayment_ReturnsPaymentWithMatchingIdentifier()
        {
            var paymentEntity = BuildPaymentEntity();

            try
            {
                paymentEntity.Tid = paymentRepository.AddPayment(paymentEntity);

                var persistedPayment = paymentRepository.GetById(paymentEntity.Tid);

                Assert.NotNull(persistedPayment);
                Assert.Equal(paymentEntity.Tid, persistedPayment!.Tid);
            }
            finally
            {
                if (paymentEntity.Tid > 0)
                {
                    paymentRepository.DeletePayment(paymentEntity);
                }
            }
        }

        [Fact]
        public void GetAll_IncludesAddedPayment()
        {
            var paymentEntity = BuildPaymentEntity();

            try
            {
                paymentEntity.Tid = paymentRepository.AddPayment(paymentEntity);

                var allPersistedPayments = paymentRepository.GetAll();

                Assert.Contains(allPersistedPayments, persistedPayment => persistedPayment.Tid == paymentEntity.Tid);
            }
            finally
            {
                if (paymentEntity.Tid > 0)
                {
                    paymentRepository.DeletePayment(paymentEntity);
                }
            }
        }

        [Fact]
        public void DeletePayment_ForExistingPayment_ReturnsTrue()
        {
            var paymentEntity = BuildPaymentEntity();
            paymentEntity.Tid = paymentRepository.AddPayment(paymentEntity);

            var wasPaymentDeleted = paymentRepository.DeletePayment(paymentEntity);

            Assert.True(wasPaymentDeleted);
        }

        [Fact]
        public void DeletePayment_ForMissingPayment_ReturnsFalse()
        {
            var paymentEntity = new Payment { Tid = -1 };

            var wasPaymentDeleted = paymentRepository.DeletePayment(paymentEntity);

            Assert.False(wasPaymentDeleted);
        }

        [Fact]
        public void UpdatePayment_ReturnsPreviousPersistedVersion()
        {
            var paymentEntity = BuildPaymentEntity();

            try
            {
                paymentEntity.Tid = paymentRepository.AddPayment(paymentEntity);
                var updatedPaymentEntity = new Payment
                {
                    Tid = paymentEntity.Tid,
                    FilePath = "receipts/new.pdf",
                    DateOfTransaction = new DateTime(2026, 4, 19, 12, 0, 0),
                    DateConfirmedBuyer = new DateTime(2026, 4, 19, 13, 0, 0),
                    DateConfirmedSeller = null,
                };

                var previousPersistedPayment = paymentRepository.UpdatePayment(updatedPaymentEntity);

                Assert.NotNull(previousPersistedPayment);
                Assert.Equal(string.Empty, previousPersistedPayment!.FilePath);
            }
            finally
            {
                if (paymentEntity.Tid > 0)
                {
                    paymentRepository.DeletePayment(paymentEntity);
                }
            }
        }

        [Fact]
        public void UpdatePayment_PersistsMutablePaymentFields()
        {
            var paymentEntity = BuildPaymentEntity();

            try
            {
                paymentEntity.Tid = paymentRepository.AddPayment(paymentEntity);
                var dateOfTransaction = new DateTime(2026, 4, 19, 14, 0, 0);
                var dateConfirmedBuyer = new DateTime(2026, 4, 19, 15, 0, 0);
                var updatedPaymentEntity = new Payment
                {
                    Tid = paymentEntity.Tid,
                    FilePath = "receipts/updated.pdf",
                    DateOfTransaction = dateOfTransaction,
                    DateConfirmedBuyer = dateConfirmedBuyer,
                    DateConfirmedSeller = null,
                };

                paymentRepository.UpdatePayment(updatedPaymentEntity);
                var persistedPayment = paymentRepository.GetById(paymentEntity.Tid);

                Assert.NotNull(persistedPayment);
                var expected = new
                {
                    FilePath = (string?)"receipts/updated.pdf",
                    DateOfTransaction = (DateTime?)dateOfTransaction,
                    DateConfirmedBuyer = (DateTime?)dateConfirmedBuyer,
                    DateConfirmedSeller = (DateTime?)null,
                };
                var actual = new
                {
                    persistedPayment!.FilePath,
                    persistedPayment.DateOfTransaction,
                    persistedPayment.DateConfirmedBuyer,
                    persistedPayment.DateConfirmedSeller,
                };

                Assert.Equal(expected, actual);
            }
            finally
            {
                if (paymentEntity.Tid > 0)
                {
                    paymentRepository.DeletePayment(paymentEntity);
                }
            }
        }

        private static Payment BuildPaymentEntity()
        {
            return new Payment
            {
                RequestId = 1,
                ClientId = 1,
                OwnerId = 2,
                Amount = 123.45m,
                PaymentMethod = "CARD",
                State = 0,
                DateOfTransaction = DateTime.Now,
                DateConfirmedBuyer = null,
                DateConfirmedSeller = null,
                FilePath = null,
            };
        }
    }
}
