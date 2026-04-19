using BookingBoardgamesILoveBan.Src.PaymentCash.Mapper;
using BookingBoardgamesILoveBan.Src.PaymentCash.Model;
using BookingBoardgamesILoveBan.Src.PaymentCash.Service;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Constants;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Repository;
using BookingBoardgamesILoveBan.Src.Receipt.Service;
using Moq;

namespace BookingBoardgamesLoveBan.Tests.PaymentCash
{
    public class CashPaymentServiceTests
    {
        [Fact]
        public void AddCashPayment_ReturnsIdentifierFromPaymentRepository()
        {
            var repository = new Mock<IPaymentRepository>();
            var mapper = new Mock<ICashPaymentMapper>();
            var receiptService = new Mock<IReceiptService>();
            mapper.Setup(cashPaymentMapper => cashPaymentMapper.ToEntity(It.IsAny<CashPaymentDto>()))
                .Returns((CashPaymentDto _) => new Payment(1, 2, 3, 4, 10m, "CASH"));
            repository.Setup(paymentRepository => paymentRepository.AddPayment(It.IsAny<Payment>())).Returns(42);
            var service = new CashPaymentService(repository.Object, mapper.Object, receiptService.Object);
            var dto = new CashPaymentDto(1, 2, 3, 4, 15m);

            var paymentId = service.AddCashPayment(dto);

            Assert.Equal(42, paymentId);
        }

        [Fact]
        public void AddCashPayment_PassesSuppliedDtoToMapper()
        {
            var repository = new Mock<IPaymentRepository>();
            var mapper = new Mock<ICashPaymentMapper>();
            var receiptService = new Mock<IReceiptService>();
            var dto = new CashPaymentDto(1, 2, 3, 4, 15m);
            mapper.Setup(cashPaymentMapper => cashPaymentMapper.ToEntity(dto))
                .Returns(() => new Payment(1, 2, 3, 4, 15m, "CASH"));
            repository.Setup(paymentRepository => paymentRepository.AddPayment(It.IsAny<Payment>())).Returns(1);
            var service = new CashPaymentService(repository.Object, mapper.Object, receiptService.Object);

            service.AddCashPayment(dto);

            mapper.Verify(cashPaymentMapper => cashPaymentMapper.ToEntity(dto), Times.Once);
        }

        [Fact]
        public void AddCashPayment_PersistsMappedPaymentWithCompletedState()
        {
            var repository = new Mock<IPaymentRepository>();
            var mapper = new Mock<ICashPaymentMapper>();
            var receiptService = new Mock<IReceiptService>();
            var mapped = new Payment(9, 8, 7, 6, 20m, "CASH");
            mapper.Setup(cashPaymentMapper => cashPaymentMapper.ToEntity(It.IsAny<CashPaymentDto>())).Returns(mapped);
            Payment? added = null;
            repository.Setup(paymentRepository => paymentRepository.AddPayment(It.IsAny<Payment>()))
                .Callback<Payment>(paymentEntity => added = paymentEntity)
                .Returns(1);
            var service = new CashPaymentService(repository.Object, mapper.Object, receiptService.Object);
            var dto = new CashPaymentDto(1, 2, 3, 4, 15m);

            service.AddCashPayment(dto);

            Assert.Same(mapped, added);
            var expected = new
            {
                Tid = 9,
                RequestId = 8,
                ClientId = 7,
                OwnerId = 6,
                Amount = 20m,
                PaymentMethod = "CASH",
                State = PaymentConstrants.StateCompleted,
            };
            var actual = new
            {
                added!.Tid,
                added.RequestId,
                added.ClientId,
                added.OwnerId,
                added.Amount,
                added.PaymentMethod,
                added.State,
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetCashPayment_ReturnsDtoProducedByMapperForStoredPayment()
        {
            var repository = new Mock<IPaymentRepository>();
            var mapper = new Mock<ICashPaymentMapper>();
            var receiptService = new Mock<IReceiptService>();
            var stored = new Payment(10, 11, 12, 13, 14m, "CASH");
            var expectedDto = new CashPaymentDto(20, 21, 22, 23, 24m);
            repository.Setup(paymentRepository => paymentRepository.GetById(10)).Returns(stored);
            mapper.Setup(cashPaymentMapper => cashPaymentMapper.ToDto(stored)).Returns(expectedDto);
            var service = new CashPaymentService(repository.Object, mapper.Object, receiptService.Object);

            var result = service.GetCashPayment(10);

            Assert.Same(expectedDto, result);
        }

        [Fact]
        public void GetCashPayment_LoadsPaymentByIdentifierFromRepository()
        {
            var repository = new Mock<IPaymentRepository>();
            var mapper = new Mock<ICashPaymentMapper>();
            var receiptService = new Mock<IReceiptService>();
            var stored = new Payment(10, 11, 12, 13, 14m, "CASH");
            repository.Setup(paymentRepository => paymentRepository.GetById(55)).Returns(stored);
            mapper.Setup(cashPaymentMapper => cashPaymentMapper.ToDto(stored)).Returns(new CashPaymentDto(1, 1, 1, 1, 1m));
            var service = new CashPaymentService(repository.Object, mapper.Object, receiptService.Object);

            service.GetCashPayment(55);

            repository.Verify(paymentRepository => paymentRepository.GetById(55), Times.Once);
        }

        [Fact]
        public void ConfirmDelivery_RecordsBuyerConfirmationOnPayment()
        {
            var repository = new Mock<IPaymentRepository>();
            var mapper = new Mock<ICashPaymentMapper>();
            var receiptService = new Mock<IReceiptService>();
            var payment = new Payment(1, 5, 2, 3, 10m, "CASH")
            {
                DateConfirmedSeller = null,
                DateConfirmedBuyer = null,
            };
            repository.Setup(paymentRepository => paymentRepository.GetById(1)).Returns(payment);
            var service = new CashPaymentService(repository.Object, mapper.Object, receiptService.Object);

            service.ConfirmDelivery(1);

            var expected = new
            {
                BuyerConfirmed = true,
                SellerConfirmed = false,
            };
            var actual = new
            {
                BuyerConfirmed = payment.DateConfirmedBuyer != null,
                SellerConfirmed = payment.DateConfirmedSeller != null,
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ConfirmDelivery_WhenOnlyBuyerHasConfirmed_DoesNotGenerateReceipt()
        {
            var repository = new Mock<IPaymentRepository>();
            var mapper = new Mock<ICashPaymentMapper>();
            var receiptService = new Mock<IReceiptService>();
            var payment = new Payment(1, 5, 2, 3, 10m, "CASH")
            {
                DateConfirmedSeller = null,
                DateConfirmedBuyer = null,
            };
            repository.Setup(paymentRepository => paymentRepository.GetById(1)).Returns(payment);
            var service = new CashPaymentService(repository.Object, mapper.Object, receiptService.Object);

            service.ConfirmDelivery(1);

            receiptService.Verify(receiptServiceMock => receiptServiceMock.GenerateReceiptRelativePath(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void ConfirmDelivery_WhenBothPartiesAlreadyConfirmed_GeneratesReceiptForRentalRequest()
        {
            var repository = new Mock<IPaymentRepository>();
            var mapper = new Mock<ICashPaymentMapper>();
            var receiptService = new Mock<IReceiptService>();
            var payment = new Payment(2, 42, 2, 3, 10m, "CASH")
            {
                DateConfirmedSeller = DateTime.Now.AddMinutes(-1),
                DateConfirmedBuyer = null,
            };
            repository.Setup(paymentRepository => paymentRepository.GetById(2)).Returns(payment);
            var service = new CashPaymentService(repository.Object, mapper.Object, receiptService.Object);

            service.ConfirmDelivery(2);

            receiptService.Verify(receiptServiceMock => receiptServiceMock.GenerateReceiptRelativePath(42), Times.Once);
        }

        [Fact]
        public void ConfirmDelivery_PersistsUpdatedPayment()
        {
            var repository = new Mock<IPaymentRepository>();
            var mapper = new Mock<ICashPaymentMapper>();
            var receiptService = new Mock<IReceiptService>();
            var payment = new Payment(1, 5, 2, 3, 10m, "CASH");
            repository.Setup(paymentRepository => paymentRepository.GetById(1)).Returns(payment);
            var service = new CashPaymentService(repository.Object, mapper.Object, receiptService.Object);

            service.ConfirmDelivery(1);

            repository.Verify(paymentRepository => paymentRepository.UpdatePayment(payment), Times.Once);
        }

        [Fact]
        public void ConfirmPayment_RecordsSellerConfirmationOnPayment()
        {
            var repository = new Mock<IPaymentRepository>();
            var mapper = new Mock<ICashPaymentMapper>();
            var receiptService = new Mock<IReceiptService>();
            var payment = new Payment(3, 8, 2, 3, 10m, "CASH")
            {
                DateConfirmedBuyer = null,
                DateConfirmedSeller = null,
            };
            repository.Setup(paymentRepository => paymentRepository.GetById(3)).Returns(payment);
            var service = new CashPaymentService(repository.Object, mapper.Object, receiptService.Object);

            service.ConfirmPayment(3);

            var expected = new
            {
                BuyerConfirmed = false,
                SellerConfirmed = true,
            };
            var actual = new
            {
                BuyerConfirmed = payment.DateConfirmedBuyer != null,
                SellerConfirmed = payment.DateConfirmedSeller != null,
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ConfirmPayment_WhenOnlySellerHasConfirmed_DoesNotGenerateReceipt()
        {
            var repository = new Mock<IPaymentRepository>();
            var mapper = new Mock<ICashPaymentMapper>();
            var receiptService = new Mock<IReceiptService>();
            var payment = new Payment(3, 8, 2, 3, 10m, "CASH");
            repository.Setup(paymentRepository => paymentRepository.GetById(3)).Returns(payment);
            var service = new CashPaymentService(repository.Object, mapper.Object, receiptService.Object);

            service.ConfirmPayment(3);

            receiptService.Verify(receiptServiceMock => receiptServiceMock.GenerateReceiptRelativePath(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void ConfirmPayment_WhenBothPartiesAlreadyConfirmed_GeneratesReceiptForRentalRequest()
        {
            var repository = new Mock<IPaymentRepository>();
            var mapper = new Mock<ICashPaymentMapper>();
            var receiptService = new Mock<IReceiptService>();
            var payment = new Payment(4, 71, 2, 3, 10m, "CASH")
            {
                DateConfirmedBuyer = DateTime.Now.AddMinutes(-1),
                DateConfirmedSeller = null,
            };
            repository.Setup(paymentRepository => paymentRepository.GetById(4)).Returns(payment);
            var service = new CashPaymentService(repository.Object, mapper.Object, receiptService.Object);

            service.ConfirmPayment(4);

            receiptService.Verify(receiptServiceMock => receiptServiceMock.GenerateReceiptRelativePath(71), Times.Once);
        }

        [Fact]
        public void ConfirmPayment_PersistsUpdatedPayment()
        {
            var repository = new Mock<IPaymentRepository>();
            var mapper = new Mock<ICashPaymentMapper>();
            var receiptService = new Mock<IReceiptService>();
            var payment = new Payment(3, 8, 2, 3, 10m, "CASH");
            repository.Setup(paymentRepository => paymentRepository.GetById(3)).Returns(payment);
            var service = new CashPaymentService(repository.Object, mapper.Object, receiptService.Object);

            service.ConfirmPayment(3);

            repository.Verify(paymentRepository => paymentRepository.UpdatePayment(payment), Times.Once);
        }

        [Fact]
        public void IsAllConfirmed_WhenBothConfirmationDatesAreSet_ReturnsTrue()
        {
            var repository = new Mock<IPaymentRepository>();
            var mapper = new Mock<ICashPaymentMapper>();
            var receiptService = new Mock<IReceiptService>();
            var payment = new Payment(5, 9, 2, 3, 10m, "CASH")
            {
                DateConfirmedBuyer = DateTime.Now,
                DateConfirmedSeller = DateTime.Now,
            };
            repository.Setup(paymentRepository => paymentRepository.GetById(5)).Returns(payment);
            var service = new CashPaymentService(repository.Object, mapper.Object, receiptService.Object);

            var result = service.IsAllConfirmed(5);

            Assert.True(result);
        }

        [Fact]
        public void IsAllConfirmed_WhenBothConfirmationDatesAreSet_SetsStateToConfirmed()
        {
            var repository = new Mock<IPaymentRepository>();
            var mapper = new Mock<ICashPaymentMapper>();
            var receiptService = new Mock<IReceiptService>();
            var payment = new Payment(5, 9, 2, 3, 10m, "CASH")
            {
                DateConfirmedBuyer = DateTime.Now,
                DateConfirmedSeller = DateTime.Now,
            };
            repository.Setup(paymentRepository => paymentRepository.GetById(5)).Returns(payment);
            var service = new CashPaymentService(repository.Object, mapper.Object, receiptService.Object);

            service.IsAllConfirmed(5);

            Assert.Equal(PaymentConstrants.StateConfirmed, payment.State);
        }

        [Fact]
        public void IsAllConfirmed_WhenSellerConfirmationIsMissing_ReturnsFalse()
        {
            var repository = new Mock<IPaymentRepository>();
            var mapper = new Mock<ICashPaymentMapper>();
            var receiptService = new Mock<IReceiptService>();
            var payment = new Payment(6, 9, 2, 3, 10m, "CASH")
            {
                DateConfirmedBuyer = DateTime.Now,
                DateConfirmedSeller = null,
            };
            repository.Setup(paymentRepository => paymentRepository.GetById(6)).Returns(payment);
            var service = new CashPaymentService(repository.Object, mapper.Object, receiptService.Object);

            var result = service.IsAllConfirmed(6);

            Assert.False(result);
        }

        [Fact]
        public void IsDeliveryConfirmed_WhenBuyerConfirmationExists_ReturnsTrue()
        {
            var repository = new Mock<IPaymentRepository>();
            var mapper = new Mock<ICashPaymentMapper>();
            var receiptService = new Mock<IReceiptService>();
            var payment = new Payment(7, 9, 2, 3, 10m, "CASH")
            {
                DateConfirmedBuyer = DateTime.Now,
            };
            repository.Setup(paymentRepository => paymentRepository.GetById(7)).Returns(payment);
            var service = new CashPaymentService(repository.Object, mapper.Object, receiptService.Object);

            Assert.True(service.IsDeliveryConfirmed(7));
        }

        [Fact]
        public void IsDeliveryConfirmed_WhenBuyerConfirmationIsMissing_ReturnsFalse()
        {
            var repository = new Mock<IPaymentRepository>();
            var mapper = new Mock<ICashPaymentMapper>();
            var receiptService = new Mock<IReceiptService>();
            var payment = new Payment(8, 9, 2, 3, 10m, "CASH")
            {
                DateConfirmedBuyer = null,
            };
            repository.Setup(paymentRepository => paymentRepository.GetById(8)).Returns(payment);
            var service = new CashPaymentService(repository.Object, mapper.Object, receiptService.Object);

            Assert.False(service.IsDeliveryConfirmed(8));
        }

        [Fact]
        public void IsPaymentConfirmed_WhenSellerConfirmationExists_ReturnsTrue()
        {
            var repository = new Mock<IPaymentRepository>();
            var mapper = new Mock<ICashPaymentMapper>();
            var receiptService = new Mock<IReceiptService>();
            var payment = new Payment(9, 9, 2, 3, 10m, "CASH")
            {
                DateConfirmedSeller = DateTime.Now,
            };
            repository.Setup(paymentRepository => paymentRepository.GetById(9)).Returns(payment);
            var service = new CashPaymentService(repository.Object, mapper.Object, receiptService.Object);

            Assert.True(service.IsPaymentConfirmed(9));
        }

        [Fact]
        public void IsPaymentConfirmed_WhenSellerConfirmationIsMissing_ReturnsFalse()
        {
            var repository = new Mock<IPaymentRepository>();
            var mapper = new Mock<ICashPaymentMapper>();
            var receiptService = new Mock<IReceiptService>();
            var payment = new Payment(10, 9, 2, 3, 10m, "CASH")
            {
                DateConfirmedSeller = null,
            };
            repository.Setup(paymentRepository => paymentRepository.GetById(10)).Returns(payment);
            var service = new CashPaymentService(repository.Object, mapper.Object, receiptService.Object);

            Assert.False(service.IsPaymentConfirmed(10));
        }
    }
}
