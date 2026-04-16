using BookingBoardgamesILoveBan.Src.PaymentCash.Mapper;
using BookingBoardgamesILoveBan.Src.PaymentCash.Model;

namespace BookingBoardgamesLoveBan.Tests.PaymentCash
{
    public class CashPaymentMapperTests
    {
        [Fact]
        public void ToEntity_MapsAllFields_AndUsesCashMethod()
        {
            var mapper = new CashPaymentMapper();
            var dto = new CashPaymentDto(7, 11, 13, 17, 19.5m);

            var entity = mapper.ToEntity(dto);

            Assert.Equal(7, entity.Tid);
            Assert.Equal(11, entity.RequestId);
            Assert.Equal(13, entity.ClientId);
            Assert.Equal(17, entity.OwnerId);
            Assert.Equal(19.5m, entity.Amount);
            Assert.Equal("CASH", entity.PaymentMethod);
        }

        [Fact]
        public void ToDto_MapsAllFields()
        {
            var mapper = new CashPaymentMapper();
            var payment = new BookingBoardgamesILoveBan.Src.PaymentCommon.Model.Payment(3, 5, 7, 9, 12.34m, "CASH");

            var dto = mapper.ToDto(payment);

            Assert.Equal(3, dto.Id);
            Assert.Equal(5, dto.Requestd);
            Assert.Equal(7, dto.ClientId);
            Assert.Equal(9, dto.OwnerId);
            Assert.Equal(12.34m, dto.Amount);
        }
    }
}
