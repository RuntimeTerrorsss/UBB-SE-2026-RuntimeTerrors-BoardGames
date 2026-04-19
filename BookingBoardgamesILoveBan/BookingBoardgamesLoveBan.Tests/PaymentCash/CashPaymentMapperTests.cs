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
            var cashPaymentDataTransferObject = new CashPaymentDto(7, 11, 13, 17, 19.5m);

            var paymentEntity = cashPaymentMapper.ToEntity(cashPaymentDataTransferObject);

            var expected = new
            {
                Tid = 7,
                RequestId = 11,
                ClientId = 13,
                OwnerId = 17,
                Amount = 19.5m,
                PaymentMethod = "CASH",
                State = PaymentConstrants.StatePending,
            };
            var actual = new
            {
                paymentEntity.Tid,
                paymentEntity.RequestId,
                paymentEntity.ClientId,
                paymentEntity.OwnerId,
                paymentEntity.Amount,
                paymentEntity.PaymentMethod,
                paymentEntity.State,
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToDataTransferObject_MapsPaymentToDataTransferObject()
        {
            var cashPaymentMapper = new CashPaymentMapper();
            var paymentEntity = new Payment(3, 5, 7, 9, 12.34m, "CASH");

            var cashPaymentDataTransferObject = cashPaymentMapper.ToDto(paymentEntity);

            var expected = new
            {
                Id = 3,
                Requestd = 5,
                ClientId = 7,
                OwnerId = 9,
                Amount = 12.34m,
            };
            var actual = new
            {
                cashPaymentDataTransferObject.Id,
                cashPaymentDataTransferObject.Requestd,
                cashPaymentDataTransferObject.ClientId,
                cashPaymentDataTransferObject.OwnerId,
                cashPaymentDataTransferObject.Amount,
            };

            Assert.Equal(expected, actual);
        }
    }
}
