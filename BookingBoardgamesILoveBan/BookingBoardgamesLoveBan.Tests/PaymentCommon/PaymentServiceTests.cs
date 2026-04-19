using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.Mocks.GameMock;
using BookingBoardgamesILoveBan.Src.Mocks.RequestMock;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Repository;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Service;
using BookingBoardgamesILoveBan.Src.Receipt.Service;

namespace BookingBoardgamesLoveBan.Tests.PaymentCommon
{
    public class PaymentServiceTests // unit tests
    {
        // ================================ fakes ======================================
        private class FakePaymentRepository : IPaymentRepository
        {
            private readonly List<Payment> payments = new ();
            private int nextId = 1;

            public IReadOnlyList<Payment> GetAll()
            {
                return payments;
            }

            public Payment GetById(int tid)
            {
                foreach (Payment payment in payments)
                {
                    if (payment.Tid == tid)
                    {
                        return payment;
                    }
                }
                throw new Exception("No payment found");
            }

            public int AddPayment(Payment payment)
            {
                payment.Tid = nextId++;
                payments.Add(payment);
                return payment.Tid;
            }

            public bool DeletePayment(Payment payment)
            {
                int foundPaymentId = -1;
                for (int i = 0; i < payments.Count; i++)
                {
                    if (payments[i].Tid == payment.Tid)
                    {
                        foundPaymentId = i;
                        break;
                    }
                }
                if (foundPaymentId == -1)
                {
                    return false;
                }
                payments.RemoveAt(foundPaymentId);
                return true;
            }

            public Payment UpdatePayment(Payment updated)
            {
                var old = GetById(updated.Tid);
                if (old == null)
                {
                    return null;
                }

                var copy = new Payment
                {
                    Tid = old.Tid,
                    FilePath = old.FilePath,
                    RequestId = old.RequestId,
                    ClientId = old.ClientId,
                    OwnerId = old.OwnerId,
                    Amount = old.Amount,
                    PaymentMethod = old.PaymentMethod,
                    State = old.State
                };
                old.FilePath = updated.FilePath;
                old.DateOfTransaction = updated.DateOfTransaction;
                old.DateConfirmedBuyer = updated.DateConfirmedBuyer;
                old.DateConfirmedSeller = updated.DateConfirmedSeller;
                return copy;
            }
        }

        private class FakeReceiptService : IReceiptService
        {
            public string GenerateReceiptRelativePath(int requestId)
            {
                return $"receipts\\receipt_{requestId}_test.pdf";
            }

            public string GetReceiptDocument(Payment payment)
            {
                return $"C:\\Documents\\BookingBoardgames\\receipts\\receipt_{payment.RequestId}_test.pdf";
            }
        }

        private class FakePaymentService : PaymentService // abstract class
        {
            public FakePaymentService(IPaymentRepository repo, IReceiptService receiptService)
                : base(repo, receiptService)
            {
            }
        }

        // ================================ setup ======================================
        private readonly PaymentService paymentService;
        private readonly IPaymentRepository paymentRepository;
        private readonly IReceiptService receiptService;

        public PaymentServiceTests()
        {
            paymentRepository = new FakePaymentRepository();
            receiptService = new FakeReceiptService();
            paymentService = new FakePaymentService(paymentRepository, receiptService);
        }

        private Payment CreateAndAddPayment(string filePath = null)
        {
            var payment = new Payment
            {
                RequestId = 1,
                ClientId = 1,
                OwnerId = 2,
                Amount = 500,
                PaymentMethod = "Card",
                State = 0,
                FilePath = filePath
            };
            payment.Tid = paymentRepository.AddPayment(payment);
            return payment;
        }

        // ================================ GenerateReceipt ======================================
        [Fact]
        public void GenerateReceipt_SetsFilePathOnPayment()
        {
            var payment = CreateAndAddPayment();
            Assert.Null(payment.FilePath);

            paymentService.GenerateReceipt(payment.Tid);

            var updated = paymentRepository.GetById(payment.Tid);
            Assert.NotNull(updated.FilePath);
            Assert.NotEmpty(updated.FilePath);
        }

        [Fact]
        public void GenerateReceipt_FilePathContainsRequestId()
        {
            var payment = CreateAndAddPayment();

            paymentService.GenerateReceipt(payment.Tid);

            var updated = paymentRepository.GetById(payment.Tid);
            Assert.Contains(payment.RequestId.ToString(), updated.FilePath);
        }

        // ================================ GetReceipt ======================================
        [Fact]
        public void GetReceipt_PaymentAlreadyHasFilePath_ReturnsPath()
        {
            var payment = CreateAndAddPayment("receipts\\receipt_1_existing.pdf");

            var result = paymentService.GetReceipt(payment.Tid);

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetReceipt_PaymentHasNoFilePath_GeneratesAndReturnsPath()
        {
            var payment = CreateAndAddPayment(filePath: null);

            var result = paymentService.GetReceipt(payment.Tid);

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetReceipt_PaymentHasNoFilePath_SetsFilePathOnPayment()
        {
            var payment = CreateAndAddPayment(filePath: null);

            paymentService.GetReceipt(payment.Tid);

            var updated = paymentRepository.GetById(payment.Tid);
            Assert.NotNull(updated.FilePath);
            Assert.NotEmpty(updated.FilePath);
        }
    }
}


