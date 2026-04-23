using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.Src.PaymentCash.Mapper;
using BookingBoardgamesILoveBan.Src.PaymentCash.Service;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Repository;
using BookingBoardgamesILoveBan.Src.Receipt.Service;
using Moq;

namespace BookingBoardgamesLoveBan.Tests.PaymentCommon
{
    public class PaymentServiceTests
    {
        [Fact]
        public void GenerateReceipt_LoadsPaymentUsingProvidedIdentifier()
        {
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var receiptServiceMock = new Mock<IReceiptService>();
            var cashPaymentMapperMock = new Mock<ICashPaymentMapper>();
            paymentRepositoryMock
                .Setup(paymentRepository => paymentRepository.GetPaymentByIdentifier(25))
                .Returns(new Payment(25, 9, 2, 3, 10m, "CARD"));
            var cashPaymentService = new CashPaymentService(paymentRepositoryMock.Object, cashPaymentMapperMock.Object, receiptServiceMock.Object);

            cashPaymentService.GenerateReceipt(25);

            paymentRepositoryMock.Verify(paymentRepository => paymentRepository.GetPaymentByIdentifier(25), Times.Once);
        }

        [Fact]
        public void GenerateReceipt_AssignsGeneratedReceiptPathToPayment()
        {
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var receiptServiceMock = new Mock<IReceiptService>();
            var cashPaymentMapperMock = new Mock<ICashPaymentMapper>();
            var paymentEntity = new Payment(1, 42, 2, 3, 10m, "CARD") { ReceiptFilePath = null };
            paymentRepositoryMock.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifier(1)).Returns(paymentEntity);
            receiptServiceMock.Setup(receiptService => receiptService.GenerateReceiptRelativePath(42)).Returns("receipts/42.pdf");
            var cashPaymentService = new CashPaymentService(paymentRepositoryMock.Object, cashPaymentMapperMock.Object, receiptServiceMock.Object);

            cashPaymentService.GenerateReceipt(1);

            Assert.Equal("receipts/42.pdf", paymentEntity.ReceiptFilePath);
        }

        [Fact]
        public void GenerateReceipt_PersistsUpdatedPayment()
        {
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var receiptServiceMock = new Mock<IReceiptService>();
            var cashPaymentMapperMock = new Mock<ICashPaymentMapper>();
            var paymentEntity = new Payment(1, 42, 2, 3, 10m, "CARD");
            paymentRepositoryMock.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifier(1)).Returns(paymentEntity);
            receiptServiceMock.Setup(receiptService => receiptService.GenerateReceiptRelativePath(42)).Returns("receipts/42.pdf");
            var cashPaymentService = new CashPaymentService(paymentRepositoryMock.Object, cashPaymentMapperMock.Object, receiptServiceMock.Object);

            cashPaymentService.GenerateReceipt(1);

            paymentRepositoryMock.Verify(paymentRepository => paymentRepository.UpdatePayment(paymentEntity), Times.Once);
        }

        [Fact]
        public void GetReceipt_WhenPaymentAlreadyHasFilePath_DoesNotGenerateNewReceiptPath()
        {
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var receiptServiceMock = new Mock<IReceiptService>();
            var cashPaymentMapperMock = new Mock<ICashPaymentMapper>();
            var paymentEntity = new Payment(2, 50, 2, 3, 10m, "CARD") { ReceiptFilePath = "receipts/existing.pdf" };
            paymentRepositoryMock.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifier(2)).Returns(paymentEntity);
            receiptServiceMock.Setup(receiptService => receiptService.GetReceiptDocument(paymentEntity)).Returns("C:/docs/existing.pdf");
            var cashPaymentService = new CashPaymentService(paymentRepositoryMock.Object, cashPaymentMapperMock.Object, receiptServiceMock.Object);

            cashPaymentService.GetReceipt(2);

            receiptServiceMock.Verify(receiptService => receiptService.GenerateReceiptRelativePath(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void GetReceipt_WhenPaymentAlreadyHasFilePath_ReturnsReceiptDocumentFromReceiptService()
        {
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var receiptServiceMock = new Mock<IReceiptService>();
            var cashPaymentMapperMock = new Mock<ICashPaymentMapper>();
            var paymentEntity = new Payment(2, 50, 2, 3, 10m, "CARD") { ReceiptFilePath = "receipts/existing.pdf" };
            paymentRepositoryMock.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifier(2)).Returns(paymentEntity);
            receiptServiceMock.Setup(receiptService => receiptService.GetReceiptDocument(paymentEntity)).Returns("C:/docs/existing.pdf");
            var cashPaymentService = new CashPaymentService(paymentRepositoryMock.Object, cashPaymentMapperMock.Object, receiptServiceMock.Object);

            var receiptDocumentPath = cashPaymentService.GetReceipt(2);

            Assert.Equal("C:/docs/existing.pdf", receiptDocumentPath);
        }

        [Fact]
        public void GetReceipt_WhenFilePathIsMissing_GeneratesReceiptPathUsingPaymentRequestIdentifier()
        {
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var receiptServiceMock = new Mock<IReceiptService>();
            var cashPaymentMapperMock = new Mock<ICashPaymentMapper>();
            var paymentEntity = new Payment(3, 70, 2, 3, 10m, "CARD") { ReceiptFilePath = string.Empty };
            paymentRepositoryMock.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifier(3)).Returns(paymentEntity);
            receiptServiceMock.Setup(receiptService => receiptService.GenerateReceiptRelativePath(70)).Returns("receipts/70.pdf");
            receiptServiceMock.Setup(receiptService => receiptService.GetReceiptDocument(paymentEntity)).Returns("C:/docs/70.pdf");
            var cashPaymentService = new CashPaymentService(paymentRepositoryMock.Object, cashPaymentMapperMock.Object, receiptServiceMock.Object);

            cashPaymentService.GetReceipt(3);

            receiptServiceMock.Verify(receiptService => receiptService.GenerateReceiptRelativePath(70), Times.Once);
        }

        [Fact]
        public void GetReceipt_WhenFilePathIsMissing_RefreshesPaymentFromRepositoryAfterGeneratingReceipt()
        {
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var receiptServiceMock = new Mock<IReceiptService>();
            var cashPaymentMapperMock = new Mock<ICashPaymentMapper>();
            var paymentEntity = new Payment(3, 70, 2, 3, 10m, "CARD") { ReceiptFilePath = null };
            paymentRepositoryMock.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifier(3)).Returns(paymentEntity);
            receiptServiceMock.Setup(receiptService => receiptService.GenerateReceiptRelativePath(70)).Returns("receipts/70.pdf");
            receiptServiceMock.Setup(receiptService => receiptService.GetReceiptDocument(paymentEntity)).Returns("C:/docs/70.pdf");
            var cashPaymentService = new CashPaymentService(paymentRepositoryMock.Object, cashPaymentMapperMock.Object, receiptServiceMock.Object);

            cashPaymentService.GetReceipt(3);

            paymentRepositoryMock.Verify(paymentRepository => paymentRepository.GetPaymentByIdentifier(3), Times.Exactly(3));
        }

        [Fact]
        public void GetReceipt_WhenFilePathIsMissing_ReturnsReceiptDocumentForUpdatedPayment()
        {
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var receiptServiceMock = new Mock<IReceiptService>();
            var cashPaymentMapperMock = new Mock<ICashPaymentMapper>();
            var paymentEntity = new Payment(3, 70, 2, 3, 10m, "CARD") { ReceiptFilePath = null };
            paymentRepositoryMock.Setup(paymentRepository => paymentRepository.GetPaymentByIdentifier(3)).Returns(paymentEntity);
            receiptServiceMock.Setup(receiptService => receiptService.GenerateReceiptRelativePath(70)).Returns("receipts/70.pdf");
            receiptServiceMock.Setup(receiptService => receiptService.GetReceiptDocument(paymentEntity)).Returns("C:/docs/70.pdf");
            var cashPaymentService = new CashPaymentService(paymentRepositoryMock.Object, cashPaymentMapperMock.Object, receiptServiceMock.Object);

            var receiptDocumentPath = cashPaymentService.GetReceipt(3);

            Assert.Equal("C:/docs/70.pdf", receiptDocumentPath);
        }
    }
}
