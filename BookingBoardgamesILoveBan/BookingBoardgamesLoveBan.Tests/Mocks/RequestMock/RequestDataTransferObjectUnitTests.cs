using System;
using BookingBoardgamesILoveBan.Src.Mocks.RequestMock;
using Xunit;

namespace BookingBoardgamesLoveBan.Tests.Mocks.RequestMock
{
    public class RequestDataTransferObjectUnitTests
    {
        [Fact]
        public void RequestDataTransferObject_ValidParameters_SetsPropertiesCorrectly()
        {
            var start = new DateTime(2023, 1, 1);
            var end = new DateTime(2023, 1, 5);
            var dto = new RequestDataTransferObject(1, "Game", 2, 3, "Owner", "Client", start, end, 50.0m);

            var expected = new { Id = 1, GameName = "Game", ClientId = 2, OwnerId = 3, OwnerName = "Owner", ClientName = "Client", StartDate = start, EndDate = end, Price = 50.0m };
            var actual = new { dto.Id, dto.GameName, dto.ClientId, dto.OwnerId, dto.OwnerName, dto.ClientName, dto.StartDate, dto.EndDate, dto.Price };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RequestDataTransferObject_PropertyUpdates_SetsPropertiesCorrectly()
        {
            var start = new DateTime(2023, 1, 1);
            var end = new DateTime(2023, 1, 5);
            var dto = new RequestDataTransferObject(0, "", 0, 0, "", "", DateTime.MinValue, DateTime.MaxValue, 0m);

            dto.Id = 1;
            dto.GameName = "Game";
            dto.ClientId = 2;
            dto.OwnerId = 3;
            dto.OwnerName = "Owner";
            dto.ClientName = "Client";
            dto.StartDate = start;
            dto.EndDate = end;
            dto.Price = 50.0m;

            var expected = new { Id = 1, GameName = "Game", ClientId = 2, OwnerId = 3, OwnerName = "Owner", ClientName = "Client", StartDate = start, EndDate = end, Price = 50.0m };
            var actual = new { dto.Id, dto.GameName, dto.ClientId, dto.OwnerId, dto.OwnerName, dto.ClientName, dto.StartDate, dto.EndDate, dto.Price };

            Assert.Equal(expected, actual);
        }
    }
}
