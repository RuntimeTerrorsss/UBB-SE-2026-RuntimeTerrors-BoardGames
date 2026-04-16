using BookingBoardgamesILoveBan.Src.Mocks.GameMock;
using BookingBoardgamesILoveBan.Src.Mocks.RequestMock;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.Src.Receipt.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardgamesLoveBan.Tests.Receipt
{
    public class ReceiptServiceTests
    {
        private readonly ReceiptService receiptService;

        public ReceiptServiceTests()
        {
            receiptService = new ReceiptService(
                    new UserService(),
                    new RequestService(new GameService()),
                    new GameService()
                );
        }

        // ================================ GenerateReceiptRelativePath ======================================

        [Fact]
        public void GenerateReceiptRelativePath_ReturnsPathFolder()
        {
            var result = receiptService.GenerateReceiptRelativePath(1);
            Assert.StartsWith("receipts\\", result);
        }

        [Fact]
        public void GenerateReceiptRelativePath_ReturnsPathEndingWithPdf()
        {
            var result = receiptService.GenerateReceiptRelativePath(1);
            Assert.EndsWith(".pdf", result);
        }

        [Fact]
        public void GenerateReceiptRelativePath_ContainsRequestId()
        {
            var result = receiptService.GenerateReceiptRelativePath(1);
            Assert.Contains("1", result);
        }

        [Fact]
        public void GenerateReceiptRelativePath_SameIds()
        {
            var result1 = receiptService.GenerateReceiptRelativePath(1);
            System.Threading.Thread.Sleep(1000);
            var result2 = receiptService.GenerateReceiptRelativePath(1);

            Assert.NotEqual(result1, result2);
        }

        // ================================ GetReceiptDocument ======================================

        [Fact]
        public void GetReceiptDocument_NullFilePath()
        {
            var payment = new Payment { FilePath = null };
            Assert.Throws<InvalidOperationException>(() => receiptService.GetReceiptDocument(payment));
        }

        [Fact]
        public void GetReceiptDocument_EmptyFilePath()
        {
            var payment = new Payment { FilePath = string.Empty };
            Assert.Throws<InvalidOperationException>(() => receiptService.GetReceiptDocument(payment));
        }

        [Fact]
        public void GetReceiptDocument_ValidFilename_ParsesDate()
        {
            var payment = new Payment
            {
                RequestId = 1,
                ClientId = 1,
                OwnerId = 2,
                Amount = 100,
                PaymentMethod = "card",
                DateOfTransaction = DateTime.Now,
                FilePath = "receipts\\receipt_1_240101_123000.pdf"
            };

            var path = receiptService.GetReceiptDocument(payment);

            Assert.True(File.Exists(path));

            File.Delete(path);
        }
    }
}
