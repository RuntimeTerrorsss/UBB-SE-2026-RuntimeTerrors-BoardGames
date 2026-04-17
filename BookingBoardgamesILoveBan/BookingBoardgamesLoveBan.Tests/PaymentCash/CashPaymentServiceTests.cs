using BookingBoardgamesILoveBan.Src.PaymentCash.Mapper;
using BookingBoardgamesILoveBan.Src.PaymentCash.Model;
using BookingBoardgamesILoveBan.Src.PaymentCash.Service;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Constants;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Repository;
using BookingBoardgamesILoveBan.Src.Receipt.Service;

namespace BookingBoardgamesLoveBan.Tests.PaymentCash
{
    public class CashPaymentServiceTests
    {
        [Fact]
        public void AddCashPayment_MapsEntity_SetsCompletedState_AndReturnsRepositoryId()
        {
            var repository = new FakePaymentRepository();
            var mapper = new FakeCashPaymentMapper();
            var receiptService = new FakeReceiptService();
            var service = new CashPaymentService(repository, mapper, receiptService);
            var dto = new CashPaymentDto(1, 2, 3, 4, 15m);

            var paymentId = service.AddCashPayment(dto);

            Assert.Equal(99, paymentId);
            Assert.Same(dto, mapper.LastToEntityInput);
            Assert.NotNull(repository.LastAddedPayment);
            Assert.Equal(PaymentConstrants.StateCompleted, repository.LastAddedPayment!.State);
        }

        [Fact]
        public void GetCashPayment_UsesRepositoryAndMapper()
        {
            var repository = new FakePaymentRepository();
            var mapper = new FakeCashPaymentMapper();
            var receiptService = new FakeReceiptService();
            var service = new CashPaymentService(repository, mapper, receiptService);
            var payment = new Payment(10, 11, 12, 13, 14m, "CASH");
            repository.ById[10] = payment;
            var mappedDto = new CashPaymentDto(20, 21, 22, 23, 24m);
            mapper.ToDtoResult = mappedDto;

            var result = service.GetCashPayment(10);

            Assert.Same(mappedDto, result);
            Assert.Equal(10, repository.LastGetByIdArgument);
            Assert.Same(payment, mapper.LastToDtoInput);
        }

        [Fact]
        public void ConfirmDelivery_WhenNotFullyConfirmed_DoesNotGenerateReceipt_AndUpdatesPayment()
        {
            var repository = new FakePaymentRepository();
            var mapper = new FakeCashPaymentMapper();
            var receiptService = new FakeReceiptService();
            var service = new CashPaymentService(repository, mapper, receiptService);
            var payment = new Payment(1, 5, 2, 3, 10m, "CASH");
            payment.DateConfirmedSeller = null;
            payment.DateConfirmedBuyer = null;
            repository.ById[1] = payment;

            service.ConfirmDelivery(1);

            Assert.NotNull(payment.DateConfirmedBuyer);
            Assert.Null(payment.DateConfirmedSeller);
            Assert.Equal(0, receiptService.GenerateCalls);
            Assert.Same(payment, repository.LastUpdatedPayment);
        }

        [Fact]
        public void ConfirmDelivery_WhenFullyConfirmed_GeneratesReceipt_AndUpdatesPayment()
        {
            var repository = new FakePaymentRepository();
            var mapper = new FakeCashPaymentMapper();
            var receiptService = new FakeReceiptService();
            var service = new CashPaymentService(repository, mapper, receiptService);
            var payment = new Payment(2, 42, 2, 3, 10m, "CASH");
            payment.DateConfirmedSeller = DateTime.Now.AddMinutes(-1);
            payment.DateConfirmedBuyer = null;
            repository.ById[2] = payment;

            service.ConfirmDelivery(2);

            Assert.Equal(1, receiptService.GenerateCalls);
            Assert.Equal(42, receiptService.LastGenerateRequestId);
            Assert.Same(payment, repository.LastUpdatedPayment);
        }

        [Fact]
        public void ConfirmPayment_WhenNotFullyConfirmed_DoesNotGenerateReceipt_AndUpdatesPayment()
        {
            var repository = new FakePaymentRepository();
            var mapper = new FakeCashPaymentMapper();
            var receiptService = new FakeReceiptService();
            var service = new CashPaymentService(repository, mapper, receiptService);
            var payment = new Payment(3, 8, 2, 3, 10m, "CASH");
            payment.DateConfirmedBuyer = null;
            payment.DateConfirmedSeller = null;
            repository.ById[3] = payment;

            service.ConfirmPayment(3);

            Assert.NotNull(payment.DateConfirmedSeller);
            Assert.Null(payment.DateConfirmedBuyer);
            Assert.Equal(0, receiptService.GenerateCalls);
            Assert.Same(payment, repository.LastUpdatedPayment);
        }

        [Fact]
        public void ConfirmPayment_WhenFullyConfirmed_GeneratesReceipt_AndUpdatesPayment()
        {
            var repository = new FakePaymentRepository();
            var mapper = new FakeCashPaymentMapper();
            var receiptService = new FakeReceiptService();
            var service = new CashPaymentService(repository, mapper, receiptService);
            var payment = new Payment(4, 71, 2, 3, 10m, "CASH");
            payment.DateConfirmedBuyer = DateTime.Now.AddMinutes(-1);
            payment.DateConfirmedSeller = null;
            repository.ById[4] = payment;

            service.ConfirmPayment(4);

            Assert.Equal(1, receiptService.GenerateCalls);
            Assert.Equal(71, receiptService.LastGenerateRequestId);
            Assert.Same(payment, repository.LastUpdatedPayment);
        }

        [Fact]
        public void IsAllConfirmed_WhenBothDatesSet_ReturnsTrue_AndSetsConfirmedState()
        {
            var repository = new FakePaymentRepository();
            var mapper = new FakeCashPaymentMapper();
            var receiptService = new FakeReceiptService();
            var service = new CashPaymentService(repository, mapper, receiptService);
            var payment = new Payment(5, 9, 2, 3, 10m, "CASH");
            payment.DateConfirmedBuyer = DateTime.Now;
            payment.DateConfirmedSeller = DateTime.Now;
            repository.ById[5] = payment;

            var result = service.IsAllConfirmed(5);

            Assert.True(result);
            Assert.Equal(PaymentConstrants.StateConfirmed, payment.State);
        }

        [Fact]
        public void IsAllConfirmed_WhenOneDateMissing_ReturnsFalse()
        {
            var repository = new FakePaymentRepository();
            var mapper = new FakeCashPaymentMapper();
            var receiptService = new FakeReceiptService();
            var service = new CashPaymentService(repository, mapper, receiptService);
            var payment = new Payment(6, 9, 2, 3, 10m, "CASH");
            payment.DateConfirmedBuyer = DateTime.Now;
            payment.DateConfirmedSeller = null;
            repository.ById[6] = payment;

            var result = service.IsAllConfirmed(6);

            Assert.False(result);
        }

        [Fact]
        public void IsDeliveryConfirmed_ReturnsTrueWhenBuyerDateExists_ElseFalse()
        {
            var repository = new FakePaymentRepository();
            var mapper = new FakeCashPaymentMapper();
            var receiptService = new FakeReceiptService();
            var service = new CashPaymentService(repository, mapper, receiptService);

            var confirmed = new Payment(7, 9, 2, 3, 10m, "CASH")
            {
                DateConfirmedBuyer = DateTime.Now
            };
            var notConfirmed = new Payment(8, 9, 2, 3, 10m, "CASH")
            {
                DateConfirmedBuyer = null
            };
            repository.ById[7] = confirmed;
            repository.ById[8] = notConfirmed;

            Assert.True(service.IsDeliveryConfirmed(7));
            Assert.False(service.IsDeliveryConfirmed(8));
        }

        [Fact]
        public void IsPaymentConfirmed_ReturnsTrueWhenSellerDateExists_ElseFalse()
        {
            var repository = new FakePaymentRepository();
            var mapper = new FakeCashPaymentMapper();
            var receiptService = new FakeReceiptService();
            var service = new CashPaymentService(repository, mapper, receiptService);

            var confirmed = new Payment(9, 9, 2, 3, 10m, "CASH")
            {
                DateConfirmedSeller = DateTime.Now
            };
            var notConfirmed = new Payment(10, 9, 2, 3, 10m, "CASH")
            {
                DateConfirmedSeller = null
            };
            repository.ById[9] = confirmed;
            repository.ById[10] = notConfirmed;

            Assert.True(service.IsPaymentConfirmed(9));
            Assert.False(service.IsPaymentConfirmed(10));
        }

        private sealed class FakePaymentRepository : IPaymentRepository
        {
            public Dictionary<int, Payment> ById { get; } = new();
            public Payment? LastAddedPayment { get; private set; }
            public Payment? LastUpdatedPayment { get; private set; }
            public int LastGetByIdArgument { get; private set; }

            public IReadOnlyList<Payment> GetAll()
            {
                return ById.Values.ToList();
            }

            public Payment GetById(int tid)
            {
                LastGetByIdArgument = tid;
                return ById[tid];
            }

            public int AddPayment(Payment transaction)
            {
                LastAddedPayment = transaction;
                ById[transaction.Tid] = transaction;
                return 99;
            }

            public bool DeletePayment(Payment transaction)
            {
                return ById.Remove(transaction.Tid);
            }

            public Payment UpdatePayment(Payment transaction)
            {
                LastUpdatedPayment = transaction;
                ById[transaction.Tid] = transaction;
                return transaction;
            }
        }

        private sealed class FakeCashPaymentMapper : ICashPaymentMapper
        {
            public CashPaymentDto? LastToEntityInput { get; private set; }
            public Payment? LastToDtoInput { get; private set; }
            public CashPaymentDto? ToDtoResult { get; set; }

            public Payment ToEntity(CashPaymentDto paymentDto)
            {
                LastToEntityInput = paymentDto;
                return new Payment(
                    paymentDto.Id, paymentDto.Requestd, paymentDto.ClientId, paymentDto.OwnerId, paymentDto.Amount, "CASH");
            }

            public CashPaymentDto ToDto(Payment payment)
            {
                LastToDtoInput = payment;
                return ToDtoResult ?? new CashPaymentDto(payment.Tid, payment.RequestId, payment.ClientId, payment.OwnerId, payment.Amount);
            }
        }

        private class FakeReceiptService : IReceiptService
        {
            public int GenerateCalls { get; private set; }
            public int LastGenerateRequestId { get; private set; }

            public string GenerateReceiptRelativePath(int rentalId)
            {
                GenerateCalls++;
                LastGenerateRequestId = rentalId;
                return $"receipt-{rentalId}.pdf";
            }

            public string GetReceiptDocument(Payment payment)
            {
                return payment.FilePath ?? string.Empty;
            }
        }
    }
}
