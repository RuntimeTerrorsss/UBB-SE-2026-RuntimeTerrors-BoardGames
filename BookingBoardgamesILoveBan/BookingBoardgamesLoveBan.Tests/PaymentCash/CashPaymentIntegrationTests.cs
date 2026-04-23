using BookingBoardgamesILoveBan.Src.PaymentCash.Mapper;
using BookingBoardgamesILoveBan.Src.PaymentCash.Model;
using BookingBoardgamesILoveBan.Src.PaymentCash.Service;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Constants;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Repository;
using BookingBoardgamesILoveBan.Src.Mocks.GameMock;
using BookingBoardgamesILoveBan.Src.Mocks.RequestMock;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using BookingBoardgamesILoveBan.Src.Receipt.Service;

namespace BookingBoardgamesLoveBan.Tests.PaymentCash
{
    public class CashPaymentIntegrationTests
    {
        public CashPaymentIntegrationTests()
        {
            DatabaseBootstrap.Initialize();
        }

        [Fact]
        public void AddCashPayment_UsingPaymentCommonRepository_PersistsCashPaymentAndReturnsIdentifier()
        {
            var paymentRepository = new PaymentRepository();
            var cashPaymentService = BuildCashPaymentService(paymentRepository);
            var cashPaymentDataTransferObject = BuildCashPaymentDataTransferObject();
            var paymentIdentifier = -1;

            try
            {
                paymentIdentifier = cashPaymentService.AddCashPayment(cashPaymentDataTransferObject);
                var persistedPayment = paymentRepository.GetPaymentByIdentifier(paymentIdentifier);

                Assert.True(paymentIdentifier > 0);
                Assert.NotNull(persistedPayment);
                var expected = new
                {
                    cashPaymentDataTransferObject.RequestId,
                    cashPaymentDataTransferObject.ClientId,
                    cashPaymentDataTransferObject.OwnerId,
                    PaidAmount = decimal.Round(cashPaymentDataTransferObject.PaidAmount, 0),
                    PaymentMethod = "CASH",
                    PaymentState = PaymentConstrants.StateCompleted,
                };
                var actual = new
                {
                    persistedPayment!.RequestId,
                    persistedPayment.ClientId,
                    persistedPayment.OwnerId,
                    persistedPayment.PaidAmount,
                    persistedPayment.PaymentMethod,
                    persistedPayment.PaymentState,
                };

                Assert.Equal(expected, actual);
            }
            finally
            {
                DeletePaymentIfCreated(paymentRepository, paymentIdentifier);
            }
        }

        [Fact]
        public void GetCashPayment_AfterPersistingPayment_ReturnsMatchingDataTransferObject()
        {
            var paymentRepository = new PaymentRepository();
            var cashPaymentService = BuildCashPaymentService(paymentRepository);
            var cashPaymentDataTransferObject = BuildCashPaymentDataTransferObject();
            var paymentIdentifier = -1;

            try
            {
                paymentIdentifier = cashPaymentService.AddCashPayment(cashPaymentDataTransferObject);
                var retrievedCashPaymentDataTransferObject = cashPaymentService.GetCashPayment(paymentIdentifier);

                Assert.NotNull(retrievedCashPaymentDataTransferObject);
                var expected = new
                {
                    Id = paymentIdentifier,
                    cashPaymentDataTransferObject.RequestId,
                    cashPaymentDataTransferObject.ClientId,
                    cashPaymentDataTransferObject.OwnerId,
                    PaidAmount = decimal.Round(cashPaymentDataTransferObject.PaidAmount, 0),
                };
                var actual = new
                {
                    retrievedCashPaymentDataTransferObject.Id,
                    retrievedCashPaymentDataTransferObject.RequestId,
                    retrievedCashPaymentDataTransferObject.ClientId,
                    retrievedCashPaymentDataTransferObject.OwnerId,
                    retrievedCashPaymentDataTransferObject.PaidAmount,
                };

                Assert.Equal(expected, actual);
            }
            finally
            {
                DeletePaymentIfCreated(paymentRepository, paymentIdentifier);
            }
        }

        [Fact]
        public void ConfirmDelivery_AfterPersistingPayment_SetsBuyerConfirmationDate()
        {
            var paymentRepository = new PaymentRepository();
            var cashPaymentService = BuildCashPaymentService(paymentRepository);
            var paymentIdentifier = -1;

            try
            {
                paymentIdentifier = cashPaymentService.AddCashPayment(BuildCashPaymentDataTransferObject());
                cashPaymentService.ConfirmDelivery(paymentIdentifier);
                var persistedPayment = paymentRepository.GetPaymentByIdentifier(paymentIdentifier);

                Assert.NotNull(persistedPayment);
                Assert.NotNull(persistedPayment!.DateConfirmedBuyer);
                Assert.Null(persistedPayment.DateConfirmedSeller);
            }
            finally
            {
                DeletePaymentIfCreated(paymentRepository, paymentIdentifier);
            }
        }

        private static void DeletePaymentIfCreated(IPaymentRepository paymentRepository, int paymentIdentifier)
        {
            if (paymentIdentifier <= 0)
            {
                return;
            }

            paymentRepository.DeletePayment(new Payment { TransactionIdentifier = paymentIdentifier });
        }

        private static ICashPaymentService BuildCashPaymentService(IPaymentRepository paymentRepository)
        {
            var userRepository = new UserRepository();
            var gameRepository = new GameRepository();
            var requestRepository = new RequestRepository();
            var requestService = new RequestService(requestRepository, gameRepository);
            var receiptService = new ReceiptService(userRepository, requestService, gameRepository);
            var cashPaymentMapper = new CashPaymentMapper();

            return new CashPaymentService(paymentRepository, cashPaymentMapper, receiptService);
        }

        private static CashPaymentDataTransferObject BuildCashPaymentDataTransferObject()
        {
            return new CashPaymentDataTransferObject(
                paymentId: -1,
                requestId: 1,
                clientId: 1,
                ownerId: 2,
                amount: 20m);
        }
    }
}
