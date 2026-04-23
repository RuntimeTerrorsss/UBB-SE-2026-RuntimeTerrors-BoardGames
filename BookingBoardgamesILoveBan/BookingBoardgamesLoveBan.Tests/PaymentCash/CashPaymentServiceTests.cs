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
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var paymentMapperMock = new Mock<ICashPaymentMapper>();
            var receiptServiceMock = new Mock<IReceiptService>();
            paymentMapperMock.Setup(cashPaymentMapper => cashPaymentMapper.TurnDataTransferObjectIntoEntity(It.IsAny<CashPaymentDataTransferObject>()))
                .Returns((CashPaymentDataTransferObject _) => new Payment(1, 2, 3, 4, 10m, "CASH"));
            paymentRepositoryMock.Setup(paymentRepository => paymentRepository.AddPayment(It.IsAny<Payment>())).Returns(42);
            var service = new CashPaymentService(paymentRepositoryMock.Object, paymentMapperMock.Object, receiptServiceMock.Object);
            var dataTransferObject = new CashPaymentDataTransferObject(1, 2, 3, 4, 15m);

            var paymentId = service.AddCashPayment(dataTransferObject);

            Assert.Equal(42, paymentId);
        }

        [Fact]
        public void AddCashPayment_PassesSuppliedDtoToMapper()
        {
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var paymentMapperMock = new Mock<ICashPaymentMapper>();
            var receiptServiceMock = new Mock<IReceiptService>();
            var dto = new CashPaymentDataTransferObject(1, 2, 3, 4, 15m);
            paymentMapperMock.Setup(cashPaymentMapper => cashPaymentMapper.TurnDataTransferObjectIntoEntity(dto))
                .Returns(() => new Payment(1, 2, 3, 4, 15m, "CASH"));
            paymentRepositoryMock.Setup(paymentRepository => paymentRepository.AddPayment(It.IsAny<Payment>())).Returns(1);
            var service = new CashPaymentService(paymentRepositoryMock.Object, paymentMapperMock.Object, receiptServiceMock.Object);

            service.AddCashPayment(dto);

            paymentMapperMock.Verify(cashPaymentMapper => cashPaymentMapper.TurnDataTransferObjectIntoEntity(dto), Times.Once);
        }

        [Fact]
        public void AddCashPayment_PersistsMappedPaymentWithCompletedState()
        {
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var paymentMapperMock = new Mock<ICashPaymentMapper>();
            var receiptServiceMock = new Mock<IReceiptService>();
            var mapped = new Payment(9, 8, 7, 6, 20m, "CASH");
            paymentMapperMock.Setup(cashPaymentMapper => cashPaymentMapper.TurnDataTransferObjectIntoEntity(It.IsAny<CashPaymentDataTransferObject>())).Returns(mapped);
            Payment? added = null;
            paymentRepositoryMock.Setup(paymentRepository => paymentRepository.AddPayment(It.IsAny<Payment>()))
                .Callback<Payment>(paymentEntity => added = paymentEntity)
                .Returns(1);
            var service = new CashPaymentService(paymentRepositoryMock.Object, paymentMapperMock.Object, receiptServiceMock.Object);
            var dto = new CashPaymentDataTransferObject(1, 2, 3, 4, 15m);

            service.AddCashPayment(dto);

            Assert.Same(mapped, added);
            var expected = new
            {
                TransactionIdentifier = 9,
                RequestId = 8,
                ClientId = 7,
                OwnerId = 6,
                PaidAmount = 20m,
                PaymentMethod = "CASH",
                PaymentState = PaymentConstrants.StateCompleted,
            };
            var actual = new
            {
                added!.TransactionIdentifier,
                added.RequestId,
                added.ClientId,
                added.OwnerId,
                added.PaidAmount,
                added.PaymentMethod,
                added.PaymentState,
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetCashPayment_ReturnsDtoProducedByMapperForStoredPayment()
        {
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var paymentMapperMock = new Mock<ICashPaymentMapper>();
            var receiptServiceMock = new Mock<IReceiptService>();
            var stored = new Payment(10, 11, 12, 13, 14m, "CASH");
            var expectedDto = new CashPaymentDataTransferObject(20, 21, 22, 23, 24m);
            paymentRepositoryMock.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifier(10)).Returns(stored);
            paymentMapperMock.Setup(cashPaymentMapper => cashPaymentMapper.TurnEntityIntoDataTransferObject(stored)).Returns(expectedDto);
            var service = new CashPaymentService(paymentRepositoryMock.Object, paymentMapperMock.Object, receiptServiceMock.Object);

            var result = service.GetCashPayment(10);

            Assert.Same(expectedDto, result);
        }

        [Fact]
        public void GetCashPayment_LoadsPaymentByIdentifierFromRepository()
        {
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var paymentMapperMock = new Mock<ICashPaymentMapper>();
            var receiptServiceMock = new Mock<IReceiptService>();
            var stored = new Payment(10, 11, 12, 13, 14m, "CASH");
            paymentRepositoryMock.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifier(55)).Returns(stored);
            paymentMapperMock.Setup(cashPaymentMapper => cashPaymentMapper.TurnEntityIntoDataTransferObject(stored)).Returns(new CashPaymentDataTransferObject(1, 1, 1, 1, 1m));
            var service = new CashPaymentService(paymentRepositoryMock.Object, paymentMapperMock.Object, receiptServiceMock.Object);

            service.GetCashPayment(55);

            paymentRepositoryMock.Verify(paymentRepository => paymentRepository.GetPaymentByIdentifier(55), Times.Once);
        }

        [Fact]
        public void ConfirmDelivery_RecordsBuyerConfirmationOnPayment()
        {
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var paymentMapperMock = new Mock<ICashPaymentMapper>();
            var receiptServiceMock = new Mock<IReceiptService>();
            var payment = new Payment(1, 5, 2, 3, 10m, "CASH")
            {
                DateConfirmedSeller = null,
                DateConfirmedBuyer = null,
            };
            paymentRepositoryMock.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifier(1)).Returns(payment);
            var service = new CashPaymentService(paymentRepositoryMock.Object, paymentMapperMock.Object, receiptServiceMock.Object);

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
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var paymentMapperMock = new Mock<ICashPaymentMapper>();
            var receiptServiceMock = new Mock<IReceiptService>();
            var payment = new Payment(1, 5, 2, 3, 10m, "CASH")
            {
                DateConfirmedSeller = null,
                DateConfirmedBuyer = null,
            };
            paymentRepositoryMock.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifier(1)).Returns(payment);
            var service = new CashPaymentService(paymentRepositoryMock.Object, paymentMapperMock.Object, receiptServiceMock.Object);

            service.ConfirmDelivery(1);

            receiptServiceMock.Verify(receiptServiceMock => receiptServiceMock.GenerateReceiptRelativePath(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void ConfirmDelivery_WhenBothPartiesAlreadyConfirmed_GeneratesReceiptForRentalRequest()
        {
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var paymentMapperMock = new Mock<ICashPaymentMapper>();
            var receiptServiceMock = new Mock<IReceiptService>();
            var payment = new Payment(2, 42, 2, 3, 10m, "CASH")
            {
                DateConfirmedSeller = DateTime.Now.AddMinutes(-1),
                DateConfirmedBuyer = null,
            };
            paymentRepositoryMock.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifier(2)).Returns(payment);
            var service = new CashPaymentService(paymentRepositoryMock.Object, paymentMapperMock.Object, receiptServiceMock.Object);

            service.ConfirmDelivery(2);

            receiptServiceMock.Verify(receiptServiceMock => receiptServiceMock.GenerateReceiptRelativePath(42), Times.Once);
        }

        [Fact]
        public void ConfirmDelivery_PersistsUpdatedPayment()
        {
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var paymentMapperMock = new Mock<ICashPaymentMapper>();
            var receiptServiceMock = new Mock<IReceiptService>();
            var payment = new Payment(1, 5, 2, 3, 10m, "CASH");
            paymentRepositoryMock.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifier(1)).Returns(payment);
            var service = new CashPaymentService(paymentRepositoryMock.Object, paymentMapperMock.Object, receiptServiceMock.Object);

            service.ConfirmDelivery(1);

            paymentRepositoryMock.Verify(paymentRepository => paymentRepository.UpdatePayment(payment), Times.Once);
        }

        [Fact]
        public void ConfirmPayment_RecordsSellerConfirmationOnPayment()
        {
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var paymentMapperMock = new Mock<ICashPaymentMapper>();
            var receiptServiceMock = new Mock<IReceiptService>();
            var payment = new Payment(3, 8, 2, 3, 10m, "CASH")
            {
                DateConfirmedBuyer = null,
                DateConfirmedSeller = null,
            };
            paymentRepositoryMock.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifier(3)).Returns(payment);
            var service = new CashPaymentService(paymentRepositoryMock.Object, paymentMapperMock.Object, receiptServiceMock.Object);

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
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var paymentMapperMock = new Mock<ICashPaymentMapper>();
            var receiptServiceMock = new Mock<IReceiptService>();
            var payment = new Payment(3, 8, 2, 3, 10m, "CASH");
            paymentRepositoryMock.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifier(3)).Returns(payment);
            var service = new CashPaymentService(paymentRepositoryMock.Object, paymentMapperMock.Object, receiptServiceMock.Object);

            service.ConfirmPayment(3);

            receiptServiceMock.Verify(receiptServiceMock => receiptServiceMock.GenerateReceiptRelativePath(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void ConfirmPayment_WhenBothPartiesAlreadyConfirmed_GeneratesReceiptForRentalRequest()
        {
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var paymentMapperMock = new Mock<ICashPaymentMapper>();
            var receiptServiceMock = new Mock<IReceiptService>();
            var payment = new Payment(4, 71, 2, 3, 10m, "CASH")
            {
                DateConfirmedBuyer = DateTime.Now.AddMinutes(-1),
                DateConfirmedSeller = null,
            };
            paymentRepositoryMock.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifier(4)).Returns(payment);
            var service = new CashPaymentService(paymentRepositoryMock.Object, paymentMapperMock.Object, receiptServiceMock.Object);

            service.ConfirmPayment(4);

            receiptServiceMock.Verify(receiptServiceMock => receiptServiceMock.GenerateReceiptRelativePath(71), Times.Once);
        }

        [Fact]
        public void ConfirmPayment_PersistsUpdatedPayment()
        {
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var paymentMapperMock = new Mock<ICashPaymentMapper>();
            var receiptServiceMock = new Mock<IReceiptService>();
            var payment = new Payment(3, 8, 2, 3, 10m, "CASH");
            paymentRepositoryMock.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifier(3)).Returns(payment);
            var service = new CashPaymentService(paymentRepositoryMock.Object, paymentMapperMock.Object, receiptServiceMock.Object);

            service.ConfirmPayment(3);

            paymentRepositoryMock.Verify(paymentRepository => paymentRepository.UpdatePayment(payment), Times.Once);
        }

        [Fact]
        public void IsAllConfirmed_WhenBothConfirmationDatesAreSet_ReturnsTrue()
        {
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var paymentMapperMock = new Mock<ICashPaymentMapper>();
            var receiptServiceMock = new Mock<IReceiptService>();
            var payment = new Payment(5, 9, 2, 3, 10m, "CASH")
            {
                DateConfirmedBuyer = DateTime.Now,
                DateConfirmedSeller = DateTime.Now,
            };
            paymentRepositoryMock.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifier(5)).Returns(payment);
            var service = new CashPaymentService(paymentRepositoryMock.Object, paymentMapperMock.Object, receiptServiceMock.Object);

            var result = service.IsAllConfirmed(5);

            Assert.True(result);
        }

        [Fact]
        public void IsAllConfirmed_WhenBothConfirmationDatesAreSet_SetsStateToConfirmed()
        {
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var paymentMapperMock = new Mock<ICashPaymentMapper>();
            var receiptServiceMock = new Mock<IReceiptService>();
            var payment = new Payment(5, 9, 2, 3, 10m, "CASH")
            {
                DateConfirmedBuyer = DateTime.Now,
                DateConfirmedSeller = DateTime.Now,
            };
            paymentRepositoryMock.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifier(5)).Returns(payment);
            var service = new CashPaymentService(paymentRepositoryMock.Object, paymentMapperMock.Object, receiptServiceMock.Object);

            service.IsAllConfirmed(5);

            Assert.Equal(PaymentConstrants.StateConfirmed, payment.PaymentState);
        }

        [Fact]
        public void IsAllConfirmed_WhenSellerConfirmationIsMissing_ReturnsFalse()
        {
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var paymentMapperMock = new Mock<ICashPaymentMapper>();
            var receiptServiceMock = new Mock<IReceiptService>();
            var payment = new Payment(6, 9, 2, 3, 10m, "CASH")
            {
                DateConfirmedBuyer = DateTime.Now,
                DateConfirmedSeller = null,
            };
            paymentRepositoryMock.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifier(6)).Returns(payment);
            var service = new CashPaymentService(paymentRepositoryMock.Object, paymentMapperMock.Object, receiptServiceMock.Object);

            var result = service.IsAllConfirmed(6);

            Assert.False(result);
        }

        [Fact]
        public void IsDeliveryConfirmed_WhenBuyerConfirmationExists_ReturnsTrue()
        {
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var paymentMapperMock = new Mock<ICashPaymentMapper>();
            var receiptServiceMock = new Mock<IReceiptService>();
            var payment = new Payment(7, 9, 2, 3, 10m, "CASH")
            {
                DateConfirmedBuyer = DateTime.Now,
            };
            paymentRepositoryMock.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifier(7)).Returns(payment);
            var service = new CashPaymentService(paymentRepositoryMock.Object, paymentMapperMock.Object, receiptServiceMock.Object);

            Assert.True(service.IsDeliveryConfirmed(7));
        }

        [Fact]
        public void IsDeliveryConfirmed_WhenBuyerConfirmationIsMissing_ReturnsFalse()
        {
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var paymentMapperMock = new Mock<ICashPaymentMapper>();
            var receiptServiceMock = new Mock<IReceiptService>();
            var payment = new Payment(8, 9, 2, 3, 10m, "CASH")
            {
                DateConfirmedBuyer = null,
            };
            paymentRepositoryMock.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifier(8)).Returns(payment);
            var service = new CashPaymentService(paymentRepositoryMock.Object, paymentMapperMock.Object, receiptServiceMock.Object);

            Assert.False(service.IsDeliveryConfirmed(8));
        }

        [Fact]
        public void IsPaymentConfirmed_WhenSellerConfirmationExists_ReturnsTrue()
        {
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var paymentMapperMock = new Mock<ICashPaymentMapper>();
            var receiptServiceMock = new Mock<IReceiptService>();
            var payment = new Payment(9, 9, 2, 3, 10m, "CASH")
            {
                DateConfirmedSeller = DateTime.Now,
            };
            paymentRepositoryMock.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifier(9)).Returns(payment);
            var service = new CashPaymentService(paymentRepositoryMock.Object, paymentMapperMock.Object, receiptServiceMock.Object);

            Assert.True(service.IsPaymentConfirmed(9));
        }

        [Fact]
        public void IsPaymentConfirmed_WhenSellerConfirmationIsMissing_ReturnsFalse()
        {
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var paymentMapperMock = new Mock<ICashPaymentMapper>();
            var receiptServiceMock = new Mock<IReceiptService>();
            var payment = new Payment(10, 9, 2, 3, 10m, "CASH")
            {
                DateConfirmedSeller = null,
            };
            paymentRepositoryMock.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifier(10)).Returns(payment);
            var service = new CashPaymentService(paymentRepositoryMock.Object, paymentMapperMock.Object, receiptServiceMock.Object);

            Assert.False(service.IsPaymentConfirmed(10));
        }
    }
}
