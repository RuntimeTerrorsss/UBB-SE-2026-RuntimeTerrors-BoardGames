using BookingBoardgamesILoveBan.Src.PaymentCash.Mapper;
using BookingBoardgamesILoveBan.Src.PaymentCash.Model;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Constants;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;

namespace BookingBoardgamesLoveBan.Tests.PaymentCash
{
    public class CashPaymentMapperTests
    {
        [Fact]
        public void ToEntity_MapsDataTransferObjectToPaymentRowWithCashMethod()
        {
            var cashPaymentMapper = new CashPaymentMapper();
            var cashPaymentDataTransferObject = new CashPaymentDataTransferObject(7, 11, 13, 17, 19.5m);

            var paymentEntity = cashPaymentMapper.TurnDataTransferObjectIntoEntity(cashPaymentDataTransferObject);

            var expected = new
            {
                TransactionIdentifier = 7,
                RequestId = 11,
                ClientId = 13,
                OwnerId = 17,
                PaidAmount = 19.5m,
                PaymentMethod = "CASH",
                PaymentState = PaymentConstrants.StatePending,
            };
            var actual = new
            {
                paymentEntity.TransactionIdentifier,
                paymentEntity.RequestId,
                paymentEntity.ClientId,
                paymentEntity.OwnerId,
                paymentEntity.PaidAmount,
                paymentEntity.PaymentMethod,
                paymentEntity.PaymentState,
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToDataTransferObject_MapsPaymentToDataTransferObject()
        {
            var cashPaymentMapper = new CashPaymentMapper();
            var paymentEntity = new Payment(3, 5, 7, 9, 12.34m, "CASH");

            var cashPaymentDataTransferObject = cashPaymentMapper.TurnEntityIntoDataTransferObject(paymentEntity);

            var expected = new
            {
                Id = 3,
                RequestId = 5,
                ClientId = 7,
                OwnerId = 9,
                PaidAmount = 12.34m,
            };
            var actual = new
            {
                cashPaymentDataTransferObject.Id,
                cashPaymentDataTransferObject.RequestId,
                cashPaymentDataTransferObject.ClientId,
                cashPaymentDataTransferObject.OwnerId,
                cashPaymentDataTransferObject.PaidAmount,
            };

            Assert.Equal(expected, actual);
        }
    }
}
