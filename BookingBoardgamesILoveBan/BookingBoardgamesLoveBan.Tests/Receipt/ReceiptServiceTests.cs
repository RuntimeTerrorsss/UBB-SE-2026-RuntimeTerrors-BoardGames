using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.Delivery.Model;
using BookingBoardgamesILoveBan.Src.Mocks.GameMock;
using BookingBoardgamesILoveBan.Src.Mocks.RequestMock;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Enums;
using BookingBoardgamesILoveBan.Src.Receipt.Service;
using Windows.ApplicationModel.Payments;

namespace BookingBoardgamesLoveBan.Tests.Receipt
{
    public class ReceiptServiceTests
    {
        // ================================ fakes ======================================
        private class FakeUserService : IUserService
        {
            public User GetById(int id)
            {
                return new User(id, $"user_{id}", "country", "city", "str", "number"); 
            }
            public void SaveAddress(int id, Address address)
            {
            }
            public decimal GetUserBalance(int userId)
            {
                return 0;
            }
            public void UpdateBalance(int userId, decimal newBalance) { }
        }

        private class FakeGameService : IGameService
        {
            public Game GetById(int id)
            {
                return new Game(id, $"game_{id}", 100m);
            }
            public decimal GetPriceGameById(int gameId)
            {
                return 0;
            }
        }

        private class FakeRequestService : IRequestService
        {
            public Request GetById(int id) => new Request(
                id,
                1, 1, 2,
                DateTime.Now,
                DateTime.Now.AddDays(3));
            public decimal GetRequestPrice(int requestId)
            {
                return 0;
            }
            public string GetGameName(int requestId)
            {
                return "game_1";
            }
        }

        // ================================ setup ======================================
        private readonly ReceiptService receiptService;

        public ReceiptServiceTests()
        {
            receiptService = new ReceiptService(
                    new FakeUserService(),
                    new FakeRequestService(),
                    new FakeGameService());
        }

        private static string ToFullPath(string relativePath)
        {
            return System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "BookingBoardgames",
                relativePath.TrimStart('\\', '/'));
        }

        private Payment MakePayment(string relativePath, string paymentMethod)
        {
            return new Payment
            {
                FilePath = relativePath,
                RequestId = 1,
                ClientId = 1,
                OwnerId = 2,
                PaymentMethod = paymentMethod,
                Amount = 100,
                DateOfTransaction = DateTime.Now
            };
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
        public void GetReceiptDocument_NullFilePath_ThrowsException()
        {
            var payment = new Payment { FilePath = null };
            Assert.Throws<InvalidOperationException>(() => receiptService.GetReceiptDocument(payment));
        }

        [Fact]
        public void GetReceiptDocument_EmptyFilePath_ThrowsException()
        {
            var payment = new Payment { FilePath = string.Empty };
            Assert.Throws<InvalidOperationException>(() => receiptService.GetReceiptDocument(payment));
        }

        [Fact]
        public void GetReceiptDocument_FileExists_ReturnsPath()
        {
            string relativePath = $"receipts\\receipt_1_{DateTime.Now:yyMMdd_HHmmss}.pdf";
            string fullPath = ToFullPath(relativePath);

            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fullPath));
            System.IO.File.WriteAllBytes(fullPath, new byte[] { 0x25, 0x50, 0x44, 0x46 });

            var payment = new Payment { FilePath = relativePath };
            var result = receiptService.GetReceiptDocument(payment);

            Assert.Equal(fullPath, result);

            // clean
            System.IO.File.Delete(fullPath);
        }

        [Fact]
        public void GetReceiptDocument_InexistentFile_CardPayment_CreatesPdfAndReturnsPath()
        {
            string relativePath = $"receipts\\receipt_1_{DateTime.Now:yyMMdd_HHmmss}.pdf";
            string fullPath = ToFullPath(relativePath);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            var payment = MakePayment(relativePath, "card");
            var result = receiptService.GetReceiptDocument(payment);

            Assert.True(System.IO.File.Exists(result));
            Assert.EndsWith(".pdf", result);

            // clean
            System.IO.File.Delete(result);
        }

        [Fact]
        public void GetReceiptDocument_InexistentFile_CashPayment_CreatesPdfAndReturnsPath()
        {
            string relativePath = $"receipts\\receipt_1_{DateTime.Now:yyMMdd_HHmmss}.pdf";
            string fullPath = ToFullPath(relativePath);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            var payment = MakePayment(relativePath, "cash");
            var result = receiptService.GetReceiptDocument(payment);
            Assert.True(System.IO.File.Exists(result));

            // clean
            System.IO.File.Delete(result);
        }

        [Fact]
        public void GetReceiptDocument_InvalidFilename_FallsBackToTodayDate()
        {
            string relativePath = "receipts\\receipt_BADNAME.pdf";
            string fullPath = ToFullPath(relativePath);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            var payment = MakePayment(relativePath, "card");
            var result = receiptService.GetReceiptDocument(payment);

            Assert.True(System.IO.File.Exists(result));

            // clean
            System.IO.File.Delete(result);
        }
    }
}
